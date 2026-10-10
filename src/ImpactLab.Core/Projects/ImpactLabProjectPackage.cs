using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ImpactLab.Core.IO;
using ImpactLab.Core.Materials;
using ImpactLab.Core.Scenarios;

namespace ImpactLab.Core.Projects;

public static class ImpactLabProjectPackage
{
    public const string FileExtension = ".impactlab";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static void Save(
        string path,
        ScenarioDefinition scenario,
        IEnumerable<MaterialProfile>? customMaterials = null
    )
    {
        ArgumentNullException.ThrowIfNull(scenario);
        scenario.Validate();
        if (
            !string.Equals(
                Path.GetExtension(path),
                FileExtension,
                StringComparison.OrdinalIgnoreCase
            )
        )
            path += FileExtension;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        var portable = ScenarioEnvelopeSerializer.Deserialize(
            ScenarioEnvelopeSerializer.Serialize(scenario)
        );
        var geometry = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var geometrySources = new List<(string Source, string Entry)>();
        foreach (
            var part in portable.Parts.Where(x => !string.IsNullOrWhiteSpace(x.ImportedMeshPath))
        )
        {
            var source = Path.GetFullPath(part.ImportedMeshPath!);
            if (!File.Exists(source))
                throw new FileNotFoundException(
                    $"Imported geometry was not found for part {part.Id}.",
                    source
                );
            var entry = $"geometry/{Safe(part.Id)}{Path.GetExtension(source).ToLowerInvariant()}";
            var geometryKey = $"part:{part.Id}";
            if (geometry.ContainsKey(geometryKey))
                throw new InvalidDataException($"Duplicate part id in project: {part.Id}");
            geometry.Add(geometryKey, entry);
            geometrySources.Add((source, entry));
            part.ImportedMeshPath = entry;
        }
        foreach (
            var body in portable.RigidBodies.Where(x =>
                !string.IsNullOrWhiteSpace(x.ImportedMeshPath)
            )
        )
        {
            var source = Path.GetFullPath(body.ImportedMeshPath!);
            if (!File.Exists(source))
                throw new FileNotFoundException(
                    $"Imported geometry was not found for rigid body {body.Id}.",
                    source
                );
            var entry =
                $"geometry/rigid-{Safe(body.Id)}{Path.GetExtension(source).ToLowerInvariant()}";
            var geometryKey = $"rigid:{body.Id}";
            if (geometry.ContainsKey(geometryKey))
                throw new InvalidDataException($"Duplicate rigid body id in project: {body.Id}");
            geometry.Add(geometryKey, entry);
            geometrySources.Add((source, entry));
            body.ImportedMeshPath = entry;
        }

        var profiles = (customMaterials ?? []).OrderBy(x => x.Definition.Id).ToArray();
        foreach (var profile in profiles)
            profile.Validate();
        var materialEntries = profiles
            .Select(x => $"materials/{Safe(x.Definition.Id)}.json")
            .ToArray();
        var manifest = new ImpactLabProjectManifest(
            ImpactLabProjectManifest.CurrentSchemaVersion,
            ImpactLabProjectManifest.FormatId,
            scenario.Id,
            scenario.Name,
            DateTimeOffset.UtcNow,
            "scenario.json",
            materialEntries,
            geometry
        );

        var temporary = path + ".tmp";
        if (File.Exists(temporary))
            File.Delete(temporary);
        try
        {
            using (var archive = ZipFile.Open(temporary, ZipArchiveMode.Create))
            {
                WriteText(archive, "manifest.json", JsonSerializer.Serialize(manifest, Json));
                WriteText(
                    archive,
                    manifest.ScenarioEntry,
                    ScenarioEnvelopeSerializer.Serialize(portable)
                );
                for (var i = 0; i < profiles.Length; i++)
                    WriteText(
                        archive,
                        materialEntries[i],
                        MaterialProfileJsonSerializer.Serialize(profiles[i])
                    );
                foreach (var asset in geometrySources)
                    archive.CreateEntryFromFile(
                        asset.Source,
                        asset.Entry,
                        CompressionLevel.Optimal
                    );
            }
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    public static ImpactLabProject Load(string path, string extractionRoot)
    {
        if (
            !string.Equals(
                Path.GetExtension(path),
                FileExtension,
                StringComparison.OrdinalIgnoreCase
            )
        )
            throw new InvalidDataException($"ImpactLab projects must use {FileExtension}.");
        Directory.CreateDirectory(extractionRoot);
        using var archive = ZipFile.OpenRead(path);
        var manifest =
            JsonSerializer.Deserialize<ImpactLabProjectManifest>(
                ReadText(archive, "manifest.json"),
                Json
            ) ?? throw new InvalidDataException("Project manifest is empty.");
        manifest.Validate();

        var packageHash = Convert
            .ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
            .ToLowerInvariant()[..16];
        var assetDirectory = Path.GetFullPath(Path.Combine(extractionRoot, packageHash));
        Directory.CreateDirectory(assetDirectory);
        var scenario = ScenarioEnvelopeSerializer.Deserialize(
            ReadText(archive, manifest.ScenarioEntry)
        );
        foreach (
            var part in scenario.Parts.Where(x => !string.IsNullOrWhiteSpace(x.ImportedMeshPath))
        )
        {
            if (
                !manifest.GeometryEntries.TryGetValue($"part:{part.Id}", out var entryName)
                && !manifest.GeometryEntries.TryGetValue(part.Id, out entryName)
            )
                throw new InvalidDataException($"Geometry entry is missing for part {part.Id}.");
            var entry = RequireSafeEntry(archive, entryName);
            var destination = SafeDestination(assetDirectory, entryName);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, true);
            part.ImportedMeshPath = destination;
        }
        foreach (
            var body in scenario.RigidBodies.Where(x =>
                !string.IsNullOrWhiteSpace(x.ImportedMeshPath)
            )
        )
        {
            if (!manifest.GeometryEntries.TryGetValue($"rigid:{body.Id}", out var entryName))
                throw new InvalidDataException(
                    $"Geometry entry is missing for rigid body {body.Id}."
                );
            var entry = RequireSafeEntry(archive, entryName);
            var destination = SafeDestination(assetDirectory, entryName);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, true);
            body.ImportedMeshPath = destination;
        }
        var materials = manifest
            .MaterialEntries.Select(x =>
                MaterialProfileJsonSerializer.Deserialize(ReadText(archive, x))
            )
            .ToArray();
        scenario.Validate();
        return new(manifest, scenario, materials, assetDirectory);
    }

    private static void WriteText(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string ReadText(ZipArchive archive, string name)
    {
        var entry = RequireSafeEntry(archive, name);
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8, true);
        return reader.ReadToEnd();
    }

    private static ZipArchiveEntry RequireSafeEntry(ZipArchive archive, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || Path.IsPathRooted(name) || name.Contains(".."))
            throw new InvalidDataException($"Unsafe project entry path: {name}");
        return archive.GetEntry(name)
            ?? throw new InvalidDataException($"Project entry is missing: {name}");
    }

    private static string SafeDestination(string root, string relative)
    {
        var destination = Path.GetFullPath(
            Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar))
        );
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!destination.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(
                $"Project entry escapes the asset directory: {relative}"
            );
        return destination;
    }

    private static string Safe(string value) =>
        string.Concat(value.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
}
