using System.ComponentModel;
using ImpactLab.App.Accessibility;
using ImpactLab.App.Results;
using ImpactLab.Core.Results.Storage;

namespace ImpactLab.App.Visualization;

public sealed class LargeResultWorkspaceViewModel : INotifyPropertyChanged, IAsyncDisposable
{
    private IAsyncResultFieldStore? _store;
    private AsyncFieldPrefetchService? _prefetch;
    private string? _field;
    private int _frame;
    private double[] _values = [];
    private string _status = "No result store attached";
    private CancellationTokenSource? _loadCts;
    private readonly AccessibilityAnnouncementService _a11y;

    public LargeResultWorkspaceViewModel(AccessibilityAnnouncementService a11y) => _a11y = a11y;

    public string? FieldId
    {
        get => _field;
        set
        {
            if (_field == value)
                return;
            _field = value;
            Changed(nameof(FieldId));
        }
    }
    public int FrameIndex
    {
        get => _frame;
        set
        {
            if (_frame == value)
                return;
            _frame = Math.Max(0, value);
            Changed(nameof(FrameIndex));
        }
    }
    public double[] Values
    {
        get => _values;
        private set
        {
            _values = value;
            Changed(nameof(Values));
            Changed(nameof(Minimum));
            Changed(nameof(Maximum));
        }
    }
    public double Minimum => Values.Length == 0 ? 0 : Values.Min();
    public double Maximum => Values.Length == 0 ? 0 : Values.Max();
    public bool IsAttached => _store is not null;
    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            Changed(nameof(Status));
        }
    }

    public void Attach(IAsyncResultFieldStore store)
    {
        _store = store;
        _prefetch = new(store, 2);
        Changed(nameof(IsAttached));
        Status = "Result store attached";
    }

    public async Task LoadAsync(IReadOnlyList<int> availableFrames, CancellationToken ct = default)
    {
        if (_store is null || _field is null)
            return;
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            Status = $"Loading {_field} frame {_frame}";
            Values = await _store.ReadAsync(_field, _frame, _loadCts.Token);
            Status = $"Loaded {Values.Length:N0} values";
            _a11y.Announce($"Loaded {_field} frame {_frame}, range {Minimum:G4} to {Maximum:G4}.");
            var plan = ResultPrefetchPlanner.Around(_field, _frame, availableFrames, 2);
            _ = _prefetch!.PrefetchAsync(plan, _loadCts.Token);
        }
        catch (OperationCanceledException)
        {
            Status = "Load cancelled";
        }
    }

    public async ValueTask DisposeAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        if (_store is not null)
            await _store.DisposeAsync();
    }

    private void Changed(string n) => PropertyChanged?.Invoke(this, new(n));

    public event PropertyChangedEventHandler? PropertyChanged;
}
