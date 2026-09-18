namespace ImpactLab.Core.Materials;

public sealed record MaterialCatalogEntry(
    MaterialDefinition Definition,
    string Category,
    string Notes,
    bool IsBuiltIn
);
