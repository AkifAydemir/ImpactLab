using ImpactLab.Core.Fracture;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Contact;

public sealed class CoarseFragmentContactSolver
{
    private readonly Dictionary<ContactKey, Vec3> _tangentialDisplacement = [];
    private readonly HashSet<ContactKey> _activeContacts = [];

    public int HistoryCount => _tangentialDisplacement.Count;

    public void BeginStep() => _activeContacts.Clear();

    public void EndStep()
    {
        foreach (
            var key in _tangentialDisplacement
                .Keys.Where(x => !_activeContacts.Contains(x))
                .ToArray()
        )
            _tangentialDisplacement.Remove(key);
    }

    public CoarseFragmentContactMetrics ApplyResolved(
        SimulationMesh mesh,
        SimulationState state,
        FragmentCoarseBodyState fragment,
        IReadOnlyList<int> surfaceNodes,
        string resolvedPartId,
        ContactSettings settings,
        DeformableContactRule rule,
        double timeStepSeconds
    )
    {
        if (!rule.Enabled)
            return Empty(fragment.Id, resolvedPartId);
        var accumulator = new ContactAccumulator();
        var stiffness = settings.NormalStiffnessNPerM * rule.StiffnessScale;
        var criticalStiffness = Math.Max(
            stiffness,
            settings.TangentialStiffnessNPerM * rule.StiffnessScale
        );
        var damping = settings.NormalDampingNsPerM * rule.DampingScale;
        for (var sampleIndex = 0; sampleIndex < fragment.Source.ContactSamples.Count; sampleIndex++)
        {
            var sample = fragment.Source.ContactSamples[sampleIndex];
            var fragmentPoint = fragment.ContactPointWorld(sample);
            var fragmentVelocity = fragment.VelocityAtWorldPoint(fragmentPoint);
            var fragmentRadius = sample.CellSizeMeters * settings.NodeRadiusScale;
            foreach (var nodeId in surfaceNodes)
            {
                if (!state.IsActive[nodeId])
                    continue;
                var nodePoint = state.Position[nodeId];
                var delta = nodePoint - fragmentPoint;
                var distance = delta.Length;
                var targetRadius =
                    fragmentRadius + mesh.Nodes[nodeId].CellSize * settings.NodeRadiusScale;
                var overlap = targetRadius - distance;
                if (overlap <= 0.0)
                    continue;
                var normal = distance <= 1e-12 ? Vec3.UnitZ : delta / distance;
                var relativeVelocity = state.Velocity[nodeId] - fragmentVelocity;
                var normalSpeed = Vec3.Dot(relativeVelocity, normal);
                var magnitude = stiffness * overlap - damping * normalSpeed;
                if (magnitude <= 0.0)
                    continue;
                var tangentialVelocity = relativeVelocity - normal * normalSpeed;
                var friction = CalculateFriction(
                    new(fragment.Id, sampleIndex, $"resolved:{resolvedPartId}", nodeId),
                    tangentialVelocity,
                    normal,
                    magnitude,
                    settings,
                    rule,
                    timeStepSeconds,
                    out var tangentialPotentialEnergy,
                    out var frictionDissipationEnergy
                );
                var forceOnNode = normal * magnitude + friction;
                var forceOnFragment = -forceOnNode;
                state.Force[nodeId] += forceOnNode;
                fragment.ApplyForceAtPoint(forceOnFragment, fragmentPoint);
                accumulator.Add(
                    overlap,
                    magnitude,
                    stiffness,
                    criticalStiffness,
                    damping,
                    normalSpeed,
                    friction,
                    frictionDissipationEnergy,
                    tangentialPotentialEnergy,
                    timeStepSeconds,
                    EffectiveMass(fragment.MassKg, mesh.Nodes[nodeId].MassKg),
                    (forceOnNode + forceOnFragment).Length
                );
            }
        }
        return accumulator.Result(fragment.Id, resolvedPartId);
    }

    public CoarseFragmentContactMetrics ApplyPair(
        FragmentCoarseBodyState a,
        FragmentCoarseBodyState b,
        ContactSettings settings,
        DeformableContactRule rule,
        double timeStepSeconds
    )
    {
        if (!rule.Enabled)
            return Empty(a.Id, b.Id);
        var accumulator = new ContactAccumulator();
        var stiffness = settings.NormalStiffnessNPerM * rule.StiffnessScale;
        var criticalStiffness = Math.Max(
            stiffness,
            settings.TangentialStiffnessNPerM * rule.StiffnessScale
        );
        var damping = settings.NormalDampingNsPerM * rule.DampingScale;
        for (var sampleAIndex = 0; sampleAIndex < a.Source.ContactSamples.Count; sampleAIndex++)
        {
            var sampleA = a.Source.ContactSamples[sampleAIndex];
            var pointA = a.ContactPointWorld(sampleA);
            var velocityA = a.VelocityAtWorldPoint(pointA);
            var radiusA = sampleA.CellSizeMeters * settings.NodeRadiusScale;
            for (var sampleBIndex = 0; sampleBIndex < b.Source.ContactSamples.Count; sampleBIndex++)
            {
                var sampleB = b.Source.ContactSamples[sampleBIndex];
                var pointB = b.ContactPointWorld(sampleB);
                var delta = pointA - pointB;
                var distance = delta.Length;
                var overlap =
                    radiusA + sampleB.CellSizeMeters * settings.NodeRadiusScale - distance;
                if (overlap <= 0.0)
                    continue;
                var normal = distance <= 1e-12 ? Vec3.UnitZ : delta / distance;
                var velocityB = b.VelocityAtWorldPoint(pointB);
                var relativeVelocity = velocityA - velocityB;
                var normalSpeed = Vec3.Dot(relativeVelocity, normal);
                var magnitude = stiffness * overlap - damping * normalSpeed;
                if (magnitude <= 0.0)
                    continue;
                var tangentialVelocity = relativeVelocity - normal * normalSpeed;
                var friction = CalculateFriction(
                    new(a.Id, sampleAIndex, b.Id, sampleBIndex),
                    tangentialVelocity,
                    normal,
                    magnitude,
                    settings,
                    rule,
                    timeStepSeconds,
                    out var tangentialPotentialEnergy,
                    out var frictionDissipationEnergy
                );
                var forceOnA = normal * magnitude + friction;
                var forceOnB = -forceOnA;
                a.ApplyForceAtPoint(forceOnA, pointA);
                b.ApplyForceAtPoint(forceOnB, pointB);
                accumulator.Add(
                    overlap,
                    magnitude,
                    stiffness,
                    criticalStiffness,
                    damping,
                    normalSpeed,
                    friction,
                    frictionDissipationEnergy,
                    tangentialPotentialEnergy,
                    timeStepSeconds,
                    EffectiveMass(a.MassKg, b.MassKg),
                    (forceOnA + forceOnB).Length
                );
            }
        }
        return accumulator.Result(a.Id, b.Id);
    }

