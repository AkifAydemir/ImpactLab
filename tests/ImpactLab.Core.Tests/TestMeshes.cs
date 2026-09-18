using ImpactLab.Core.Continuum;
using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Tests;

public static class TestMeshes
{
    public static TetrahedralMesh SingleTetra()
    {
        var m = TestMaterials.Steel;
        return new(
            [
                new(0, new(0, 0, 0), "p", m),
                new(1, new(1, 0, 0), "p", m),
                new(2, new(0, 1, 0), "p", m),
                new(3, new(0, 0, 1), "p", m),
            ],
            [new(0, 0, 1, 2, 3, "p", m)]
        );
    }
}
