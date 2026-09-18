namespace ImpactLab.Core.Materials;

public static class MaterialLibrary
{
    private static readonly IReadOnlyList<MaterialDefinition> Items =
    [
        new(
            "generic-steel",
            "Generic Structural Steel",
            7_850.0,
            200e9,
            0.30,
            0.0020,
            0.120,
            0.025,
            MaterialResponseKind.Ductile,
            1.5e9,
            1.25,
            0.015
        ),
        new(
            "generic-aluminium",
            "Generic Aluminium Alloy",
            2_700.0,
            69e9,
            0.33,
            0.0030,
            0.150,
            0.020,
            MaterialResponseKind.Ductile,
            0.8e9,
            1.15,
            0.012
        ),
        new(
            "generic-ceramic",
            "Generic Engineering Ceramic",
            3_600.0,
            300e9,
            0.22,
            0.0008,
            0.006,
            0.012,
            MaterialResponseKind.Brittle,
            0.0,
            5.0,
            0.004
        ),
        new(
            "generic-polymer",
            "Generic Tough Polymer",
            1_150.0,
            2.5e9,
            0.38,
            0.020,
            0.450,
            0.060,
            MaterialResponseKind.Polymer,
            0.08e9,
            1.05,
            0.060
        ),
        new(
            "generic-elastomer",
            "Generic Elastomer",
            1_050.0,
            0.025e9,
            0.48,
            0.080,
            0.700,
            0.120,
            MaterialResponseKind.Polymer,
            0.005e9,
            1.0,
            0.090
        ),
    ];
    public static IReadOnlyList<MaterialDefinition> All => Items;

    public static MaterialDefinition Get(string id) =>
        Items.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Unknown material: {id}");
}
