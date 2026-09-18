using ImpactLab.Core.Constraints;
using ImpactLab.Core.Contact;
using ImpactLab.Core.Fields;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Geometry.Imported;
using ImpactLab.Core.Loads;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.Parameters;
using ImpactLab.Core.PostProcessing;
using ImpactLab.Core.Rigid;
using ImpactLab.Core.Scene;
using ImpactLab.Core.Selection;

namespace ImpactLab.Core.Scenarios;

public static class ScenarioCompiler
{
    public static CompiledScenario Compile(ScenarioDefinition definition, MaterialCatalog materials)
    {
        definition.Validate();
        var scene = new SimulationScene();
        var importer = new MeshImportService();
        foreach (var part in definition.Parts)
        {
            GeometrySpec geometry;
            if (part.GeometrySource == ScenarioGeometrySourceKind.ImportedMesh)
            {
                if (string.IsNullOrWhiteSpace(part.ImportedMeshPath))
                    throw new InvalidOperationException(
                        $"Part {part.Id} has no imported mesh path."
                    );
                geometry = new AcceleratedTriangleMeshSpec(
                    part.Name,
                    importer.Import(part.ImportedMeshPath).Asset,
                    Vec3.Zero
                );
            }
            else
            {
                geometry = ParametricGeometryFactory.Create(
                    part.GeometryKind,
                    part.Name,
                    part.GeometryParameters.Clone()
                );
            }

            if (part.Transform != GeometryTransform3.Identity)
                geometry = new TransformedGeometrySpec(part.Name, geometry, part.Transform);
            scene.Add(
                new ScenePart(
                    part.Id,
                    part.Name,
                    geometry,
                    materials.Get(part.MaterialId),
                    part.Behavior
                )
            );
        }

        var meshSettings = definition.Meshing with
        {
            BaseCellSizeMeters =
                definition.Meshing.BaseCellSizeMeters <= 0
                    ? definition.CellSizeMeters
                    : definition.Meshing.BaseCellSizeMeters,
        };
        var compiled = SceneMeshCompiler.Compile(scene, meshSettings);
        var mesh = compiled.Mesh;
        var fixedConstraints = definition
            .Parts.Where(part => part.FixMinimumZPlane)
            .Select(part =>
                (IFixedConstraint)
                    new FixedPlaneConstraint(
                        ConstraintAxis.Z,
                        ConstraintSide.Minimum,
                        mesh.CellSize * 0.55,
                        part.Id
                    )
            )
            .ToList();
        var kinematic = definition
            .Boundaries.Where(boundary => boundary.Enabled)
            .Select(boundary => CompileBoundary(mesh, boundary))
            .ToArray();
        var loads = definition
            .Loads.Where(load => load.Enabled)
            .Select(load => CompileLoad(mesh, load))
            .ToArray();
        var probes = definition.Probes.Select(probe => CompileProbe(mesh, probe)).ToArray();
        var bodies = definition
            .RigidBodies.Select(body => new RigidBodyDefinition(
                body.Id,
                body.Name,
                ParametricGeometryFactory.Create(
                    body.GeometryKind,
                    body.Name,
                    body.GeometryParameters.Clone()
                ),
                materials.Get(body.MaterialId),
                body.InitialVelocity,
                initialAngularVelocityRadPerSec: body.InitialAngularVelocity
            ))
            .ToArray();
        var rigidContacts = new ContactInteractionTable();
        foreach (var contact in definition.Contacts)
        {
            rigidContacts.Set(
                new ContactPairRule(
                    contact.RigidBodyId,
                    contact.TargetPartId,
                    contact.Enabled,
                    contact.NormalStiffnessScale,
                    contact.NormalDampingScale,
                    contact.FrictionScale
                )
            );
        }

        return new CompiledScenario(
            mesh,
            fixedConstraints,
            kinematic,
            loads,
            probes,
            bodies,
            rigidContacts,
            new DeformableContactTable()
        );
    }

