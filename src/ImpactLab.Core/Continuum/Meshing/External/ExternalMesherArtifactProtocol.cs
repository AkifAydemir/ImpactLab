using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImpactLab.Core.Geometry.Cad;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum.Meshing.External;

public static class ExternalMesherArtifactProtocol
{
    public const int SchemaVersion = 1;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static ExternalMesherRequestArtifact ToArtifact(ExternalTetraMesherRequest request)
    {
        request.Validate();
        static string Id(CadEntityId value) => value.Value.ToString("D");
        var model = request.Model;
        var vertices = model
            .Vertices.OrderBy(x => x.Id.Value)
            .Select(x => new ExternalCadVertexArtifact(
                Id(x.Id),
                x.Position.X,
                x.Position.Y,
                x.Position.Z,
                x.ToleranceMeters
            ))
            .ToArray();
        var edges = model
            .Edges.OrderBy(x => x.Id.Value)
            .Select(x => new ExternalCadEdgeArtifact(
                Id(x.Id),
                Id(x.StartVertex),
                Id(x.EndVertex),
                x.AdjacentFaces.Select(Id).OrderBy(v => v).ToArray(),
                x.IsSeam
            ))
            .ToArray();
        var faces = model
            .Faces.OrderBy(x => x.Id.Value)
            .Select(x => new ExternalCadFaceArtifact(
                Id(x.Id),
                x.BoundaryEdges.Select(Id).ToArray(),
                x.Reversed,
                x.SurfaceKind,
                x.AreaMeters2
            ))
            .ToArray();
        var bodies = model
            .Bodies.OrderBy(x => x.Id.Value)
            .Select(x => new ExternalCadBodyArtifact(
                Id(x.Id),
                x.Name,
                x.Faces.Select(Id).ToArray(),
                x.IsSolid,
                x.VolumeMeters3
            ))
            .ToArray();
        var m = request.Meshing;
        var meshing = new ExternalMeshingParametersArtifact(
            m.TargetSizeMeters,
            m.MinimumSizeMeters,
            m.MaximumSizeMeters,
            m.CurvatureFactor,
            m.GrowthRate,
            m.OptimizeSlivers,
            m.PreserveBoundary,
            request.BoundaryLayers
        );
        var options = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in request.Options)
            options[pair.Key] = pair.Value;
        return new(
            SchemaVersion,
            model.SourceFormat,
            vertices,
            edges,
            faces,
            bodies,
            meshing,
            options
        );
    }

    public static void WriteRequest(string path, ExternalTetraMesherRequest request) =>
        WriteJson(path, ToArtifact(request));

    public static ExternalMesherRequestArtifact ReadRequest(string path) =>
        ReadJson<ExternalMesherRequestArtifact>(path);

    public static void WriteResult(string path, ExternalMesherResultArtifact result) =>
        WriteJson(path, result);

    public static ExternalMesherResultArtifact ReadResult(string path) =>
        ReadJson<ExternalMesherResultArtifact>(path);

    public static TetrahedralMesh ToMesh(
        ExternalMesherResultArtifact result,
        MaterialDefinition material
    )
    {
        if (!result.Success)
            throw new InvalidDataException(result.Error ?? "External mesher reported failure.");
        if (result.SchemaVersion != SchemaVersion)
            throw new InvalidDataException(
                $"Unsupported external mesher result schema {result.SchemaVersion}."
            );
        var orderedNodes = result.Nodes.OrderBy(x => x.Id).ToArray();
        var map = orderedNodes
            .Select((n, i) => new { n.Id, Index = i })
            .ToDictionary(x => x.Id, x => x.Index);
        var nodes = orderedNodes
            .Select((n, i) => new TetraNode(i, new Vec3(n.X, n.Y, n.Z), n.PartId, material))
            .ToArray();
        var elements = result
            .Elements.OrderBy(x => x.Id)
            .Select(
                (e, i) =>
                    new TetraElement(
                        i,
                        Resolve(e.A),
                        Resolve(e.B),
                        Resolve(e.C),
                        Resolve(e.D),
                        e.PartId,
                        material
                    )
            )
            .ToArray();
        return new TetrahedralMesh(nodes, elements);
        int Resolve(int id) =>
            map.TryGetValue(id, out var index)
                ? index
                : throw new InvalidDataException(
                    $"External element references missing node id {id}."
                );
    }

    public static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void WriteJson<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var text = JsonSerializer.Serialize(value, Json) + "\n";
        File.WriteAllText(path, text, new UTF8Encoding(false));
    }

    private static T ReadJson<T>(string path)
    {
        var value = JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), Json);
        return value
            ?? throw new InvalidDataException($"Could not parse external mesher artifact {path}.");
    }
}
