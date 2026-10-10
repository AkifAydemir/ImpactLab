using ImpactLab.Core.Contact;
using ImpactLab.Core.Fracture;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Physics;

public sealed class PhysicsModelRegistry
{
    private sealed record Registration(
        PhysicsModelDescriptor Descriptor,
        Type Contract,
        Func<object> Factory
    );

    private readonly Dictionary<string, Registration> _registrations = new(
        StringComparer.OrdinalIgnoreCase
    );

    public void Register<TContract>(
        PhysicsModelDescriptor descriptor,
        Func<TContract> factory,
        bool replace = false
    )
        where TContract : class
    {
        descriptor.Validate();
        ArgumentNullException.ThrowIfNull(factory);
        if (!replace && _registrations.ContainsKey(descriptor.Id))
            throw new InvalidOperationException($"Duplicate physics model id: {descriptor.Id}");
        _registrations[descriptor.Id] = new(
            descriptor,
            typeof(TContract),
            () =>
                factory()
                ?? throw new InvalidOperationException(
                    $"Physics model factory returned null: {descriptor.Id}"
                )
        );
    }

    public TContract Create<TContract>(string id)
        where TContract : class
    {
        if (!_registrations.TryGetValue(id, out var registration))
            throw new KeyNotFoundException($"Unknown physics model: {id}");
        if (!typeof(TContract).IsAssignableFrom(registration.Contract))
            throw new InvalidOperationException(
                $"Physics model '{id}' implements {registration.Contract.Name}, not {typeof(TContract).Name}."
            );
        var model = registration.Factory();
        return model as TContract
            ?? throw new InvalidOperationException(
                $"Physics model factory '{id}' did not produce {typeof(TContract).Name}."
            );
    }

    public IReadOnlyList<PhysicsModelDescriptor> Describe() =>
        _registrations
            .Values.Select(x => x.Descriptor)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public IReadOnlyList<PhysicsModelDescriptor> ResolvePlan(IEnumerable<string> selectedIds)
    {
        var ordered = new List<PhysicsModelDescriptor>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in selectedIds.Distinct(StringComparer.OrdinalIgnoreCase))
            Visit(id, visiting, visited, ordered);
        return ordered;
    }

    private void Visit(
        string id,
        HashSet<string> visiting,
        HashSet<string> visited,
        List<PhysicsModelDescriptor> ordered
    )
    {
        if (visited.Contains(id))
            return;
        if (!visiting.Add(id))
            throw new InvalidOperationException(
                $"Physics model dependency cycle detected at: {id}"
            );
        if (!_registrations.TryGetValue(id, out var registration))
            throw new KeyNotFoundException($"Required physics model is not registered: {id}");
        foreach (var dependency in registration.Descriptor.RequiredModels)
            Visit(dependency, visiting, visited, ordered);
        visiting.Remove(id);
        visited.Add(id);
        ordered.Add(registration.Descriptor);
    }

    public static PhysicsModelRegistry CreateDefault()
    {
        var registry = new PhysicsModelRegistry();
        registry.Register<IDamageModel>(
            new(
                "damage.material-response-v1",
                "Material response damage",
                new Version(1, 0, 0),
                PhysicsModelCategory.Damage,
                PhysicsModelValidationLevel.ComponentVerified,
                "Strain-driven lattice damage with response-family-specific evolution rates."
            ),
            () => new MaterialResponseDamageModel()
        );
        registry.Register<IDamageModel>(
            new(
                "damage.strain-growth-v1",
                "Strain growth damage",
                new Version(1, 0, 0),
                PhysicsModelCategory.Damage,
                PhysicsModelValidationLevel.Experimental,
                "Simple strain-threshold damage law for controlled numerical studies."
            ),
            () => new StrainDamageModel()
        );
        registry.Register<IRigidContactModel>(
            new(
                ScenarioPhysicsConfiguration.DefaultRigidContactModelId,
                "Rigid penalty contact",
                new Version(1, 0, 0),
                PhysicsModelCategory.Contact,
                PhysicsModelValidationLevel.ComponentVerified,
                "Penalty normal contact with dashpot damping and regularized Coulomb friction."
            ),
            () => new PenaltyRigidContactModel()
        );
        registry.Register<IDeformableContactModelFactory>(
            new(
                ScenarioPhysicsConfiguration.DefaultDeformableContactModelId,
                "Deformable penalty contact",
                new Version(1, 0, 0),
                PhysicsModelCategory.Contact,
                PhysicsModelValidationLevel.Experimental,
                "Spatial-hash node contact using penalty stiffness and normal damping."
            ),
            () => new PenaltyDeformableContactModelFactory()
        );
        registry.Register<ILatticeConstitutiveModel>(
            new(
                ScenarioPhysicsConfiguration.DefaultLatticeConstitutiveModelId,
                "Legacy lattice elastic/bilinear response",
                new Version(1, 0, 0),
                PhysicsModelCategory.Constitutive,
                PhysicsModelValidationLevel.ComponentVerified,
                "Compatibility model matching the original stateless lattice force response."
            ),
            () => new LegacyLatticeConstitutiveModel()
        );
        registry.Register<ILatticeConstitutiveModel>(
            new(
                "constitutive.lattice-bilinear-plastic-v1",
                "Stateful bilinear lattice plasticity",
                new Version(1, 0, 0),
                PhysicsModelCategory.Constitutive,
                PhysicsModelValidationLevel.Experimental,
                "Uniaxial return mapping with permanent plastic strain and plastic-work tracking."
            ),
            () => new StatefulBilinearLatticePlasticityModel()
        );
        registry.Register<IFragmentTopologyModel>(
            new(
                ScenarioPhysicsConfiguration.DefaultFragmentTopologyModelId,
                "Connected lattice fragments",
                new Version(1, 0, 0),
                PhysicsModelCategory.Failure,
                PhysicsModelValidationLevel.ComponentVerified,
                "Deterministic connected-component topology over intact lattice links."
            ),
            () => new ConnectedLatticeFragmentTopologyModel()
        );
        return registry;
    }
}
