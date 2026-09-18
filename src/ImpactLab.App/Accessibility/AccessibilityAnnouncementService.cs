namespace ImpactLab.App.Accessibility;

public sealed class AccessibilityAnnouncementService
{
    public event EventHandler<LiveRegionMessage>? Announced;

    public void Announce(string text, bool assertive = false) =>
        Announced?.Invoke(
            this,
            new(Guid.NewGuid().ToString("N"), text, DateTimeOffset.UtcNow, assertive)
        );
}
