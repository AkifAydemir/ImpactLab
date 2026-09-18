namespace ImpactLab.Core.Finalization;

public enum ReleaseGateKind
{
    FeatureCompleteness = 0,
    CanonicalExtraction = 1,
    Build = 2,
    Tests = 3,
    GoldenFixtures = 4,
    Documentation = 5,
    SampleProjects = 6,
    PerformanceBaseline = 7,
    SecurityReview = 8,
    AccessibilityReview = 9,
    NumericalVerification = 10,
    PersistenceCompatibility = 11,
    RuntimeIntegration = 12,
}
