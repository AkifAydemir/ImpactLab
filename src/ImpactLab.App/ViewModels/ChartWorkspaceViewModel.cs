using System.ComponentModel;
using ImpactLab.Core.Visualization;

namespace ImpactLab.App.ViewModels;

public sealed class ChartWorkspaceViewModel : INotifyPropertyChanged
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

    public void Load(ChartDocument document) => Document = document;

    public void ExportSvg(string path)
    {
        if (Document is null)
            throw new InvalidOperationException("No chart loaded.");
        SvgChartExporter.Save(path, Document);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
