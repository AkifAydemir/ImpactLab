using System.Text.Json.Nodes;

namespace ImpactLab.Core.IO;

public sealed class JsonMigrationPipeline
{
    private readonly List<IJsonMigrationStep> _steps = [];

    public void Register(IJsonMigrationStep step) => _steps.Add(step);

    public JsonObject Migrate(JsonObject root, string format, int targetMajor)
    {
        var current =
            root["SchemaMajor"]?.GetValue<int>() ?? root["schemaMajor"]?.GetValue<int>() ?? 1;
        var guard = 0;
        while (current < targetMajor)
        {
            if (++guard > 32)
                throw new InvalidOperationException("Migration pipeline exceeded safety limit.");
            var step =
                _steps.FirstOrDefault(x => x.Format == format && x.FromMajor == current)
                ?? throw new NotSupportedException(
                    $"No migration {format} v{current} -> target v{targetMajor}."
                );
            root = step.Apply(root);
            current = step.ToMajor;
            root["SchemaMajor"] = current;
        }
        return root;
    }
}
