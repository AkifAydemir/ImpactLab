using ImpactLab.Core.Thermal;

namespace ImpactLab.Core.Continuum.Mechanics;

public static class ThermalStrainLoad
{
    public static double[] Assemble(
        TetrahedralMesh mesh,
        ThermalState thermal,
        ThermalPropertyTable properties,
        double referenceKelvin = 293.15
    )
    {
        var f = new double[mesh.Nodes.Count * 3];
        foreach (var e in mesh.Elements)
        {
            var p = properties.Resolve(e.Material.Id);
            var avg = e.Nodes().Average(n => thermal.TemperatureKelvin[n]);
            var eps = p.ThermalExpansionPerKelvin * (avg - referenceKelvin);
            if (Math.Abs(eps) < 1e-18)
                continue;
            var a = mesh.Nodes[e.A].Position;
            var b = mesh.Nodes[e.B].Position;
            var c = mesh.Nodes[e.C].Position;
            var d = mesh.Nodes[e.D].Position;
            var B = TetraStrainDisplacement.Build(a, b, c, d);
            var D = IsotropicElasticity.Matrix(e.Material);
            var eth = new[] { eps, eps, eps, 0.0, 0.0, 0.0 };
            var stress = new double[6];
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 6; j++)
                stress[i] += D[i, j] * eth[j];
            var vol = Math.Abs(TetraElementGeometry.SignedVolume(a, b, c, d));
            var dofs = new ContinuumDofMap(mesh.Nodes.Count).ElementDofs(e);
            for (var j = 0; j < 12; j++)
            {
                double q = 0;
                for (var i = 0; i < 6; i++)
                    q += B[i, j] * stress[i];
                f[dofs[j]] -= q * vol;
            }
        }
        return f;
    }
}
