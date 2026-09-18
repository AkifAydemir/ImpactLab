namespace ImpactLab.App.Layout;

public static class WorkspaceLayoutMigration
{
    public static WorkspaceLayoutEnvelope Migrate(WorkspaceLayoutEnvelope e)
    {
        if (e.SchemaVersion == LayoutSchema.CurrentVersion)
            return e;
        if (e.SchemaVersion == 1)
            return e with
            {
                SchemaVersion = 2,
                Layout = e.Layout with
                {
                    Panes = e
                        .Layout.Panes.Select(
                            (p, i) => p with { Order = p.Order == 0 ? i : p.Order }
                        )
                        .ToArray(),
                },
            };
        throw new InvalidDataException($"Unsupported workspace layout schema {e.SchemaVersion}.");
    }
}
