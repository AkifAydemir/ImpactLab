namespace ImpactLab.Core.Geometry.Exchange;

[Flags]
public enum CadExchangeCapability
{
    None = 0,
    Step = 1,
    Iges = 2,
    Brep = 4,
    SurfaceTessellation = 8,
    VolumeMeshing = 16,
}
