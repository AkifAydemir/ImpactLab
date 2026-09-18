using ImpactLab.Core.Continuum;
using ImpactLab.Core.Continuum.Mechanics;
using ImpactLab.Core.Continuum.Static;

namespace ImpactLab.Core.Thermal;

public sealed record ThermoMechanicalStep(
    double TimeSeconds,
    double[] TemperatureKelvin,
    ContinuumStaticResult Mechanical
);

public sealed class ThermoMechanicalCoupler
{
    public IReadOnlyList<ThermoMechanicalStep> Run(
        TetrahedralMesh mesh,
        ThermalScenarioPackage package,
        double[] mechanicalLoad,
        IReadOnlyList<ContinuumDirichletDof> mechanicalConstraints,
        double duration,
        double dt,
        ContinuumStaticSettings staticSettings,
        CancellationToken ct = default
    )
    {
        var thermal = new ThermalState(mesh.Nodes.Count);
        var thermalSolver = new ImplicitThermalSolver();
        var mech = new LinearStaticContinuumSolver();
        var result = new List<ThermoMechanicalStep>();
        for (double time = dt; time <= duration + dt * 0.5; time += dt)
        {
            thermalSolver.Step(
                mesh,
                thermal,
                package.Properties,
                package.Boundaries,
                package.Sources,
                dt,
                ct
            );
            var rhs = (double[])mechanicalLoad.Clone();
            var tf = ThermalStrainLoad.Assemble(mesh, thermal, package.Properties);
            for (var i = 0; i < rhs.Length; i++)
                rhs[i] += tf[i];
            var s = mech.Solve(mesh, rhs, mechanicalConstraints, staticSettings, ct);
            result.Add(new(time, (double[])thermal.TemperatureKelvin.Clone(), s));
        }
        return result;
    }
}
