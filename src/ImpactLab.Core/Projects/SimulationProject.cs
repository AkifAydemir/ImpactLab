using ImpactLab.Core.Contact;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scene;
using ImpactLab.Core.Simulation;

namespace ImpactLab.Core.Projects;

public sealed class SimulationProject
{
    public int FormatVersion { get; init; } = 1;
    public string Name { get; set; } = "Untitled ImpactLab Project";
    public SimulationScene Scene { get; } = new();
    public List<RigidBodyDefinition> RigidBodies { get; } = [];
    public List<ILoadSource> Loads { get; } = [];
    public SimulationSettings Settings { get; set; } = new();
    public ContactSettings ContactSettings { get; set; } = ContactSettings.Default;
    public ContactInteractionTable RigidContactRules { get; } = new();
    public DeformableContactTable DeformableContactRules { get; } = new();
}
