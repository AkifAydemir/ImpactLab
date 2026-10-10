using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.PostProcessing;

public sealed record LatticeDerivedFields(
    double[] EquivalentStrain,
    double[] EquivalentStressPa,
    double[] YieldSafetyFactor
);

public static class LatticeDerivedFieldCalculator
{
    public static LatticeDerivedFields Evaluate(SimulationMesh mesh, SimulationFrame frame)
    {
        if (frame.Position.Length != mesh.Nodes.Count)
            throw new ArgumentException("Frame and mesh node counts do not match.", nameof(frame));

        var strain = new double[mesh.Nodes.Count];
        var stress = new double[mesh.Nodes.Count];
        var count = new int[mesh.Nodes.Count];
        foreach (var spring in mesh.Springs)
        {
            if (
                frame.NodeActive is not null
                && (!frame.NodeActive[spring.NodeA] || !frame.NodeActive[spring.NodeB])
            )
                continue;
            var currentLength = (
                frame.Position[spring.NodeB] - frame.Position[spring.NodeA]
            ).Length;
            var axialStrain =
                (currentLength - spring.RestLength) / Math.Max(spring.RestLength, 1e-15);
            var material = spring.Material;
            var absoluteStrain = Math.Abs(axialStrain);
            var absoluteStress =
                absoluteStrain <= material.EffectiveYieldStrain
                    ? material.YoungModulusPa * absoluteStrain
                    : material.EffectiveYieldStrengthPa
                        + material.TangentModulusPa
                            * (absoluteStrain - material.EffectiveYieldStrain);
            absoluteStress *= 1.0 - Math.Clamp(spring.Damage, 0.0, 1.0);

            Accumulate(spring.NodeA, absoluteStrain, absoluteStress);
            Accumulate(spring.NodeB, absoluteStrain, absoluteStress);
        }

        var safety = new double[mesh.Nodes.Count];
        for (var i = 0; i < mesh.Nodes.Count; i++)
        {
            if (count[i] > 0)
            {
                strain[i] /= count[i];
                stress[i] /= count[i];
            }
            safety[i] =
                stress[i] <= 1e-9
                    ? double.PositiveInfinity
                    : mesh.Nodes[i].Material.EffectiveYieldStrengthPa / stress[i];
        }
        return new(strain, stress, safety);

        void Accumulate(int nodeId, double axialStrain, double axialStress)
        {
            strain[nodeId] += axialStrain;
            stress[nodeId] += axialStress;
            count[nodeId]++;
        }
    }
}
