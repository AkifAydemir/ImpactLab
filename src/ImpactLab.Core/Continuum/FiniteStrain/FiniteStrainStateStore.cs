namespace ImpactLab.Core.Continuum.FiniteStrain;

public sealed class FiniteStrainStateStore
{
    private FiniteStrainElementState[] _committed = [];
    private FiniteStrainElementState[] _trial = [];
    public IReadOnlyList<FiniteStrainElementState> Committed => _committed;

    public void Initialize(int count, double t = 293.15)
    {
        _committed = Enumerable
            .Range(0, count)
            .Select(_ => FiniteStrainElementState.Initial(t))
            .ToArray();
        _trial = _committed.ToArray();
    }

    public void SetTrial(int i, FiniteStrainElementState state) => _trial[i] = state;

    public void Commit() => _committed = _trial.ToArray();

    public void Revert() => _trial = _committed.ToArray();
}
