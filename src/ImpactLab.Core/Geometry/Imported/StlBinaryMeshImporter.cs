using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public sealed class StlBinaryMeshImporter : IRawMeshImporter
{
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".stlbin", ".bstl" };

    public TriangleMeshAsset Import(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        if (fs.Length < 84)
            throw new InvalidDataException("Binary STL is too short.");
        br.ReadBytes(80);
        var count = br.ReadUInt32();
        if (84L + count * 50L > fs.Length)
            throw new InvalidDataException("Binary STL triangle count exceeds file length.");
        var vertices = new List<Vec3>((int)Math.Min(count * 3, int.MaxValue));
        var indices = new List<int>(vertices.Capacity);
        for (uint i = 0; i < count; i++)
        {
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            for (var v = 0; v < 3; v++)
            {
                vertices.Add(new Vec3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                indices.Add(vertices.Count - 1);
            }
            br.ReadUInt16();
        }
        return new TriangleMeshAsset(Path.GetFileNameWithoutExtension(path), vertices, indices);
    }
}