    private static NodeSelection ResolveSelection(
        ImpactLab.Core.Meshing.SimulationMesh mesh,
        string id,
        string? partId,
        bool surfaceOnly,
        Vec3 center,
        Vec3 size
    )
    {
        var half = size * 0.5;
        return NodeSelectionResolver.Resolve(
            mesh,
            id,
            new NodeSelectionQuery
            {
                PartId = partId,
                SurfaceOnly = surfaceOnly,
                Bounds = new BoundingBox3(center - half, center + half),
            }
        );
    }

    private static PiecewiseLinearCurve Curve(
        IReadOnlyList<LoadCurvePoint> points,
        double magnitude,
        double duration
    ) =>
        new(
            points.Count == 0
                ?
                [
                    new LoadCurvePoint(0, magnitude),
                    new LoadCurvePoint(Math.Max(duration, 1e-9), 0),
                ]
                : points
        );

    private static ILoadSource CompileLoad(
        ImpactLab.Core.Meshing.SimulationMesh mesh,
        ScenarioLoadDefinition definition
    )
    {
        var selection = ResolveSelection(
            mesh,
            $"load:{definition.Id}",
            definition.PartId,
            definition.SurfaceOnly,
            definition.SelectionCenter,
            definition.SelectionSize
        );
        var curve = Curve(definition.Curve, definition.Magnitude, definition.DurationSeconds);
        return definition.Kind switch
        {
            ScenarioLoadKind.PatchPressure => new PatchPressureLoad(
                selection,
                definition.Direction,
                curve
            ),
            ScenarioLoadKind.GaussianPressure => new FieldPressureLoad(
                selection,
                definition.Direction,
                curve,
                new GaussianScalarField(
                    definition.FieldCenter,
                    Math.Max(definition.FieldSigmaMeters, 1e-6)
                )
            ),
            ScenarioLoadKind.RadialPressure => new FieldPressureLoad(
                selection,
                definition.Direction,
                curve,
                new RadialFalloffField(
                    definition.FieldCenter,
                    Math.Max(definition.FieldRadiusMeters, 1e-6)
                )
            ),
            ScenarioLoadKind.BodyAcceleration => new BodyAccelerationLoad(
                selection,
                definition.Direction * definition.Magnitude
            ),
            ScenarioLoadKind.Impulse => new ImpulseLoad(
                selection,
                definition.Direction,
                definition.Magnitude,
                Math.Max(definition.DurationSeconds, 1e-9)
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(definition),
                definition.Kind,
                "Unsupported load kind."
            ),
        };
    }

    private static IKinematicConstraint CompileBoundary(
        ImpactLab.Core.Meshing.SimulationMesh mesh,
        ScenarioBoundaryDefinition definition
    )
    {
        var selection = ResolveSelection(
            mesh,
            $"boundary:{definition.Id}",
            definition.PartId,
            definition.SurfaceOnly,
            definition.SelectionCenter,
            definition.SelectionSize
        );
        var curve = Curve(definition.Curve, 1, 1);
        return definition.Kind switch
        {
            ScenarioBoundaryKind.AxisLock => new AxisLockConstraint(
                definition.Id,
                selection,
                definition.AxisMask
            ),
            ScenarioBoundaryKind.PrescribedVelocity => new PrescribedVelocityConstraint(
                definition.Id,
                selection,
                definition.Vector,
                curve
            ),
            ScenarioBoundaryKind.PrescribedDisplacement => new PrescribedDisplacementConstraint(
                definition.Id,
                selection,
                definition.Vector.Length <= 1e-12 ? Vec3.UnitZ : definition.Vector.Normalized(),
                curve
            ),
            ScenarioBoundaryKind.SymmetryPlane => new SymmetryPlaneConstraint(
                definition.Id,
                selection,
                definition.Vector.Length <= 1e-12 ? Vec3.UnitZ : definition.Vector
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(definition),
                definition.Kind,
                "Unsupported boundary kind."
            ),
        };
    }

    private static ProbeDefinition CompileProbe(
        ImpactLab.Core.Meshing.SimulationMesh mesh,
        ScenarioProbeDefinition definition
    ) =>
        new(
            definition.Id,
            definition.Name,
            definition.Quantity,
            ResolveSelection(
                mesh,
                $"probe:{definition.Id}",
                definition.PartId,
                definition.SurfaceOnly,
                definition.SelectionCenter,
                definition.SelectionSize
            )
        );
}
