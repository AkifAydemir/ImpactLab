using System.ComponentModel;
using System.Windows.Input;
using ImpactLab.App.Infrastructure;
using ImpactLab.Core.Mobility;

namespace ImpactLab.App.ViewModels;

public sealed class MobilityViewModel : INotifyPropertyChanged
{
    private TraversabilityAssessment? _assessment;
    private string _status = "Enter vehicle and terrain data, then run the screening assessment.";

    public MobilityViewModel()
    {
        AssessCommand = new RelayCommand(Assess);
        Assess();
    }

    public ICommand AssessCommand { get; }
    public string VehicleName { get; set; } = "Utility ground vehicle";
    public double MassKg { get; set; } = 3_500;
    public double ContactAreaM2 { get; set; } = 0.8;
    public double GroundClearanceM { get; set; } = 0.35;
    public double MaximumSlopeDeg { get; set; } = 35;
    public double MaximumStepM { get; set; } = 0.35;
    public double MaximumWaterDepthM { get; set; } = 0.6;
    public double TargetSpeedMps { get; set; } = 5;
    public double TerrainSlopeDeg { get; set; } = 12;
    public double TerrainRoughnessM { get; set; } = 0.04;
    public double ObstacleHeightM { get; set; } = 0.12;
    public double BearingStrengthKPa { get; set; } = 350;
    public double FrictionCoefficient { get; set; } = 0.65;
    public double WaterDepthM { get; set; }
    public double StructuralCapacityKn { get; set; }
    public int SyntheticDatasetRows { get; set; } = 2_000;
    public TraversabilityAssessment? Assessment
    {
        get => _assessment;
        private set
        {
            _assessment = value;
            Changed(nameof(Assessment));
            Changed(nameof(Reasons));
        }
    }
    public IReadOnlyList<string> Reasons => Assessment?.Reasons ?? [];
    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            Changed(nameof(Status));
        }
    }

    public void ExportTensorFlowDataset(string path)
    {
        var vehicle = Vehicle();
        var rows = TraversabilityDatasetGenerator.GenerateSynthetic(vehicle, SyntheticDatasetRows);
        TraversabilityDatasetExporter.Write(path, rows);
        Status =
            $"Exported {rows.Count:N0} TensorFlow-ready CSV rows and a schema file. "
            + "Synthetic rule labels must be augmented with measured outcomes before deployment.";
    }

    private void Assess()
    {
        try
        {
            Assessment = TraversabilityAssessor.Assess(Vehicle(), Terrain());
            Status =
                $"{Assessment.Status}: cost {Assessment.CostMultiplier:G4}, "
                + $"ground pressure {Assessment.GroundPressurePa / 1000:G4} kPa, "
                + $"confidence {Assessment.Confidence:P0}.";
        }
        catch (Exception ex)
        {
            Assessment = null;
            Status = $"Assessment failed: {ex.Message}";
        }
    }

    private VehicleMobilityProfile Vehicle() =>
        new(
            "ui-vehicle",
            VehicleName,
            MassKg,
            ContactAreaM2,
            GroundClearanceM,
            MaximumSlopeDeg,
            MaximumStepM,
            MaximumWaterDepthM,
            TargetSpeedMps
        );

    private TerrainCellObservation Terrain() =>
        new(
            0,
            0,
            0,
            TerrainSlopeDeg,
            TerrainRoughnessM,
            ObstacleHeightM,
            BearingStrengthKPa * 1000,
            FrictionCoefficient,
            WaterDepthM,
            StructuralCapacityKn * 1000
        );

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
}
