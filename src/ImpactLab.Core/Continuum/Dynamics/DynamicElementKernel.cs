using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Dynamics;

public static class DynamicElementKernel
{
    public static void Accumulate(
        TetrahedralMesh mesh,
        TetraElement e,
        ContinuumDynamicState state,
        PlasticityCatalog plasticity,
        double[] internalForce,
        double dt
    )
    {
        var nodes = e.Nodes();
        var B = TetraStrainDisplacement.Build(
            mesh.Nodes[nodes[0]].Position,
            mesh.Nodes[nodes[1]].Position,
            mesh.Nodes[nodes[2]].Position,
            mesh.Nodes[nodes[3]].Position
        );
        var u = new double[12];
        for (var i = 0; i < 4; i++)
        {
            var q = state.Displacement[nodes[i]];
            u[i * 3] = q.X;
            u[i * 3 + 1] = q.Y;
            u[i * 3 + 2] = q.Z;
        }
        var ev = new double[6];
        for (var i = 0; i < 6; i++)
        for (var j = 0; j < 12; j++)
            ev[i] += B[i, j] * u[j];
        var strain = new StrainTensor6(ev[0], ev[1], ev[2], ev[3], ev[4], ev[5]);
        var runtime = state.ElementStates[e.Id];
        var profile = plasticity.Find(e.Material.Id);
        StressTensor6 stress;
        double dissip = 0;
        if (profile is null)
        {
            stress = Elastic(e.Material, strain);
        }
        else
        {
            var law = profile.CreateLaw();
            var r = new J2ReturnMapper().Update(e.Material, strain, runtime.Plastic, law);
            stress = r.Stress;
            runtime.Plastic = r.State;
            dissip = r.DissipationDensityJPerM3 * TetraElementGeometry.Volume(mesh, e);
            runtime.DissipatedEnergyJ += dissip;
        }
        runtime.Stress = stress;
        runtime.TotalStrain = strain;
        var sv = new[] { stress.XX, stress.YY, stress.ZZ, stress.XY, stress.YZ, stress.ZX };
        var volume = TetraElementGeometry.Volume(mesh, e);
        for (var a = 0; a < 12; a++)
        {
            double f = 0;
            for (var k = 0; k < 6; k++)
                f += B[k, a] * sv[k];
            internalForce[nodes[a / 3] * 3 + a % 3] += f * volume;
        }
        runtime.InternalEnergyJ =
            0.5
            * volume
            * (
                stress.XX * strain.XX
                + stress.YY * strain.YY
                + stress.ZZ * strain.ZZ
                + stress.XY * strain.XY
                + stress.YZ * strain.YZ
                + stress.ZX * strain.ZX
            );
    }

    private static StressTensor6 Elastic(
        ImpactLab.Core.Materials.MaterialDefinition m,
        StrainTensor6 e
    )
    {
        var d = IsotropicElasticity.Matrix(m);
        var x = new[] { e.XX, e.YY, e.ZZ, e.XY, e.YZ, e.ZX };
        var s = new double[6];
        for (var i = 0; i < 6; i++)
        for (var j = 0; j < 6; j++)
            s[i] += d[i, j] * x[j];
        return new(s[0], s[1], s[2], s[3], s[4], s[5]);
    }
}