    private Vec3 CalculateFriction(
        ContactKey key,
        in Vec3 tangentialVelocity,
        in Vec3 normal,
        double normalForceMagnitude,
        ContactSettings settings,
        DeformableContactRule rule,
        double timeStepSeconds,
        out double storedEnergyJ,
        out double dissipatedEnergyJ
    )
    {
        _activeContacts.Add(key);
        var speed = tangentialVelocity.Length;
        var stiffness = settings.TangentialStiffnessNPerM * rule.StiffnessScale;
        var damping = settings.TangentialDampingNsPerM * rule.DampingScale;
        var previous = _tangentialDisplacement.GetValueOrDefault(key);
        previous -= normal * Vec3.Dot(previous, normal);
        var trialDisplacement = previous + tangentialVelocity * timeStepSeconds;
        var trialForce = -trialDisplacement * stiffness - tangentialVelocity * damping;
        var frictionScale = Math.Max(0.0, rule.FrictionScale);
        var staticLimit = normalForceMagnitude * settings.StaticFrictionCoefficient * frictionScale;
        if (trialForce.Length <= staticLimit)
        {
            _tangentialDisplacement[key] = trialDisplacement;
            storedEnergyJ = 0.5 * stiffness * trialDisplacement.LengthSquared;
            dissipatedEnergyJ = damping * speed * speed * timeStepSeconds;
            return trialForce;
        }

        var direction =
            speed > 1e-12 ? tangentialVelocity / speed
            : trialForce.Length > 1e-12 ? -trialForce.Normalized()
            : Vec3.Zero;
        var slidingForce =
            -direction * normalForceMagnitude * settings.DynamicFrictionCoefficient * frictionScale;
        var elasticForce = slidingForce + tangentialVelocity * damping;
        var slidingDisplacement = -elasticForce / stiffness;
        _tangentialDisplacement[key] = slidingDisplacement;
        storedEnergyJ = 0.5 * stiffness * slidingDisplacement.LengthSquared;
        dissipatedEnergyJ =
            Math.Max(0.0, -Vec3.Dot(slidingForce, tangentialVelocity)) * timeStepSeconds
            + damping * speed * speed * timeStepSeconds;
        return slidingForce;
    }

    private static double EffectiveMass(double a, double b) => a * b / Math.Max(a + b, 1e-18);

    private static CoarseFragmentContactMetrics Empty(string a, string b) =>
        new(a, b, 0, 0, 0, 0, 0, 0, 0, 0, double.PositiveInfinity, 0);

    private readonly record struct ContactKey(string BodyA, int PointA, string BodyB, int PointB);

    private sealed class ContactAccumulator
    {
        private int _count;
        private double _maxOverlap;
        private double _normalForce;
        private double _tangentialForce;
        private double _frictionEnergy;
        private double _normalPenaltyEnergy;
        private double _tangentialPenaltyEnergy;
        private double _dampingEnergy;
        private double _criticalTimeStep = double.PositiveInfinity;
        private double _momentumResidual;

        public void Add(
            double overlap,
            double magnitude,
            double stiffness,
            double criticalStiffness,
            double damping,
            double normalSpeed,
            in Vec3 frictionForce,
            double frictionDissipationEnergy,
            double tangentialPotentialEnergy,
            double timeStepSeconds,
            double effectiveMass,
            double momentumResidual
        )
        {
            _count++;
            _maxOverlap = Math.Max(_maxOverlap, overlap);
            _normalForce += magnitude;
            _tangentialForce += frictionForce.Length;
            _frictionEnergy += frictionDissipationEnergy;
            _normalPenaltyEnergy += 0.5 * stiffness * overlap * overlap;
            _tangentialPenaltyEnergy += tangentialPotentialEnergy;
            _dampingEnergy += damping * normalSpeed * normalSpeed * timeStepSeconds;
            _criticalTimeStep = Math.Min(
                _criticalTimeStep,
                2.0 * Math.Sqrt(Math.Max(effectiveMass / criticalStiffness, 0.0))
            );
            _momentumResidual += momentumResidual;
        }

        public CoarseFragmentContactMetrics Result(string a, string b) =>
            new(
                a,
                b,
                _count,
                _maxOverlap,
                _normalForce,
                _tangentialForce,
                _frictionEnergy,
                _normalPenaltyEnergy,
                _tangentialPenaltyEnergy,
                _dampingEnergy,
                _criticalTimeStep,
                _momentumResidual
            );
    }
}
