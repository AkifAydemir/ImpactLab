using System.ComponentModel;
using ImpactLab.Core.Continuum.Dynamics;

namespace ImpactLab.App.ViewModels;

public sealed class TransientContinuumViewModel : INotifyPropertyChanged
{
    private ContinuumDynamicSettings _settings = new() { Enabled = true };
    public ContinuumDynamicSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            PropertyChanged?.Invoke(this, new(nameof(Settings)));
        }
    }
    public Array Integrators => Enum.GetValues<TimeIntegratorKind>();
    public Array MassModes => Enum.GetValues<MassMatrixMode>();
    public event PropertyChangedEventHandler? PropertyChanged;
}
