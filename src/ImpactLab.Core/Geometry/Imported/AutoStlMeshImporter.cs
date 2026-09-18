namespace ImpactLab.Core.Geometry.Imported;

public sealed class AutoStlMeshImporter : IRawMeshImporter
{
    public IReadOnlyCollection<string> Extensions { get; } = new[] { ".stl" };

    public TriangleMeshAsset Import(string path)
    {
        using var fs = File.OpenRead(path);
        if (fs.Length >= 84)
        {
            fs.Position = 80;
            Span<byte> b = stackalloc byte[4];
            fs.ReadExactly(b);
            var count = BitConverter.ToUInt32(b);
            if (84L + count * 50L == fs.Length)
                return ImportBinary(path, count);
        }
        return new StlAsciiMeshImporter().Import(path);
    }

    private static TriangleMeshAsset ImportBinary(string path, uint count)
    {
        using var br = new BinaryReader(File.OpenRead(path));
        br.ReadBytes(84);
        var vertices = new List<ImpactLab.Core.Mathematics.Vec3>();
        var indices = new List<int>();
        for (uint i = 0; i < count; i++)
        {
            br.ReadBytes(12);
            for (var v = 0; v < 3; v++)
            {
                vertices.Add(new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                indices.Add(vertices.Count - 1);
            }
            br.ReadUInt16();
        }
        return new TriangleMeshAsset(Path.GetFileNameWithoutExtension(path), vertices, indices);
    }
}
