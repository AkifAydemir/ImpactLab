using System.ComponentModel;
using ImpactLab.Core.Results.Fields;

namespace ImpactLab.App.Visualization;

public sealed class ResultFieldSelectionService : INotifyPropertyChanged
{
    private ResultFieldDescriptor? _selected;
    public ResultFieldDescriptor? Selected
    {
        get => _selected;
        set
        {
            if (Equals(_selected, value))
                return;
            _selected = value;
            PropertyChanged?.Invoke(this, new(nameof(Selected)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
