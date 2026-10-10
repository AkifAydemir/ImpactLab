using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using ImpactLab.App.Infrastructure;
using ImpactLab.Core.Materials;

namespace ImpactLab.App.ViewModels;

public sealed class MaterialLibraryViewModel : INotifyPropertyChanged
{
    private MaterialCatalogEntry? _selected;
    private string _status = "Select a material to inspect it or save an editable project copy.";
    private readonly MaterialProfileCatalog _profiles;

    public MaterialLibraryViewModel(MaterialCatalog catalog, MaterialProfileCatalog profiles)
    {
        Catalog = catalog;
        _profiles = profiles;
        AddMaterialCommand = new RelayCommand(AddMaterial);
        Selected = Catalog.Entries.FirstOrDefault();
    }

    public MaterialCatalog Catalog { get; }
    public IReadOnlyList<MaterialCatalogEntry> Entries => Catalog.Entries;
    public IReadOnlyList<MaterialResponseKind> ResponseKinds { get; } =
        Enum.GetValues<MaterialResponseKind>();
    public ICommand AddMaterialCommand { get; }
    public string NewId { get; set; } = "custom-material";
    public string NewName { get; set; } = "Custom Material";
    public MaterialResponseKind ResponseKind { get; set; } = MaterialResponseKind.Ductile;
    public double DensityKgPerM3 { get; set; } = 7_850;
    public double YoungModulusGPa { get; set; } = 200;
    public double PoissonRatio { get; set; } = 0.30;
    public double YieldStrengthMPa { get; set; } = 250;
    public double UltimateStrengthMPa { get; set; } = 400;
    public double FailureStrain { get; set; } = 0.15;
    public double DampingRatio { get; set; } = 0.02;
    public double TangentModulusGPa { get; set; } = 2;
    public double CompressiveStrengthScale { get; set; } = 1.2;
    public double RateSensitivity { get; set; } = 0.01;
    public double ThermalConductivity { get; set; }
    public double SpecificHeat { get; set; }
    public string ColorHex { get; set; } = "#7D8794";
    public string Standard { get; set; } = "User supplied / verify source";
    public string SourceName { get; set; } = "User supplied";
    public string SourceUri { get; set; } = "";
    public string Specification { get; set; } = "";
    public string BatchOrLot { get; set; } = "";
    public string Notes { get; set; } = "";
    public string TensileCurveText { get; set; } = "0,0\n0.00125,250";
    public string CompressiveCurveText { get; set; } = "";
    public double YieldMinimumMPa { get; set; } = 225;
    public double YieldMaximumMPa { get; set; } = 275;
    public double YoungMinimumGPa { get; set; } = 190;
    public double YoungMaximumGPa { get; set; } = 210;
    public string CalibrationModel { get; set; } = "";
    public double CalibrationRmseMPa { get; set; }
    public double CalibrationMaeMPa { get; set; }
    public double CalibrationRSquared { get; set; }
    public int CalibrationSampleCount { get; set; }

    public string SelectedEvidenceSummary
    {
        get
        {
            if (Selected is null)
                return "No material selected.";
            var profile = _profiles.Get(Selected.Definition.Id);
            var curve = profile.TensileCurve is null
                ? "no test curve"
                : $"{profile.TensileCurve.Points.Count} tensile points";
            var trace = profile.Provenance.IsTraceable
                ? "traceable source declared"
                : "untraceable";
            var calibration = profile.Calibration is null ? "not calibrated" : "calibrated";
            return $"{curve} · {trace} · {calibration}. Built-in values remain nominal until replaced with specimen evidence.";
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            Changed(nameof(Status));
        }
    }

