using System.Diagnostics;

namespace ImpactLab.Core.Diagnostics;

public sealed class PerformanceTrace
{
    private readonly List<PerformanceSample> _samples = [];
    private readonly object _gate = new();
    public IReadOnlyList<PerformanceSample> Samples
    {
        get
        {
            lock (_gate)
                return _samples.ToArray();
        }
    }

    public IDisposable Measure(PerformanceStage stage, string name, long workUnits = 0) =>
        new Scope(this, stage, name, workUnits);

    public TimeSpan Total(PerformanceStage stage)
    {
        lock (_gate)
            return TimeSpan.FromTicks(
                _samples.Where(x => x.Stage == stage).Sum(x => x.Elapsed.Ticks)
            );
    }

    public IReadOnlyDictionary<PerformanceStage, TimeSpan> Totals()
    {
        lock (_gate)
            return _samples
                .GroupBy(x => x.Stage)
                .ToDictionary(g => g.Key, g => TimeSpan.FromTicks(g.Sum(x => x.Elapsed.Ticks)));
    }

    public IReadOnlyList<string> Evaluate(IEnumerable<PerformanceBudget> budgets)
    {
        var totals = Totals();
        var issues = new List<string>();
        foreach (var b in budgets)
        {
            b.Validate();
            var value = totals.GetValueOrDefault(b.Stage);
            if (b.HardLimit is not null && value > b.HardLimit)
                issues.Add($"{b.Stage} exceeded hard budget: {value.TotalMilliseconds:F1} ms");
            else if (value > b.SoftLimit)
                issues.Add($"{b.Stage} exceeded soft budget: {value.TotalMilliseconds:F1} ms");
        }
        return issues;
    }

    private sealed class Scope : IDisposable
    {
        private readonly PerformanceTrace _owner;
        private readonly PerformanceStage _stage;
        private readonly string _name;
        private readonly long _work;
        private readonly long _bytes;
        private readonly long _ticks;
        private readonly DateTimeOffset _started;
        private int _disposed;

        public Scope(PerformanceTrace owner, PerformanceStage stage, string name, long work)
        {
            _owner = owner;
            _stage = stage;
            _name = name;
            _work = work;
            _bytes = GC.GetTotalMemory(false);
            _ticks = Stopwatch.GetTimestamp();
            _started = DateTimeOffset.UtcNow;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            var elapsed = Stopwatch.GetElapsedTime(_ticks);
            var sample = new PerformanceSample(
                _stage,
                _name,
                elapsed,
                _work,
                GC.GetTotalMemory(false) - _bytes,
                _started
            );
            lock (_owner._gate)
                _owner._samples.Add(sample);
        }
    }
}
