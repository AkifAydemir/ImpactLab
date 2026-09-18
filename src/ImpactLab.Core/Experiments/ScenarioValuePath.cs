using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Experiments;

public static class ScenarioValuePath
{
    public static void Apply(ScenarioDefinition scenario, string path, double value)
    {
        var tokens = path.Split(
            ':',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (tokens.Length < 2)
            throw new FormatException($"Invalid scenario path: {path}");
        switch (tokens[0].ToLowerInvariant())
        {
            case "scenario":
                ApplyScenario(scenario, tokens[1], value);
                break;
            case "settings":
                ApplySettings(scenario, tokens[1], value);
                break;
            case "part" when tokens.Length == 3:
                FindPart(scenario, tokens[1]).GeometryParameters.Set(tokens[2], value);
                break;
            case "rigid" when tokens.Length == 3:
                ApplyRigid(FindRigid(scenario, tokens[1]), tokens[2], value);
                break;
            default:
                throw new NotSupportedException($"Unsupported scenario path: {path}");
        }
    }

    private static void ApplyScenario(ScenarioDefinition s, string key, double v)
    {
        if (key.Equals("cellSizeMeters", StringComparison.OrdinalIgnoreCase))
            s.CellSizeMeters = v;
        else
            throw new NotSupportedException(key);
    }

    private static void ApplySettings(ScenarioDefinition s, string key, double v) =>
        s.Settings = key.ToLowerInvariant() switch
        {
            "timestepsseconds" => s.Settings with { TimeStepSeconds = v },
            "durationseconds" => s.Settings with { DurationSeconds = v },
            "globalvelocitydamping" => s.Settings with { GlobalVelocityDamping = v },
            _ => throw new NotSupportedException(key),
        };

    private static void ApplyRigid(ScenarioRigidBodyDefinition b, string key, double v)
    {
        switch (key.ToLowerInvariant())
        {
            case "speedz":
                b.InitialVelocity = new Vec3(b.InitialVelocity.X, b.InitialVelocity.Y, v);
                break;
            case "speedx":
                b.InitialVelocity = new Vec3(v, b.InitialVelocity.Y, b.InitialVelocity.Z);
                break;
            case "speedy":
                b.InitialVelocity = new Vec3(b.InitialVelocity.X, v, b.InitialVelocity.Z);
                break;
            default:
                b.GeometryParameters.Set(key, v);
                break;
        }
    }

    private static ScenarioPartDefinition FindPart(ScenarioDefinition s, string id) =>
        s.Parts.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private static ScenarioRigidBodyDefinition FindRigid(ScenarioDefinition s, string id) =>
        s.RigidBodies.First(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
