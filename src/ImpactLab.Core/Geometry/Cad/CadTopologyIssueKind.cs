namespace ImpactLab.Core.Geometry.Cad;

public enum CadTopologyIssueKind
{
    OpenShell,
    NonManifoldEdge,
    DuplicateFace,
    ShortEdge,
    SmallFace,
    Gap,
    SelfIntersection,
    InvalidOrientation,
}
