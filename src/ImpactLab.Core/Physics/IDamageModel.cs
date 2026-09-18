using ImpactLab.Core.Meshing;

namespace ImpactLab.Core.Physics;

public interface IDamageModel
{
    double UpdateDamage(
        MeshSpring spring,
        double currentDamage,
        double strain,
        double timeStepSeconds
    );
}
