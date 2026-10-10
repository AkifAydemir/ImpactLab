using System.Runtime.CompilerServices;
using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Materials;

public sealed class StatefulBilinearLatticePlasticityModel : ILatticeConstitutiveModel
{
    private sealed class SpringState
    {
        public double PlasticStrain;
        public double AccumulatedPlasticStrain;
    }

    private sealed class ReferenceComparer : IEqualityComparer<MeshSpring>
    {
        public static ReferenceComparer Instance { get; } = new();

        public bool Equals(MeshSpring? x, MeshSpring? y) => ReferenceEquals(x, y);

        public int GetHashCode(MeshSpring obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private readonly Dictionary<MeshSpring, SpringState> _states = new(ReferenceComparer.Instance);

    public LatticeConstitutiveResponse Evaluate(
        MeshSpring spring,
        double strain,
        double strainRatePerSecond,
        double timeStepSeconds
    )
    {
        var material = spring.Material;
        if (!_states.TryGetValue(spring, out var state))
            _states[spring] = state = new();
        var elasticModulus = material.YoungModulusPa;
        var requestedTangent = Math.Clamp(
            material.TangentModulusPa,
            0.0,
            elasticModulus * (1.0 - 1e-12)
        );
        var hardeningModulus =
            requestedTangent <= 0.0
                ? 0.0
                : elasticModulus * requestedTangent / (elasticModulus - requestedTangent);
        var trialStress = elasticModulus * (strain - state.PlasticStrain);
        var yieldStress =
            material.EffectiveYieldStrengthPa + hardeningModulus * state.AccumulatedPlasticStrain;
        var yieldFunction = Math.Abs(trialStress) - yieldStress;
        var stress = trialStress;
        var tangent = elasticModulus;
        var plasticIncrement = 0.0;
        if (yieldFunction > 0.0)
        {
            var direction = Math.Sign(trialStress);
            var plasticMultiplier = yieldFunction / (elasticModulus + hardeningModulus);
            state.PlasticStrain += plasticMultiplier * direction;
            state.AccumulatedPlasticStrain += plasticMultiplier;
            stress = trialStress - elasticModulus * plasticMultiplier * direction;
            tangent = requestedTangent;
            plasticIncrement = Math.Abs(stress) * plasticMultiplier;
        }
        var elasticStrain = strain - state.PlasticStrain;
        return new(
            stress,
            tangent,
            0.5 * elasticModulus * elasticStrain * elasticStrain,
            plasticIncrement
        );
    }
}
