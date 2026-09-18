using ImpactLab.Core.Selection;

namespace ImpactLab.Core.PostProcessing;

public sealed record ProbeDefinition(
    string Id,
    string Name,
    ProbeQuantity Quantity,
    NodeSelection Selection
);
