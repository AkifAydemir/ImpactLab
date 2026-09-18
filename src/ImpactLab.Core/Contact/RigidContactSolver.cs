using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Contact;

public static class RigidContactSolver
{
    public static ContactPairMetrics Apply(
        SimulationMesh mesh,
        SimulationState state,
        RigidBodyState body,
        MeshPartRange targetPart,
        IReadOnlyList<int> surfaceNodes,
        ContactSettings settings,
        ContactPairRule rule,
        double timeStepSeconds
    )
    {
        if (!rule.Enabled)
            return Empty(body.Definition.Id, targetPart.PartId);
        var contactCount = 0;
        var maxPenetration = 0.0;
        var totalNormalForce = 0.0;
        var totalTangentialForce = 0.0;
        var frictionEnergy = 0.0;
        var stiffness = settings.NormalStiffnessNPerM * rule.NormalStiffnessScale;
        var damping = settings.NormalDampingNsPerM * rule.NormalDampingScale;
        foreach (var i in surfaceNodes)
        {
            var point = state.Position[i];
            var nodeRadius = mesh.Nodes[i].CellSize * settings.NodeRadiusScale;
            var sample = GeometryContact.Sample(body.Definition.Geometry, body.Transform, point);
            var penetration = nodeRadius - sample.SignedDistanceMeters;
            if (penetration <= 0.0)
                continue;
            var bodyPointVelocity = body.VelocityAtWorldPoint(point);
            var relativeVelocity = state.Velocity[i] - bodyPointVelocity;
            var normalSpeed = Vec3.Dot(relativeVelocity, sample.OutwardNormal);
            var normalForceMagnitude = stiffness * penetration - damping * normalSpeed;
            if (normalForceMagnitude <= 0.0)
                continue;
            var normalForce = sample.OutwardNormal * normalForceMagnitude;
            var tangentialVelocity = relativeVelocity - sample.OutwardNormal * normalSpeed;
            var tangentialForce = CalculateFriction(
                tangentialVelocity,
                normalForceMagnitude,
                settings,
                rule
            );
            var totalForce = normalForce + tangentialForce;
            state.Force[i] += totalForce;
            body.ApplyForceAtPoint(-totalForce, point);
            contactCount++;
            totalNormalForce += normalForceMagnitude;
            totalTangentialForce += tangentialForce.Length;
            maxPenetration = Math.Max(maxPenetration, penetration);
            frictionEnergy +=
                Math.Max(0.0, -Vec3.Dot(tangentialForce, tangentialVelocity)) * timeStepSeconds;
        }
        return new ContactPairMetrics(
            body.Definition.Id,
            targetPart.PartId,
            contactCount,
            maxPenetration,
            totalNormalForce,
            totalTangentialForce,
            frictionEnergy
        );
    }

    private static Vec3 CalculateFriction(
        in Vec3 tangentialVelocity,
        double normalForceMagnitude,
        ContactSettings settings,
        ContactPairRule rule
    )
    {
        var speed = tangentialVelocity.Length;
        if (speed <= 1e-12 || normalForceMagnitude <= 0.0)
            return Vec3.Zero;
        var direction = tangentialVelocity / speed;
        var staticLimit = normalForceMagnitude * settings.StaticFrictionCoefficient;
        var dynamicLimit = normalForceMagnitude * settings.DynamicFrictionCoefficient;
        var viscous = settings.TangentialDampingNsPerM * speed;
        var blend = Math.Clamp(speed / settings.TangentialSlipSpeedMPerSec, 0.0, 1.0);
        var coulomb = staticLimit + (dynamicLimit - staticLimit) * blend;
        var magnitude = Math.Min(coulomb, viscous + dynamicLimit * blend);
        return -direction * magnitude * Math.Max(0.0, rule.FrictionScale);
    }

    private static ContactPairMetrics Empty(string bodyId, string partId) =>
        new(bodyId, partId, 0, 0.0, 0.0, 0.0, 0.0);
}
