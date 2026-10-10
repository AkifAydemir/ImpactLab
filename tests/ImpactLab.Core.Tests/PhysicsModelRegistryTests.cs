using ImpactLab.Core.Backends;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Meshing;
using ImpactLab.Core.Physics;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scenarios;
using ImpactLab.Core.Simulation;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class PhysicsModelRegistryTests
{
    [Fact]
    public void ResolvePlanOrdersDependenciesBeforeSelectedModel()
    {
        var registry = new PhysicsModelRegistry();
        registry.Register<IDamageModel>(Descriptor("base"), () => new StrainDamageModel());
        registry.Register<IDamageModel>(
            Descriptor("dependent", ["base"]),
            () => new MaterialResponseDamageModel()
        );

        var plan = registry.ResolvePlan(["dependent"]);

        Assert.Equal(["base", "dependent"], plan.Select(x => x.Id));
    }

    [Fact]
    public void ExplicitBackendUsesDamageModelSelectedByScenario()
    {
        var calls = 0;
        var registry = PhysicsModelRegistry.CreateDefault();
        registry.Register<IDamageModel>(
            Descriptor("damage.test-spy"),
            () => new CountingDamageModel(() => calls++)
        );
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.Settings = scenario.Settings with
        {
            DurationSeconds = scenario.Settings.TimeStepSeconds * 2,
            SnapshotStride = 1,
            TelemetryStride = 1,
        };
        scenario.Physics = new() { DamageModelId = "damage.test-spy" };
        var materials = new MaterialCatalog();
        var compiled = ScenarioCompiler.Compile(scenario, materials);

        new ExplicitLatticeBackend().Run(
            new(
                compiled,
                scenario.Settings,
                scenario.ContactSettings,
                scenario,
                materials,
                new SimulationExecutionContext { PhysicsModels = registry }
            )
        );

        Assert.True(calls > 0);
    }

    [Fact]
    public void RegistryRejectsDependencyCycles()
    {
        var registry = new PhysicsModelRegistry();
        registry.Register<IDamageModel>(Descriptor("a", ["b"]), () => new StrainDamageModel());
        registry.Register<IDamageModel>(Descriptor("b", ["a"]), () => new StrainDamageModel());

        var error = Assert.Throws<InvalidOperationException>(() => registry.ResolvePlan(["a"]));

        Assert.Contains("cycle", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExplicitBackendUsesRigidContactModelSelectedByScenario()
    {
        var calls = 0;
        var registry = PhysicsModelRegistry.CreateDefault();
        registry.Register<IRigidContactModel>(
            new(
                "contact.test-spy",
                "Contact test spy",
                new Version(1, 0, 0),
                PhysicsModelCategory.Contact,
                PhysicsModelValidationLevel.Experimental,
                "Test model."
            ),
            () => new CountingRigidContactModel(() => calls++)
        );
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.Settings = scenario.Settings with
        {
            DurationSeconds = scenario.Settings.TimeStepSeconds * 2,
            SnapshotStride = 1,
            TelemetryStride = 1,
        };
        scenario.Physics = new() { RigidContactModelId = "contact.test-spy" };
        var materials = new MaterialCatalog();
        var compiled = ScenarioCompiler.Compile(scenario, materials);

        new ExplicitLatticeBackend().Run(
            new(
                compiled,
                scenario.Settings,
                scenario.ContactSettings,
                scenario,
                materials,
                new SimulationExecutionContext { PhysicsModels = registry }
            )
        );

        Assert.True(calls > 0);
    }

    [Fact]
    public void StatefulLatticePlasticityRetainsPermanentStrainAndTracksPlasticWork()
    {
        var material = MaterialLibrary.Get("steel-s355jr");
        var spring = new MeshSpring("part", 0, 1, 1, 1, material);
        var model = new StatefulBilinearLatticePlasticityModel();

        var loaded = model.Evaluate(spring, 0.01, 1, 0.001);
        var unloaded = model.Evaluate(spring, 0, -1, 0.001);

        Assert.True(loaded.PlasticDissipationIncrementJPerM3 > 0);
        Assert.True(Math.Abs(loaded.StressPa) >= material.EffectiveYieldStrengthPa);
        Assert.True(unloaded.StressPa < 0);
        Assert.True(unloaded.ElasticEnergyDensityJPerM3 > 0);
    }

    [Fact]
    public void ExplicitBackendUsesConstitutiveModelSelectedByScenario()
    {
        var calls = 0;
        var registry = PhysicsModelRegistry.CreateDefault();
        registry.Register<ILatticeConstitutiveModel>(
            new(
                "constitutive.test-spy",
                "Constitutive test spy",
                new Version(1, 0, 0),
                PhysicsModelCategory.Constitutive,
                PhysicsModelValidationLevel.Experimental,
                "Test model."
            ),
            () => new CountingConstitutiveModel(() => calls++)
        );
        var scenario = ScenarioPresetLibrary.CreatePlateImpact();
        scenario.Settings = scenario.Settings with
        {
            DurationSeconds = scenario.Settings.TimeStepSeconds * 2,
            SnapshotStride = 1,
            TelemetryStride = 1,
        };
        scenario.Physics = new() { LatticeConstitutiveModelId = "constitutive.test-spy" };
        var materials = new MaterialCatalog();
        var compiled = ScenarioCompiler.Compile(scenario, materials);

        new ExplicitLatticeBackend().Run(
            new(
                compiled,
                scenario.Settings,
                scenario.ContactSettings,
                scenario,
                materials,
                new SimulationExecutionContext { PhysicsModels = registry }
            )
        );

        Assert.True(calls > 0);
    }

    private static PhysicsModelDescriptor Descriptor(
        string id,
        IReadOnlyList<string>? dependencies = null
    ) =>
        new(
            id,
            id,
            new Version(1, 0, 0),
            PhysicsModelCategory.Damage,
            PhysicsModelValidationLevel.Experimental,
            "Test model.",
            dependencies
        );

    private sealed class CountingDamageModel(Action onUpdate) : IDamageModel
    {
        public double UpdateDamage(
            MeshSpring spring,
            double currentDamage,
            double strain,
            double timeStepSeconds
        )
        {
            onUpdate();
            return currentDamage;
        }
    }

    private sealed class CountingRigidContactModel(Action onApply) : IRigidContactModel
    {
        private readonly PenaltyRigidContactModel _inner = new();

        public ContactPairMetrics Apply(
            SimulationMesh mesh,
            SimulationState state,
            RigidBodyState body,
            MeshPartRange targetPart,
            IReadOnlyList<int> surfaceNodes,
            ContactSettings settings,
            ContactPairRule rule,
            double timeStepSeconds
        )
        {
            onApply();
            return _inner.Apply(
                mesh,
                state,
                body,
                targetPart,
                surfaceNodes,
                settings,
                rule,
                timeStepSeconds
            );
        }
    }

    private sealed class CountingConstitutiveModel(Action onEvaluate) : ILatticeConstitutiveModel
    {
        private readonly LegacyLatticeConstitutiveModel _inner = new();

        public LatticeConstitutiveResponse Evaluate(
            MeshSpring spring,
            double strain,
            double strainRatePerSecond,
            double timeStepSeconds
        )
        {
            onEvaluate();
            return _inner.Evaluate(spring, strain, strainRatePerSecond, timeStepSeconds);
        }
    }
}
