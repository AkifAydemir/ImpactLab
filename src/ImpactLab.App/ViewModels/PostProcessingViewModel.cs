using System.ComponentModel;
using System.Windows.Media.Media3D;
using ImpactLab.App.Rendering;
using ImpactLab.App.Services;
using ImpactLab.Core.Mathematics;
using ImpactLab.Core.PostProcessing;

namespace ImpactLab.App.ViewModels;

public sealed class PostProcessingViewModel : INotifyPropertyChanged
{
    private SimulationRunBundle? _run;
    private int _frameIndex;
    private Model3DGroup _sectionScene = new();
    public IReadOnlyList<ProbeSeries> Probes => _run?.Probes ?? [];
    public EnergyBalanceSeries? EnergyBalance => _run?.EnergyBalance;
    public Model3DGroup SectionScene
    {
        get => _sectionScene;
        private set
        {
            _sectionScene = value;
            PropertyChanged?.Invoke(this, new(nameof(SectionScene)));
        }
    }
    public int FrameIndex
    {
        get => _frameIndex;
        set
        {
            _frameIndex = value;
            RebuildSection();
            PropertyChanged?.Invoke(this, new(nameof(FrameIndex)));
        }
    }
    public Vec3 PlaneNormal { get; set; } = Vec3.UnitZ;
    public double PlaneOffsetMeters { get; set; } = 0.0;
    public double PlaneHalfThicknessMeters { get; set; } = 0.003;

    public void Load(SimulationRunBundle run)
    {
        _run = run;
        _frameIndex = Math.Max(0, run.Result.Frames.Count - 1);
        RebuildSection();
        PropertyChanged?.Invoke(this, new(nameof(Probes)));
        PropertyChanged?.Invoke(this, new(nameof(EnergyBalance)));
    }

    public void RebuildSection()
    {
        if (_run is null || _run.Result.Frames.Count == 0)
            return;
        var f = _run.Result.Frames[Math.Clamp(_frameIndex, 0, _run.Result.Frames.Count - 1)];
        var section = SectionExtractor.Extract(
            _run.Compiled.Mesh,
            f,
            new CutPlaneDefinition("ui", PlaneNormal, PlaneOffsetMeters, PlaneHalfThicknessMeters)
        );
        SectionScene = CutPlaneSceneBuilder.Build(section);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
