using System.Diagnostics;
using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Dynamics;
using ImpactLab.Core.Continuum.Nonlinear;
using ImpactLab.Core.Continuum.Plasticity;
using ImpactLab.Core.Continuum.Results;
using ImpactLab.Core.Continuum.Static;
using ImpactLab.Core.Sparse;

namespace ImpactLab.Core.Backends;

public sealed class TetraContinuumBackend : ISimulationBackend
{
    public const string BackendId = "tetra-continuum-v2";
    public SimulationBackendDescriptor Descriptor { get; } =
        new(
            BackendId,
            "Tetrahedral Continuum Mechanics",
            new Version(2, 2, 0),
            SimulationBackendCapabilities.TetrahedralContinuum
                | SimulationBackendCapabilities.SparseLinearAlgebra
                | SimulationBackendCapabilities.ThermalCoupling
                | SimulationBackendCapabilities.SurfaceTriangleContact
                | SimulationBackendCapabilities.TransientContinuum
                | SimulationBackendCapabilities.ElastoPlasticity
                | SimulationBackendCapabilities.ImplicitNonlinearTransient
                | SimulationBackendCapabilities.SelfContact
                | SimulationBackendCapabilities.LargeResultStorage,
            "Static, explicit transient and implicit nonlinear transient tetrahedral continuum backend with stateful plasticity and surface contact."
        );

    public BackendRunOutput Run(SimulationExecutionRequest request, CancellationToken ct = default)
    {
        var scenario =
            request.SourceScenario
            ?? throw new InvalidOperationException("Continuum backend requires SourceScenario.");
        var materials =
            request.Materials
            ?? throw new InvalidOperationException("Continuum backend requires MaterialCatalog.");
        scenario.Continuum.Validate();
        var compiled = ContinuumScenarioCompiler.Compile(scenario, materials);
        var constraints = ContinuumBoundaryCompiler.Compile(compiled.Mesh, scenario);
        var settings = scenario.Continuum;
        return settings.Mode switch
        {
            ContinuumSolveMode.ImplicitNonlinearTransient => RunImplicit(
                compiled,
                constraints,
                settings,
                ct
            ),
            ContinuumSolveMode.TransientDynamic => RunDynamic(request, compiled, constraints, ct),
            ContinuumSolveMode.IncrementalQuasiStatic => RunIncremental(
                compiled,
                constraints,
                settings,
                ct
            ),
            ContinuumSolveMode.LinearStatic => RunStatic(compiled, constraints, settings, ct),
            _ => throw new NotSupportedException(
                $"Solve mode {settings.Mode} is handled by a specialized continuum backend, not {BackendId}."
            ),
        };
    }

    private static BackendRunOutput RunDynamic(
        SimulationExecutionRequest request,
        ContinuumCompiledScenario compiled,
        IReadOnlyList<ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof> constraints,
        CancellationToken ct
    )
    {
        var result = new ContinuumTransientSolver().Solve(
            compiled.Mesh,
            compiled.Source,
            constraints,
            request.Compiled.RigidBodies,
            null,
            ct
        );
        return ContinuumBackendAdapters.FromDynamic(compiled, result);
    }

    private static BackendRunOutput RunImplicit(
        ContinuumCompiledScenario compiled,
        IReadOnlyList<ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof> constraints,
        ContinuumAnalysisSettings settings,
        CancellationToken ct
    )
    {
        var sw = Stopwatch.StartNew();
        var force = ContinuumLoadCompiler.Compile(compiled.Mesh, compiled.Source);
        var assembler = new StatefulNonlinearAssembler(new J2ReturnMapper());
        var linearSolver = SparseSolverRegistry
            .CreateDefault()
            .CreatePreferred(settings.PreferNativeSparse);
        var result = new ImplicitNonlinearTransientSolver(linearSolver).Solve(
            compiled.Mesh,
            settings.ImplicitNonlinear,
            assembler,
            _ => (double[])force.Clone(),
            constraints.Select(x => x.Dof).Distinct().OrderBy(x => x).ToArray(),
            ct
        );
        sw.Stop();
        return ContinuumBackendAdapters.FromImplicit(compiled, result, sw.Elapsed);
    }

    private static BackendRunOutput RunIncremental(
        ContinuumCompiledScenario compiled,
        IReadOnlyList<ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof> constraints,
        ContinuumAnalysisSettings settings,
        CancellationToken ct
    )
    {
        var sw = Stopwatch.StartNew();
        var rhs = ContinuumLoadCompiler.Compile(compiled.Mesh, compiled.Source);
        var linear = new ContinuumStaticSettings(PreferNative: settings.PreferNativeSparse);
        var nonlinear = new NonlinearContinuumSettings(
            settings.LoadSteps,
            settings.MaxNewtonIterations
        );
        var run = new IncrementalContinuumSolver().Solve(
            compiled.Mesh,
            rhs,
            constraints,
            linear,
            nonlinear,
            ct
        );
        if (run.Steps.Count == 0)
            throw new InvalidOperationException(
                "Incremental continuum solve produced no converged load step."
            );
        sw.Stop();
        return ContinuumBackendAdapters.FromStatic(compiled, run.Steps[^1], sw.Elapsed);
    }

    private static BackendRunOutput RunStatic(
        ContinuumCompiledScenario compiled,
        IReadOnlyList<ImpactLab.Core.Continuum.Mechanics.ContinuumDirichletDof> constraints,
        ContinuumAnalysisSettings settings,
        CancellationToken ct
    )
    {
        var sw = Stopwatch.StartNew();
        var rhs = ContinuumLoadCompiler.Compile(compiled.Mesh, compiled.Source);
        var solved = new LinearStaticContinuumSolver().Solve(
            compiled.Mesh,
            rhs,
            constraints,
            new ContinuumStaticSettings(PreferNative: settings.PreferNativeSparse),
            ct
        );
        sw.Stop();
        return ContinuumBackendAdapters.FromStatic(compiled, solved, sw.Elapsed);
    }
}
