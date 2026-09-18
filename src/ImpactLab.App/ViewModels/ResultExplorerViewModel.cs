using System.ComponentModel;
using ImpactLab.App.Services;
using ImpactLab.Core.PostProcessing;

namespace ImpactLab.App.ViewModels;

public sealed class ResultExplorerViewModel : INotifyPropertyChanged
{
    private SimulationRunBundle? _run;
    private ResultFieldKind _field = ResultFieldKind.Damage;
    private int _frameIndex;
    private IReadOnlyList<NodeResultRecord> _rows = [];
    public IReadOnlyList<ResultFieldKind> Fields { get; } = Enum.GetValues<ResultFieldKind>();
    public ResultFieldKind Field
    {
        get => _field;
        set
        {
            _field = value;
            Refresh();
            PropertyChanged?.Invoke(this, new(nameof(Field)));
        }
    }
    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;
            Refresh();
            PropertyChanged?.Invoke(this, new(nameof(FrameIndex)));
        }
    }
    public IReadOnlyList<NodeResultRecord> Rows
    {
        get => _rows;
        private set
        {
            _rows = value;
            PropertyChanged?.Invoke(this, new(nameof(Rows)));
        }
    }

    public void Load(SimulationRunBundle run)
    {
        _run = run;
        _frameIndex = Math.Max(0, run.Result.Frames.Count - 1);
        Refresh();
        PropertyChanged?.Invoke(this, new(nameof(FrameIndex)));
    }

    private void Refresh()
    {
        Rows = _run is null
            ? []
            : ResultQueryEngine.Execute(
                _run.Compiled.Mesh,
                _run.Result,
                new ResultQuery(_frameIndex, _field, Limit: 10000)
            );
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
