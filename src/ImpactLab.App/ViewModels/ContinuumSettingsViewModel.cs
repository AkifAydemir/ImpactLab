using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImpactLab.Core.Continuum;

namespace ImpactLab.App.ViewModels;

public sealed class ContinuumSettingsViewModel : INotifyPropertyChanged
{
    private ContinuumAnalysisSettings _value = new();
    public ContinuumAnalysisSettings Value
    {
        get => _value;
        set
        {
            _value = value;
            PropertyChanged?.Invoke(this, new(nameof(Value)));
        }
    }
    public Array Modes => Enum.GetValues<ContinuumSolveMode>();
    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetEdgeLengthMillimeters(double mm) =>
        Value = Value with { TargetEdgeLengthMeters = Math.Max(0.1, mm) / 1000.0 };

    public void EnableThermal(bool enabled) =>
        Value = Value with { EnableThermalCoupling = enabled };
}
