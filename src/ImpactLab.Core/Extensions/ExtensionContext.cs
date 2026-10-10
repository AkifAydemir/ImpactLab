using ImpactLab.Core.Backends;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Physics;

namespace ImpactLab.Core.Extensions;

public sealed class ExtensionContext
{
    private readonly List<Func<IEnumerable<MaterialProfile>>> _materialProviders = [];
    private readonly List<object> _reportContributors = [];

    public ExtensionContext(
        SimulationBackendRegistry backends,
        PhysicsModelRegistry? physicsModels = null
    )
    {
        Backends = backends;
        PhysicsModels = physicsModels ?? PhysicsModelRegistry.CreateDefault();
    }

    public SimulationBackendRegistry Backends { get; }
    public PhysicsModelRegistry PhysicsModels { get; }

    public void AddMaterialProvider(Func<IEnumerable<MaterialProfile>> provider) =>
        _materialProviders.Add(provider);

    public IEnumerable<MaterialProfile> EnumerateMaterials() =>
        _materialProviders.SelectMany(x => x());

    public void AddReportContributor(object contributor) => _reportContributors.Add(contributor);

    public IReadOnlyList<object> ReportContributors => _reportContributors;
}
