using ImpactLab.Core.Mathematics;

namespace ImpactLab.Core.Continuum;

public readonly record struct SurfaceTriangle(int A, int B, int C, string PartId)
{
    public int Id =>
        unchecked(
            (Math.Min(A, Math.Min(B, C)) * 397) ^ ((A + B + C) * 31) ^ Math.Max(A, Math.Max(B, C))
        );

    public int[] Nodes() => [A, B, C];

    public Vec3 Normal(TetrahedralMesh mesh) =>
        Vec3.Cross(
                mesh.Nodes[B].Position - mesh.Nodes[A].Position,
                mesh.Nodes[C].Position - mesh.Nodes[A].Position
            )
            .Normalized();

    public double Area(TetrahedralMesh mesh) =>
        0.5
        * Vec3.Cross(
            mesh.Nodes[B].Position - mesh.Nodes[A].Position,
            mesh.Nodes[C].Position - mesh.Nodes[A].Position
        ).Length;
}
