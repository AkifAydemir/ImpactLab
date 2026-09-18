using System.ComponentModel;
using ImpactLab.Core.Calibration;

namespace ImpactLab.App.ViewModels;

public sealed class CalibrationViewModel : INotifyPropertyChanged
{
    private CalibrationStudyDefinition _definition = new();
    private CalibrationResult? _result;
    public CalibrationStudyDefinition Definition => _definition;
    public CalibrationResult? Result
    {
        get => _result;
        private set
        {
            _result = value;
            PropertyChanged?.Invoke(this, new(nameof(Result)));
        }
    }

    public IReadOnlyList<CalibrationCandidate> Preview() =>
        CalibrationPlanner.BuildGrid(_definition);

    public void SetResult(CalibrationResult result) => Result = result;

    public event PropertyChangedEventHandler? PropertyChanged;
}