    public MaterialCatalogEntry? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            Changed(nameof(Selected));
            LoadSelectedIntoDraft();
        }
    }

    public void Refresh() => Changed(nameof(Entries));

    private void LoadSelectedIntoDraft()
    {
        if (_selected is null)
            return;
        var d = _selected.Definition;
        var p = _profiles.Get(d.Id);
        NewId = $"{d.Id}-custom";
        NewName = $"{d.DisplayName} — Project Copy";
        ResponseKind = d.ResponseKind;
        DensityKgPerM3 = d.DensityKgPerM3;
        YoungModulusGPa = d.YoungModulusPa / 1e9;
        PoissonRatio = d.PoissonRatio;
        YieldStrengthMPa = d.EffectiveYieldStrengthPa / 1e6;
        UltimateStrengthMPa = d.UltimateTensileStrengthPa / 1e6;
        FailureStrain = d.FailureStrain;
        DampingRatio = d.DampingRatio;
        TangentModulusGPa = d.TangentModulusPa / 1e9;
        CompressiveStrengthScale = d.CompressiveStrengthScale;
        RateSensitivity = d.RateSensitivity;
        ThermalConductivity = d.ThermalConductivityWPerMK;
        SpecificHeat = d.SpecificHeatJPerKgK;
        Standard = d.Standard;
        ColorHex = d.ColorHex;
        SourceName = p.Provenance.SourceName;
        SourceUri = p.Provenance.SourceUri ?? "";
        Specification = p.Provenance.Specification ?? d.Standard;
        BatchOrLot = p.Provenance.BatchOrLot ?? "";
        Notes = p.Notes;
        TensileCurveText = FormatCurve(p.TensileCurve);
        CompressiveCurveText = FormatCurve(p.CompressiveCurve);
        CalibrationModel = p.Calibration?.ModelName ?? "";
        CalibrationRmseMPa = (p.Calibration?.RmsePa ?? 0) / 1e6;
        CalibrationMaeMPa = (p.Calibration?.MaePa ?? 0) / 1e6;
        CalibrationRSquared = p.Calibration?.RSquared ?? 0;
        CalibrationSampleCount = p.Calibration?.SampleCount ?? 0;
        YieldMinimumMPa = YieldStrengthMPa * 0.9;
        YieldMaximumMPa = YieldStrengthMPa * 1.1;
        YoungMinimumGPa = YoungModulusGPa * 0.95;
        YoungMaximumGPa = YoungModulusGPa * 1.05;
        ChangedAllDraftProperties();
    }

    private void AddMaterial()
    {
        try
        {
            var modulus = YoungModulusGPa * 1e9;
            var yield = YieldStrengthMPa * 1e6;
            var definition = new MaterialDefinition(
                NewId.Trim(),
                NewName.Trim(),
                DensityKgPerM3,
                modulus,
                PoissonRatio,
                yield / Math.Max(modulus, 1e-12),
                FailureStrain,
                DampingRatio,
                ResponseKind,
                TangentModulusGPa * 1e9,
                CompressiveStrengthScale,
                RateSensitivity,
                yield,
                UltimateStrengthMPa * 1e6,
                ThermalConductivity,
                SpecificHeat,
                Standard,
                ColorHex
            );
            var uncertainty = new MaterialUncertainty();
            uncertainty.Set(
                "yield-strength",
                new(yield, YieldMinimumMPa * 1e6, YieldMaximumMPa * 1e6, "Pa")
            );
            uncertainty.Set(
                "young-modulus",
                new(modulus, YoungMinimumGPa * 1e9, YoungMaximumGPa * 1e9, "Pa")
            );
            var profile = new MaterialProfile(definition)
            {
                TensileCurve = ParseCurve(TensileCurveText),
                CompressiveCurve = ParseCurve(CompressiveCurveText),
                Provenance = new(
                    SourceName.Trim(),
                    EmptyToNull(SourceUri),
                    EmptyToNull(Specification),
                    EmptyToNull(BatchOrLot),
                    Notes: Notes
                ),
                Calibration = string.IsNullOrWhiteSpace(CalibrationModel)
                    ? null
                    : new(
                        CalibrationRmseMPa * 1e6,
                        CalibrationMaeMPa * 1e6,
                        CalibrationRSquared,
                        CalibrationSampleCount,
                        CalibrationModel.Trim()
                    ),
                Uncertainty = uncertainty,
                Notes = Notes,
            };
            profile.Validate();
            var entry = Catalog.AddUser(definition, "Project material", Notes);
            _profiles.Upsert(profile);
            Selected = entry;
            Refresh();
            MaterialAdded?.Invoke(this, EventArgs.Empty);
            Status =
                $"Saved {definition.DisplayName} with source, curves, uncertainty and calibration metadata.";
        }
        catch (Exception ex)
        {
            Status = $"Material was not saved: {ex.Message}";
        }
    }

    private static ConstitutiveCurve? ParseCurve(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var points = text.Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split([',', '\t'], StringSplitOptions.TrimEntries))
            .Select(parts =>
            {
                if (parts.Length != 2)
                    throw new FormatException("Curve rows must be: strain, stress_MPa");
                return new ConstitutiveCurvePoint(
                    double.Parse(parts[0], CultureInfo.InvariantCulture),
                    double.Parse(parts[1], CultureInfo.InvariantCulture) * 1e6
                );
            });
        return new ConstitutiveCurve(points);
    }

    private static string FormatCurve(ConstitutiveCurve? curve) =>
        curve is null
            ? ""
            : string.Join(
                Environment.NewLine,
                curve.Points.Select(x => $"{x.Strain:G8},{x.StressPa / 1e6:G8}")
            );

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void ChangedAllDraftProperties()
    {
        foreach (
            var property in GetType().GetProperties().Where(x => x.CanRead && x.Name != "Selected")
        )
            Changed(property.Name);
        Changed(nameof(SelectedEvidenceSummary));
    }

    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? MaterialAdded;
}
