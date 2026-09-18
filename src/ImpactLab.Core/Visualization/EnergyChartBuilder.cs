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
                "Friction",
                "s",
                "J",
                energy
                    .Samples.Select(x => new ChartPoint(x.TimeSeconds, x.FrictionEnergyJ))
                    .ToArray()
            )
        );
        return chart;
    }
}
