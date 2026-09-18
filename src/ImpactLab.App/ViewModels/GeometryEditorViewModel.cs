using System.ComponentModel;
using ImpactLab.Core.Geometry;
using ImpactLab.Core.Parameters;

namespace ImpactLab.App.ViewModels;

public sealed class GeometryEditorViewModel : INotifyPropertyChanged
{
    private GeometryKind _kind = GeometryKind.Plate;
    private ParameterSet _parameters = GeometryParameterSchemas
        .For(GeometryKind.Plate)
        .CreateDefaults();
    public GeometryKind Kind
    {
        get => _kind;
        set
        {
            if (value == GeometryKind.ImportedMesh)
                throw new InvalidOperationException(
                    "Use the Import workspace for imported mesh geometry."
                );
            if (_kind == value)
                return;
            _kind = value;
            _parameters = GeometryParameterSchemas.For(value).CreateDefaults();
            PropertyChanged?.Invoke(this, new(nameof(Kind)));
            PropertyChanged?.Invoke(this, new(nameof(Parameters)));
        }
    }
    public ParameterSet Parameters => _parameters;
    public IReadOnlyList<GeometryKind> Kinds { get; } =
        Enum.GetValues<GeometryKind>().Where(x => x != GeometryKind.ImportedMesh).ToArray();
    public ParameterSchema Schema => GeometryParameterSchemas.For(Kind);

    public GeometrySpec Build(string name = "Preview") =>
        ParametricGeometryFactory.Create(Kind, name, Parameters.Clone());

    public event PropertyChangedEventHandler? PropertyChanged;
}
