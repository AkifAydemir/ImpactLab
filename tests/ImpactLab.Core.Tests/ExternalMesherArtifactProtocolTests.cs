using ImpactLab.Core.Continuum.Meshing;
using ImpactLab.Core.Continuum.Meshing.External;
using ImpactLab.Core.Geometry.Cad;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class ExternalMesherArtifactProtocolTests
{
    [Fact]
    public void RoundTripsNeutralRequestAndBuildsMesh()
    {
        var ids = Enumerable
            .Range(0, 4)
            .Select(i => new CadEntityId(Guid.Parse($"00000000-0000-0000-0000-00000000000{i + 1}")))
            .ToArray();
        var model = new CadModel(
            [
                new(ids[0], new Vec3(0, 0, 0), 1e-9),
                new(ids[1], new Vec3(1, 0, 0), 1e-9),
                new(ids[2], new Vec3(0, 1, 0), 1e-9),
                new(ids[3], new Vec3(0, 0, 1), 1e-9),
            ],
            [],
            [],
            [],
            "neutral-fixture",
            null
        );
        var request = new ExternalTetraMesherRequest(
            model,
            MaterialLibrary.Get("generic-steel"),
            new VolumeMeshingSettings(),
            Path.GetTempPath(),
            new Dictionary<string, string>()
        );
        var artifact = ExternalMesherArtifactProtocol.ToArtifact(request);
        Assert.Equal(4, artifact.Vertices.Count);
        var result = new ExternalMesherResultArtifact(
            1,
            true,
            [
                new(0, 0, 0, 0, "p"),
                new(1, 1, 0, 0, "p"),
                new(2, 0, 1, 0, "p"),
                new(3, 0, 0, 1, "p"),
            ],
            [new(0, 0, 1, 2, 3, "p")],
            new Dictionary<string, string>(),
            null
        );
        var mesh = ExternalMesherArtifactProtocol.ToMesh(
            result,
            MaterialLibrary.Get("generic-steel")
        );
        Assert.Single(mesh.Elements);
    }
}
