namespace ImpactLab.Core.Continuum.Plasticity;

public sealed class ContinuumConstitutiveStateStore
{
    private readonly PlasticState[] _states;

    public ContinuumConstitutiveStateStore(int elementCount) =>
        _states = new PlasticState[elementCount];

    public PlasticState this[int index]
    {
        get => _states[index];
        set => _states[index] = value;
    }

    public PlasticState[] Snapshot() => _states.ToArray();

    public void Restore(ReadOnlySpan<PlasticState> states)
    {
        if (states.Length != _states.Length)
            throw new ArgumentException(
                "State count must match the element count.",
                nameof(states)
            );
        states.CopyTo(_states);
    }
}
