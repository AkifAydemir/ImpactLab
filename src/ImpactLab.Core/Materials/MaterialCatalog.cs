namespace ImpactLab.Core.Materials;

public sealed class MaterialCatalog
{
    private readonly Dictionary<string, MaterialCatalogEntry> _entries = new(
        StringComparer.OrdinalIgnoreCase
    );

    public MaterialCatalog(bool includeBuiltIns = true)
    {
        if (!includeBuiltIns)
            return;
        foreach (var material in MaterialLibrary.All)
            _entries[material.Id] = new MaterialCatalogEntry(
                material,
                CategoryFor(material),
                "Built-in reference material.",
                true
            );
    }

    public IReadOnlyList<MaterialCatalogEntry> Entries =>
        _entries.Values.OrderBy(x => x.Definition.DisplayName).ToArray();

    public MaterialDefinition Get(string id) =>
        _entries.TryGetValue(id, out var value)
            ? value.Definition
            : throw new KeyNotFoundException($"Material not found: {id}");

    public MaterialCatalogEntry AddUser(
        MaterialDefinition material,
        string category = "User",
        string notes = ""
    )
    {
        material.Validate();
        if (_entries.ContainsKey(material.Id))
            throw new InvalidOperationException($"Material id already exists: {material.Id}");
        var entry = new MaterialCatalogEntry(material, category, notes, false);
        _entries.Add(material.Id, entry);
        return entry;
    }

    public MaterialCatalogEntry CloneAsUser(string sourceId, string newId, string newName)
    {
        var source = Get(sourceId);
        return AddUser(
            source with
            {
                Id = newId,
                DisplayName = newName,
            },
            "User",
            $"Cloned from {sourceId}."
        );
    }

    public void ReplaceUser(MaterialDefinition material, string category, string notes)
    {
        material.Validate();
        if (_entries.TryGetValue(material.Id, out var current) && current.IsBuiltIn)
            throw new InvalidOperationException("Built-in materials cannot be replaced.");
        _entries[material.Id] = new MaterialCatalogEntry(material, category, notes, false);
    }

    public bool RemoveUser(string id) =>
        _entries.TryGetValue(id, out var entry) && !entry.IsBuiltIn && _entries.Remove(id);

    private static string CategoryFor(MaterialDefinition material) =>
        material.ResponseKind switch
        {
            MaterialResponseKind.Brittle => "Brittle",
            MaterialResponseKind.Ductile => "Metal / ductile",
            MaterialResponseKind.Polymer => "Polymer",
            _ => "Generic",
        };
}
