using System.ComponentModel;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.ViewModels;

public sealed class BoundaryConditionViewModel : INotifyPropertyChanged
{
    private ScenarioBoundaryDefinition? _selected;
    public List<ScenarioBoundaryDefinition> Boundaries { get; } = [];
    public ScenarioBoundaryDefinition? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            PropertyChanged?.Invoke(this, new(nameof(Selected)));
        }
    }

    public void Add(ScenarioBoundaryKind kind)
    {
        var item = new ScenarioBoundaryDefinition
        {
            Kind = kind,
            Name = $"{kind} {Boundaries.Count + 1}",
        };
        Boundaries.Add(item);
        Selected = item;
        PropertyChanged?.Invoke(this, new(nameof(Boundaries)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
