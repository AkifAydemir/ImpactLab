namespace ImpactLab.Core.Extensions.Security;

public sealed class IpcReplayGuard
{
    private readonly Dictionary<string, DateTimeOffset> _seen = new(StringComparer.Ordinal);
    private readonly TimeSpan _retention;

    public IpcReplayGuard(TimeSpan? retention = null) =>
        _retention = retention ?? TimeSpan.FromMinutes(5);

    public bool TryAccept(string nonce, DateTimeOffset now)
    {
        foreach (
            var old in _seen.Where(x => now - x.Value > _retention).Select(x => x.Key).ToArray()
        )
            _seen.Remove(old);
        if (string.IsNullOrWhiteSpace(nonce) || _seen.ContainsKey(nonce))
            return false;
        _seen[nonce] = now;
        return true;
    }
}
