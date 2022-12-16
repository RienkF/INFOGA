Random rnd = new();

// See https://aka.ms/new-console-template for more information
Console.WriteLine("INFOGA | Convex Hull | Rienk Fidder - Merijn Schepers");

IEnumerable<Point> points = GetPoints();
Algorithm algorithm = ChooseAlgorithm();

IEnumerable<Point> convexHull = algorithm switch
{
    Algorithm.GrahamScan => GrahamScan(points),
    _ => throw new NotImplementedException()
};



IEnumerable<Point> GetPoints()
{
    Console.WriteLine("How many points?");

    string? input = Console.ReadLine();

    int n;
    while (!int.TryParse(input, out n))
        input = Console.ReadLine();


    return new List<Point>(n).Select(_ => new Point(rnd.Next(-100, 100), rnd.Next(-100, 100)));
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

    IEnumerable<Point> lUpper = new List<Point>();

    for (int i = 0; i < sortedPoints.Count(); i++)
    {
        lUpper = lUpper.Append(sortedPoints.ElementAt(i));
        if (i < 2)
            continue;

        bool done = false;

        while (!done)
        {
            IEnumerable<Point> last3Points = lUpper.TakeLast(3);

            Point a = last3Points.First();
            Point b = last3Points.ElementAt(1);
            Point c = last3Points.Last();

            // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise 
            // Second answer, check if counterclockwise
            if (b.X * a.Y + c.X * b.Y + a.X * c.Y < a.X * b.Y + b.X * c.Y + c.X * a.Y)
                lUpper = lUpper.Except(new List<Point> { b });
            else done = true;
        }
    }

    // TODO: lower half

    return lUpper;
}

public record Point(int X, int Y);

public enum Algorithm
{
    GrahamScan
}