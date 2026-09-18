using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Calibration;

public sealed class CalibrationStudyDefinition
{
    public string Name { get; set; } = "Calibration study";
    public ScenarioDefinition BaseScenario { get; set; } = new();
    public List<CalibrationParameter> Parameters { get; } = [];
    public int MaxCandidates { get; set; } = 256;

    public void Validate()
    {
        BaseScenario.Validate();
        if (Parameters.Count == 0)
            throw new InvalidOperationException("Calibration study has no parameters.");
        foreach (var p in Parameters)
            p.Validate();
        if (MaxCandidates <= 0)
            throw new InvalidOperationException("MaxCandidates must be positive.");
    }
}
