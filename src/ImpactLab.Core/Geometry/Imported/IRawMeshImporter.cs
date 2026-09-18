namespace ImpactLab.Core.Geometry.Imported;

public interface IRawMeshImporter
{
    IReadOnlyCollection<string> Extensions { get; }
    TriangleMeshAsset Import(string path);
}
