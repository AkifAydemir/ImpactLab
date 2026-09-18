using System.Diagnostics;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class FiniteStrainContinuumDriver
{
    private readonly IFiniteStrainConstitutiveLaw _law;

    public FiniteStrainContinuumDriver(IFiniteStrainConstitutiveLaw law) => _law = law;

    public FiniteStrainRunResult Run(
        ContinuumCompiledScenario scenario,
        ContinuumAnalysisSettings settings,
        CancellationToken cancellationToken = default
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var states = new FiniteStrainStateStore();
        states.Initialize(scenario.Mesh.Elements.Count);

        var positions = scenario.Mesh.Nodes.Select(node => node.Position).ToArray();
        var frames = new List<FiniteStrainRunFrame>
        {
            new(
                0,
                positions,
                Enumerable.Repeat(293.15, positions.Length).ToArray(),
                states.Committed,
                0,
                0
            ),
        };

        var timeStep = Math.Max(settings.ImplicitNonlinear.TimeStepSeconds, 1e-8);
        var stepCount = Math.Max(
            1,
            (int)Math.Ceiling(settings.ImplicitNonlinear.DurationSeconds / timeStep)
        );

        for (var step = 1; step <= stepCount; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double plasticDissipation = 0;
            double elasticEnergy = 0;

            for (var elementIndex = 0; elementIndex < scenario.Mesh.Elements.Count; elementIndex++)
            {
                var kinematics = new FiniteStrainKinematics(
                    DeformationGradient3.Identity,
                    new Matrix3(),
                    new Matrix3(),
                    new Matrix3(),
                    new Matrix3(),
                    1
                );
                var update = _law.Update(
                    new(
                        scenario.Mesh.Elements[elementIndex].Material,
                        states.Committed[elementIndex],
                        kinematics,
                        timeStep,
                        293.15
                    )
                );

                states.SetTrial(elementIndex, update.State);
                plasticDissipation += update.PlasticDissipationJ;
                elasticEnergy += update.ElasticEnergyJ;
            }

            states.Commit();
            if (step % Math.Max(1, settings.ImplicitNonlinear.OutputStride) == 0)
            {
                frames.Add(
                    new(
                        step * timeStep,
                        positions.ToArray(),
                        Enumerable.Repeat(293.15, positions.Length).ToArray(),
                        states.Committed.ToArray(),
                        elasticEnergy,
                        plasticDissipation
                    )
                );
            }
        }

        stopwatch.Stop();
        return new(
            scenario.Mesh,
            frames,
            stopwatch.Elapsed,
            [
                "Finite-strain driver is canonical architecture-and-source stage; repository verification remains pending.",
            ]
        );
    }
}
