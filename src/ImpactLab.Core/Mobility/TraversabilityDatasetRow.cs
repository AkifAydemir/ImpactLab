namespace ImpactLab.Core.Mobility;

public sealed record TraversabilityDatasetRow(
    VehicleMobilityProfile Vehicle,
    TerrainCellObservation Terrain,
    TraversabilityAssessment Assessment,
    string LabelSource = "physics-screening-v1"
);
