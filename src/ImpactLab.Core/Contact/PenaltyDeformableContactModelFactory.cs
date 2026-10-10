namespace ImpactLab.Core.Contact;

public sealed class PenaltyDeformableContactModelFactory : IDeformableContactModelFactory
{
    public IDeformableContactStepSolver Create(double spatialCellSize) =>
        new DeformableContactSolver(spatialCellSize);
}
