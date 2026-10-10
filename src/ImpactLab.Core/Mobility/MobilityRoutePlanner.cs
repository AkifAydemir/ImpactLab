namespace ImpactLab.Core.Mobility;

public static class MobilityRoutePlanner
{
    private static readonly (int Row, int Column, double Distance)[] Neighbors =
    [
        (-1, 0, 1),
        (1, 0, 1),
        (0, -1, 1),
        (0, 1, 1),
        (-1, -1, Math.Sqrt(2)),
        (-1, 1, Math.Sqrt(2)),
        (1, -1, Math.Sqrt(2)),
        (1, 1, Math.Sqrt(2)),
    ];

    public static MobilityRoute Plan(
        TerrainGridMap map,
        VehicleMobilityProfile vehicle,
        GridCoordinate start,
        GridCoordinate goal
    )
    {
        Validate(map, start);
        Validate(map, goal);
        var startAssessment = TraversabilityAssessor.Assess(vehicle, map[start.Row, start.Column]);
        var goalAssessment = TraversabilityAssessor.Assess(vehicle, map[goal.Row, goal.Column]);
        if (
            startAssessment.Status == TraversabilityStatus.Blocked
            || goalAssessment.Status == TraversabilityStatus.Blocked
        )
            return new(false, [], double.PositiveInfinity, "Start or goal cell is blocked.");

        var queue = new PriorityQueue<GridCoordinate, double>();
        var cost = new Dictionary<GridCoordinate, double> { [start] = 0 };
        var previous = new Dictionary<GridCoordinate, GridCoordinate>();
        queue.Enqueue(start, Heuristic(start, goal));
        while (queue.TryDequeue(out var current, out _))
        {
            if (current == goal)
                return Build(previous, cost[goal], start, goal);
            foreach (var offset in Neighbors)
            {
                var next = new GridCoordinate(
                    current.Row + offset.Row,
                    current.Column + offset.Column
                );
                if (
                    next.Row < 0
                    || next.Row >= map.Rows
                    || next.Column < 0
                    || next.Column >= map.Columns
                )
                    continue;
                var assessment = TraversabilityAssessor.Assess(vehicle, map[next.Row, next.Column]);
                if (assessment.Status == TraversabilityStatus.Blocked)
                    continue;
                var nextCost = cost[current] + offset.Distance * assessment.CostMultiplier;
                if (cost.TryGetValue(next, out var known) && nextCost >= known)
                    continue;
                cost[next] = nextCost;
                previous[next] = current;
                queue.Enqueue(next, nextCost + Heuristic(next, goal));
            }
        }
        return new(false, [], double.PositiveInfinity, "No passable route was found.");
    }

    private static MobilityRoute Build(
        IReadOnlyDictionary<GridCoordinate, GridCoordinate> previous,
        double cost,
        GridCoordinate start,
        GridCoordinate goal
    )
    {
        var cells = new List<GridCoordinate> { goal };
        var current = goal;
        while (current != start)
        {
            current = previous[current];
            cells.Add(current);
        }
        cells.Reverse();
        return new(true, cells, cost, $"Route contains {cells.Count} cells.");
    }

    private static double Heuristic(GridCoordinate a, GridCoordinate b)
    {
        var row = a.Row - b.Row;
        var column = a.Column - b.Column;
        return Math.Sqrt(row * row + column * column);
    }

    private static void Validate(TerrainGridMap map, GridCoordinate coordinate)
    {
        if (
            coordinate.Row < 0
            || coordinate.Row >= map.Rows
            || coordinate.Column < 0
            || coordinate.Column >= map.Columns
        )
            throw new ArgumentOutOfRangeException(nameof(coordinate));
    }
}
