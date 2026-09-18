using System.Text.Json;

namespace ImpactLab.Core.Projects;

public static class ProjectJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static void Save(string path, SimulationProject project)
    {
        var json = JsonSerializer.Serialize(ProjectMapper.ToDto(project), Options);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, json);
    }

    public static SimulationProject Load(string path)
    {
        var json = File.ReadAllText(path);
        var dto =
            JsonSerializer.Deserialize<ProjectFileDto>(json, Options)
            ?? throw new InvalidDataException("Project file is empty or invalid.");
        return ProjectMapper.FromDto(dto);
    }
}
