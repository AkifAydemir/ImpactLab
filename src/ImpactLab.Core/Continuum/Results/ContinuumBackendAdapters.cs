using ImpactLab.Core.Backends;
using ImpactLab.Core.Continuum.Dynamics;
using ImpactLab.Core.Continuum.FiniteStrain;
using ImpactLab.Core.Continuum.Nonlinear;
using ImpactLab.Core.Continuum.Static;

namespace ImpactLab.Core.Continuum.Results;

public static class ContinuumBackendAdapters
{
    public static BackendRunOutput FromFiniteStrain(
        ContinuumCompiledScenario compiled,
        FiniteStrainRunResult result
    )
    {
        var compatibility = FiniteStrainResultAdapter.ToCompatibility(result);
        return new BackendRunOutput(
            compatibility,
            new ContinuumResultDomain(compiled.Mesh),
            result
        );
    }

    public static BackendRunOutput FromDynamic(
        ContinuumCompiledScenario compiled,
        ContinuumDynamicResult result
    )
    {
        var compatibility = ContinuumDynamicResultAdapter.ToCompatibility(result);
        return new BackendRunOutput(
            compatibility,
            new ContinuumResultDomain(compiled.Mesh),
            result
        );
    }

    public static BackendRunOutput FromStatic(
        ContinuumCompiledScenario compiled,
        ContinuumStaticResult result,
        TimeSpan computeTime
    )
    {
        var native = ContinuumResultFactory.FromStatic(compiled.Mesh, result, computeTime);
        return new BackendRunOutput(
            ContinuumResultAdapter.ToCompatibility(native),
            new ContinuumResultDomain(compiled.Mesh),
            native
        );
    }

    public static BackendRunOutput FromImplicit(
        ContinuumCompiledScenario compiled,
        NonlinearTransientResult result,
        TimeSpan computeTime
    )
    {
        var compatibility = NonlinearTransientResultAdapter.ToCompatibility(result, computeTime);
        return new BackendRunOutput(
            compatibility,
            new ContinuumResultDomain(compiled.Mesh),
            result
        );
    }
}
