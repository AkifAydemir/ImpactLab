using System.ComponentModel;
using ImpactLab.App.Services;
using ImpactLab.Core.Backends;

namespace ImpactLab.App.ViewModels;

public sealed class BackendViewModel : INotifyPropertyChanged
{
    private SimulationBackendDescriptor? _selected;

    public BackendViewModel(BackendCatalogService service)
    {
        Backends = service.Backends;
        Selected =
            Backends.FirstOrDefault(x =>
                x.Id.Equals(ExplicitLatticeBackend.BackendId, StringComparison.OrdinalIgnoreCase)
            ) ?? Backends.FirstOrDefault();
    }

    public IReadOnlyList<SimulationBackendDescriptor> Backends { get; }

    public SimulationBackendDescriptor? Selected
    {
        get => _selected;
        set
        {
            if (Equals(_selected, value))
                return;
            _selected = value;
            PropertyChanged?.Invoke(this, new(nameof(Selected)));
            PropertyChanged?.Invoke(this, new(nameof(VerificationState)));
        }
    }

    public string VerificationState =>
        Selected is null
            ? "No backend selected."
            : "Experimental — regression-tested; physical validation remains scenario-specific.";

    public event PropertyChangedEventHandler? PropertyChanged;
}
