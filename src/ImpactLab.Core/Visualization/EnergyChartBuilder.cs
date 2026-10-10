using ImpactLab.Core.PostProcessing;

namespace ImpactLab.Core.Visualization;

public static class EnergyChartBuilder
{
    public static ChartDocument Build(EnergyBalanceSeries energy)
    {
        var chart = new ChartDocument
        {
            Title = "Energy history",
            XLabel = "Time (s)",
            YLabel = "Energy (J)",
        };
        chart.Series.Add(
            new ChartSeries(
                "Mechanical",
                "s",
                "J",
                energy
                    .Samples.Select(x => new ChartPoint(x.TimeSeconds, x.MechanicalEnergyJ))
                    .ToArray()
            )
        );
        chart.Series.Add(
            new ChartSeries(
                "Contact potential",
                "s",
                "J",
                energy
                    .Samples.Select(x => new ChartPoint(x.TimeSeconds, x.ContactPotentialEnergyJ))
                    .ToArray()
            )
        );
        chart.Series.Add(
            new ChartSeries(
                "Dissipated",
                "s",
                "J",
                energy
                    .Samples.Select(x => new ChartPoint(
                        x.TimeSeconds,
                        x.FrictionEnergyJ
                            + x.ContactDampingEnergyJ
                            + x.StructuralDampingEnergyJ
                            + x.GlobalDampingEnergyJ
                            + x.PlasticDissipationEnergyJ
                            + x.FractureDissipationEnergyJ
                            + x.ThermalEnergyJ
                    ))
                    .ToArray()
            )
        );
        chart.Series.Add(
            new ChartSeries(
                "Accounted",
                "s",
                "J",
                energy
                    .Samples.Select(x => new ChartPoint(x.TimeSeconds, x.AccountedEnergyJ))
                    .ToArray()
            )
        );
        return chart;
    }
}
