using System.ComponentModel;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.App.ViewModels;

public sealed class LoadCaseViewModel : INotifyPropertyChanged
{
    private ScenarioLoadDefinition? _selected;
    public List<ScenarioLoadDefinition> Loads { get; } = [];
    public ScenarioLoadDefinition? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            PropertyChanged?.Invoke(this, new(nameof(Selected)));
        }
    }

    public void Add(ScenarioLoadKind kind)
    {
        var item = new ScenarioLoadDefinition { Kind = kind, Name = $"{kind} {Loads.Count + 1}" };
        Loads.Add(item);
        Selected = item;
        PropertyChanged?.Invoke(this, new(nameof(Loads)));
    }

    public void RemoveSelected()
    {
        if (Selected is null)
            return;
        Loads.Remove(Selected);
        Selected = Loads.LastOrDefault();
        PropertyChanged?.Invoke(this, new(nameof(Loads)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
