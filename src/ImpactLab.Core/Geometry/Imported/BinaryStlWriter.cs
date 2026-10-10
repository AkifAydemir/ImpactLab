using System.Text;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Geometry.Imported;

public static class BinaryStlWriter
{
    public static void Write(string path, TriangleMeshAsset mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new BinaryWriter(File.Create(path), Encoding.ASCII, false);
        var header = new byte[80];
        var label = Encoding.ASCII.GetBytes($"ImpactLab {mesh.Name}");
        Array.Copy(label, header, Math.Min(label.Length, header.Length));
        writer.Write(header);
        writer.Write((uint)mesh.TriangleCount);
        for (var i = 0; i < mesh.Indices.Count; i += 3)
        {
            var a = mesh.Vertices[mesh.Indices[i]];
            var b = mesh.Vertices[mesh.Indices[i + 1]];
            var c = mesh.Vertices[mesh.Indices[i + 2]];
            var cross = Vec3.Cross(b - a, c - a);
            var normal = cross.Length <= 1e-15 ? Vec3.Zero : cross.Normalized();
            Write(writer, normal);
            Write(writer, a);
            Write(writer, b);
            Write(writer, c);
            writer.Write((ushort)0);
        }
    }

    private static void Write(BinaryWriter writer, Vec3 value)
    {
        writer.Write((float)value.X);
        writer.Write((float)value.Y);
        writer.Write((float)value.Z);
    }
}
