namespace ImpactLab.Core.Sparse.Parallel;

public static class AssemblyPartitioner
{
    public static IReadOnlyList<AssemblyPartition> Create(int elements, int workers)
    {
        workers = Math.Clamp(workers, 1, Math.Max(1, elements));
        var list = new List<AssemblyPartition>();
        for (var w = 0; w < workers; w++)
        {
            var a = elements * w / workers;
            var b = elements * (w + 1) / workers;
            list.Add(new(w, a, b));
        }
        return list;
    }
}
