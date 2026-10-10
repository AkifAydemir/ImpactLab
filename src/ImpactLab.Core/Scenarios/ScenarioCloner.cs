using ImpactLab.Core.IO;

namespace ImpactLab.Core.Scenarios;

public static class ScenarioCloner
{
    public static ScenarioDefinition Clone(ScenarioDefinition source) =>
        ScenarioEnvelopeSerializer.Deserialize(ScenarioEnvelopeSerializer.Serialize(source));
}
