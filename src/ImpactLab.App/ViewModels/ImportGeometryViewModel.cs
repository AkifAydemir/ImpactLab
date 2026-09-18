using System.ComponentModel;
using ImpactLab.Core.Geometry.Imported;

namespace ImpactLab.App.ViewModels;

public sealed class ImportGeometryViewModel : INotifyPropertyChanged
{
    private readonly MeshImportService _service = new();
    private MeshImportResult? _result;
    private string _path = "";
    public string Path
    {
        get => _path;
        set
        {
            _path = value;
            PropertyChanged?.Invoke(this, new(nameof(Path)));
        }
    }
    public MeshImportResult? Result
    {
        get => _result;
        private set
        {
            _result = value;
            PropertyChanged?.Invoke(this, new(nameof(Result)));
        }
    }

    public void Import()
    {
        Result = _service.Import(Path);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
