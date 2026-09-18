using System.IO.MemoryMappedFiles;

namespace ImpactLab.Core.Results.Storage;

public sealed class MemoryMappedFieldReader
{
    public double[] Read(string path, long offsetBytes, int count)
    {
        using var mm = MemoryMappedFile.CreateFromFile(
            path,
            FileMode.Open,
            null,
            0,
            MemoryMappedFileAccess.Read
        );
        using var v = mm.CreateViewAccessor(
            offsetBytes,
            count * sizeof(double),
            MemoryMappedFileAccess.Read
        );
        var a = new double[count];
        v.ReadArray(0, a, 0, count);
        return a;
    }
}
