using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Rigid;

namespace ImpactLab.Core.Continuum.Contact;

public sealed class DynamicRigidSurfaceContact
{
    private readonly ContactHistoryStore _history = new();

    public DynamicSurfaceContactDiagnostics Apply(
        TetrahedralMesh mesh,
        ImpactLab.Core.Continuum.Dynamics.ContinuumDynamicState state,
        SurfaceMesh surface,
        IReadOnlyList<RigidBodyState> bodies,
        DynamicSurfaceContactSettings settings,
        double[] force,
        double dt
    )
    {
        settings.Validate();

        var active = new HashSet<ContactHistoryKey>();
        var pairs = 0;
        var maxPenetration = 0.0;
        double normalImpulse = 0;
        double frictionDissipation = 0;

        foreach (var body in bodies)
        {
            foreach (var triangle in surface.Triangles)
            {
                var a = mesh.Nodes[triangle.A].Position + state.Displacement[triangle.A];
                var b = mesh.Nodes[triangle.B].Position + state.Displacement[triangle.B];
                var c = mesh.Nodes[triangle.C].Position + state.Displacement[triangle.C];
                var center = (a + b + c) / 3.0;
                var sample = ImpactLab.Core.Contact.GeometryContact.Sample(
                    body.Definition.Geometry,
                    body.Transform,
                    center
                );

                if (sample.SignedDistanceMeters >= settings.SearchPaddingMeters)
                {
                    continue;
                }

                var penetration = Math.Max(0, -sample.SignedDistanceMeters);
                if (penetration <= 0)
                {
                    continue;
                }

                var velocity =
                    (
                        state.Velocity[triangle.A]
                        + state.Velocity[triangle.B]
                        + state.Velocity[triangle.C]
                    ) / 3.0
                    - body.VelocityAtWorldPoint(center);
                var key = new ContactHistoryKey(triangle.Id, body.Definition.Id);
                active.Add(key);

                var evaluation = PenaltyFrictionContactLaw.Evaluate(
                    sample.OutwardNormal,
                    penetration,
                    velocity,
                    _history.Get(key),
                    settings,
                    dt,
                    state.TimeSeconds
                );
                _history.Set(key, evaluation.State);

                var nodalForce = evaluation.Force / 3.0;
                foreach (var node in new[] { triangle.A, triangle.B, triangle.C })
                {
                    force[node * 3] += nodalForce.X;
                    force[node * 3 + 1] += nodalForce.Y;
                    force[node * 3 + 2] += nodalForce.Z;
                }

                body.Force -= evaluation.Force;
                body.Torque -= Vec3.Cross(center - body.WorldCenter, evaluation.Force);
                pairs++;
                maxPenetration = Math.Max(maxPenetration, penetration);
                normalImpulse += Math.Max(0, Vec3.Dot(evaluation.Force, sample.OutwardNormal)) * dt;
                frictionDissipation += evaluation.Dissipation;
            }
        }

        _history.RemoveMissing(active);
        return new(pairs, maxPenetration, normalImpulse, frictionDissipation, 0);
    }
}
