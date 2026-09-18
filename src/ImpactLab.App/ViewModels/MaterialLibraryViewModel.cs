using System.ComponentModel;
using ImpactLab.Core.Materials;

namespace ImpactLab.App.ViewModels;

public sealed class MaterialLibraryViewModel : INotifyPropertyChanged
{
    private MaterialCatalogEntry? _selected;

    public MaterialLibraryViewModel(MaterialCatalog catalog)
    {
        Catalog = catalog;
        Selected = Catalog.Entries.FirstOrDefault();
    }

    public MaterialCatalog Catalog { get; }
    public IReadOnlyList<MaterialCatalogEntry> Entries => Catalog.Entries;
    public MaterialCatalogEntry? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            PropertyChanged?.Invoke(this, new(nameof(Selected)));
        }
    }

    public void Refresh() => PropertyChanged?.Invoke(this, new(nameof(Entries)));

    public event PropertyChangedEventHandler? PropertyChanged;
}
