Random rnd = new();

const int leftBound = -30;
const int rightBound = 30;
const int topBound = 15;
const int bottomBound = -15;

Console.WriteLine("INFOGA | Convex Hull | Rienk Fidder - Merijn Schepers");

IEnumerable<Point> points = GetPoints();
Algorithm algorithm = ChooseAlgorithm();

Console.WriteLine("Points:");

foreach (Point point in points)
    Console.WriteLine($"{point.X}, {point.Y}");

IEnumerable<Point> convexHull = algorithm switch
{
    Algorithm.GrahamScan => GrahamScan(points),
    _ => throw new NotImplementedException()
};

Console.WriteLine("Convex Hull:");

foreach (Point point in convexHull) 
    Console.WriteLine($"{point.X}, {point.Y}");

Console.WriteLine("Image:");

DrawHull(points, convexHull);

IEnumerable<Point> GetPoints()
{
    Console.WriteLine("How many points?");

    string? input = Console.ReadLine();

    int n;
    while (!int.TryParse(input, out n))
        input = Console.ReadLine();


    return new int[n].Select(_ => new Point(rnd.Next(leftBound, rightBound), rnd.Next(bottomBound, topBound))).Distinct().ToList();
}

Algorithm ChooseAlgorithm()
{
    Console.WriteLine("What algorithm?");
    foreach (int i in Enum.GetValues(typeof(Algorithm)))
    {
        Console.WriteLine($"({i}) | {Enum.GetName(typeof(Algorithm), i)}");
    }

    string? input = Console.ReadLine();

    Algorithm algorithm;
    while (!Enum.TryParse(input, out algorithm) || !Enum.IsDefined(algorithm))
        input = Console.ReadLine();

    return algorithm;
}

IEnumerable<Point> GrahamScan(IEnumerable<Point> points)
{
    IEnumerable<Point> sortedPoints = points.OrderBy(p => p.X).ToList();
    IEnumerable<Point> reversedPoints = sortedPoints.Reverse().ToList();


    IEnumerable<Point> upper = new List<Point>();
    IEnumerable<Point> lower = new List<Point>();

    for (int i = 0; i < sortedPoints.Count(); i++)
    {
        lower = lower.Append(sortedPoints.ElementAt(i)).ToList();
        if (i < 2)
            continue;

        bool done = false;

        while (!done || lower.Count() < 3)
        {
            IEnumerable<Point> last3Points = lower.TakeLast(3).ToList();

            Point a = last3Points.First();
            Point b = last3Points.ElementAt(1);
            Point c = last3Points.Last();

            // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise 
            // Second answer, check if counterclockwise, but strictly
            if (b.X * a.Y + c.X * b.Y + a.X * c.Y <= a.X * b.Y + b.X * c.Y + c.X * a.Y)
                lower = lower.Except(new List<Point> { b }).ToList();
            else done = true;
        }
    }

    for (int i = 0; i < reversedPoints.Count(); i++)
    {
        upper = upper.Append(reversedPoints.ElementAt(i)).ToList();
        if (i < 2)
            continue;

        bool done = false;

        while (!done || upper.Count() < 3)
        {
            IEnumerable<Point> last3Points = upper.TakeLast(3).ToList();

            Point a = last3Points.First();
            Point b = last3Points.ElementAt(1);
            Point c = last3Points.Last();

            // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise 
            // Second answer, check if counterclockwise
            if (b.X * a.Y + c.X * b.Y + a.X * c.Y <= a.X * b.Y + b.X * c.Y + c.X * a.Y)
                upper = upper.Except(new List<Point> { b }).ToList();
            else done = true;
        }
    }
    
    return lower.Skip(1).Concat(upper.Skip(1));
}

void DrawHull(IEnumerable<Point> points, IEnumerable<Point> convexHullPoints)
{
    convexHullPoints = convexHullPoints.ToList();

    List<IGrouping<int, Point>> sortedPoints = points.OrderBy(x => x.Y).Reverse().GroupBy(x => x.Y).ToList();
    int lastLine = topBound;

    foreach (IGrouping<int, Point> grouping in sortedPoints)
    {
        while(grouping.Key < lastLine)
        {
            Console.WriteLine(" ");
            lastLine--;
        }

        IEnumerable<Point> sortedPointsOnLine = grouping.OrderBy(x => x.X).ToList();

        int lastChar = leftBound - 1;

        foreach (Point point in sortedPointsOnLine)
        {
            while (lastChar < point.X)
            {
                Console.Write(' ');
                lastChar++;
            }
            Console.Write(convexHullPoints.Any(x => x.X == point.X && x.Y == point.Y) ? 'X' : 'O');
            lastChar++;
        }

        Console.WriteLine();
        lastLine--;
    }


}

public record Point(int X, int Y);

public enum Algorithm
{
    GrahamScan
}