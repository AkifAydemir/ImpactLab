using ImpactLab.Core.Contact;
using ImpactLab.Core.Continuum;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Scenarios;

public sealed class ScenarioDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Untitled scenario";
    public double CellSizeMeters { get; set; } = 0.01;
    public MeshingSettings Meshing { get; set; } = new();
    public ContinuumAnalysisSettings Continuum { get; set; } = new();
    public SimulationSettings Settings { get; set; } = new();
    public ContactSettings ContactSettings { get; set; } = ContactSettings.Default;
    public List<ScenarioPartDefinition> Parts { get; } = [];
    public List<ScenarioRigidBodyDefinition> RigidBodies { get; } = [];
    public List<ScenarioContactDefinition> Contacts { get; } = [];
    public List<ScenarioLoadDefinition> Loads { get; } = [];
    public List<ScenarioBoundaryDefinition> Boundaries { get; } = [];
    public List<ScenarioProbeDefinition> Probes { get; } = [];
    public List<ScenarioThermalBoundaryDefinition> ThermalBoundaries { get; } = [];
    public List<ScenarioThermalSourceDefinition> ThermalSources { get; } = [];

    public void Validate()
    {
        if (CellSizeMeters <= 0)
            throw new InvalidOperationException("Cell size must be positive.");
        if (Meshing.BaseCellSizeMeters <= 0)
            Meshing = Meshing with { BaseCellSizeMeters = CellSizeMeters };
        Meshing.Validate();
        if (Parts.Count == 0)
            throw new InvalidOperationException("Scenario must contain at least one part.");
        Settings.Validate();
        ContactSettings.Validate();
        Continuum.Validate();
    }
}
