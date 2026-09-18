using System.ComponentModel;
using ImpactLab.Core.Visualization;

namespace ImpactLab.App.ViewModels;

public sealed class ChartViewModel : INotifyPropertyChanged
{
    private ChartDocument? _document;
    public ChartDocument? Document
    {
        get => _document;
        private set
        {
            _document = value;
            PropertyChanged?.Invoke(this, new(nameof(Document)));
        }
    }
    public LinkedCursorState Cursor { get; } = new();

    public void Load(ChartDocument document) => Document = document;

    public event PropertyChangedEventHandler? PropertyChanged;
}
