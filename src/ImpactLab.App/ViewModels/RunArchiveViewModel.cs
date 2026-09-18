using System.ComponentModel;
using ImpactLab.App.Services;
using ImpactLab.Core.Runs;

namespace ImpactLab.App.ViewModels;

public sealed class RunArchiveViewModel : INotifyPropertyChanged
{
    private readonly RunArchiveService _service;
    private IReadOnlyList<ArchivedRunRecord> _runs = [];
    private string _search = "";

    public RunArchiveViewModel(RunArchiveService service)
    {
        _service = service;
        Refresh();
    }

    public IReadOnlyList<ArchivedRunRecord> Runs
    {
        get => _runs;
        private set
        {
            _runs = value;
            PropertyChanged?.Invoke(this, new(nameof(Runs)));
        }
    }
    public string Search
    {
        get => _search;
        set
        {
            _search = value;
            Refresh();
            PropertyChanged?.Invoke(this, new(nameof(Search)));
        }
    }

    public void Refresh() =>
        Runs = _service.Query(
            new RunArchiveQuery(string.IsNullOrWhiteSpace(Search) ? null : Search)
        );

    public event PropertyChangedEventHandler? PropertyChanged;
}
