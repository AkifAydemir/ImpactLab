namespace ImpactLab.Core.Contact;

public interface IDeformableContactModelFactory
{
    IDeformableContactStepSolver Create(double spatialCellSize);
}
