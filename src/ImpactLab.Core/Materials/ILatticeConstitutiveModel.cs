using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Materials;

public interface ILatticeConstitutiveModel
{
    LatticeConstitutiveResponse Evaluate(
        MeshSpring spring,
        double strain,
        double strainRatePerSecond,
        double timeStepSeconds
    );
}
