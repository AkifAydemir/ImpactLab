using ImpactLab.Core.Continuum.Meshing.External;

namespace ImpactLab.MeshAdapter.Sample;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine(
                "usage: ImpactLab.MeshAdapter.Sample <request.json> <result.json>"
            );
            return 2;
        }
        try
        {
            var request = ExternalMesherArtifactProtocol.ReadRequest(args[0]);
            if (request.SchemaVersion != ExternalMesherArtifactProtocol.SchemaVersion)
                throw new InvalidDataException(
                    $"Unsupported request schema {request.SchemaVersion}."
                );
            if (request.Vertices.Count != 4)
                throw new InvalidDataException(
                    "The sample adapter intentionally accepts exactly four CAD vertices and emits one tetrahedron."
                );
            var vertices = request.Vertices.OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            var nodes = vertices
                .Select((v, i) => new ExternalMeshNodeArtifact(i, v.X, v.Y, v.Z, "sample-cad"))
                .ToArray();
            var element = new ExternalMeshElementArtifact(0, 0, 1, 2, 3, "sample-cad");
            var result = new ExternalMesherResultArtifact(
                1,
                true,
                nodes,
                [element],
                new Dictionary<string, string>
                {
                    ["adapter"] = "ImpactLab.MeshAdapter.Sample",
                    ["purpose"] = "wire-contract fixture",
                },
                null
            );
            ExternalMesherArtifactProtocol.WriteResult(args[1], result);
            Console.WriteLine("sample tetra mesh written");
            return 0;
        }
        catch (Exception ex)
        {
            ExternalMesherArtifactProtocol.WriteResult(
                args.Length > 1 ? args[1] : "mesher-result.json",
                new ExternalMesherResultArtifact(
                    1,
                    false,
                    [],
                    [],
                    new Dictionary<string, string>(),
                    ex.Message
                )
            );
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
