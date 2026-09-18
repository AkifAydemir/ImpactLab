namespace ImpactLab.Core.Extensions.Security;

public sealed class ExtensionAuditLog
{
    private readonly List<ExtensionAuditEvent> _e = [];

    public void Add(ExtensionAuditEvent e) => _e.Add(e);

    public IReadOnlyList<ExtensionAuditEvent> Events => _e;
}
