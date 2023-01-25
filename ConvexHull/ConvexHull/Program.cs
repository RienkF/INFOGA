using System.Diagnostics;

Random rnd = new ();

// Hardcoded bounds for the points, based on console width and height
const int leftBound = -30;
const int rightBound = 30;
const int topBound = 15;
const int bottomBound = -15;

Console.WriteLine("INFOGA | Convex Hull | Rienk Fidder");

// Uncomment either line to run an experiment, or an interactive version
//RunExperiment();
//RunInteractive();

// Run an experiment testing the performance of both algorithms
void RunExperiment()
{
    int[] experimentSizes = { 100, 1000, 10000, 100000, 1000000, 10000000 };

    IEnumerable<IEnumerable<Point>> randomTestSets = experimentSizes.Select(GetRandomPoints).ToList();
    IEnumerable<IEnumerable<Point>> circleTestSets = experimentSizes.Select(GetPointsOnCircle).ToList();

    // Graham scan
    Console.WriteLine("Graham scan - Random");

    foreach (IEnumerable<Point> testPoints in randomTestSets)
        Console.WriteLine(TimeAlgorithmWithPoints(testPoints, GrahamScan));

    Console.WriteLine("Graham scan - Circle");

    foreach (IEnumerable<Point> testPoints in circleTestSets)
        Console.WriteLine(TimeAlgorithmWithPoints(testPoints, GrahamScan));

    // Quickhull
    Console.WriteLine("Quickhull - Random");

    foreach (IEnumerable<Point> testPoints in randomTestSets)
        Console.WriteLine(TimeAlgorithmWithPoints(testPoints, QuickHull));

    Console.WriteLine("Quickhull - Circle");

    foreach (IEnumerable<Point> testPoints in circleTestSets)
        Console.WriteLine(TimeAlgorithmWithPoints(testPoints, QuickHull));

    long TimeAlgorithmWithPoints(IEnumerable<Point> points, Func<IEnumerable<Point>, IEnumerable<Point>> algorithm)
    {
        Stopwatch stopwatch = new ();
        stopwatch.Start();

        algorithm(points);

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}

// Run an interactive version of the program
void RunInteractive()
{
    // Get points and algorithm
    IEnumerable<Point> inputPoints = GetPoints().ToList();
    Algorithm algorithm = ChooseAlgorithm();

    // Write point coordinates
    Console.WriteLine("Points:");
    foreach (Point point in inputPoints)
        Console.WriteLine($"{point.X}, {point.Y}");

    // Calculate convex hull
    IEnumerable<Point> convexHull = algorithm switch
    {
        Algorithm.GrahamScan => GrahamScan(inputPoints).ToList(),
        Algorithm.QuickHull => QuickHull(inputPoints).ToList(),
        _ => throw new NotImplementedException()
    };

    // Write point coordinates
    Console.WriteLine("Convex Hull:");
    foreach (Point point in convexHull)
        Console.WriteLine($"{point.X}, {point.Y}");

    // Draw image
    Console.WriteLine("Image:");
    DrawHull(inputPoints, convexHull);
}

// Get a random set of points, by a given method
IEnumerable<Point> GetPoints()
{
    Console.WriteLine("What point set type?");
    foreach (int i in Enum.GetValues(typeof(PointGeneratorMethod)))
    {
        Console.WriteLine($"({i}) | {Enum.GetName(typeof(PointGeneratorMethod), i)}");
    }

    string? input = Console.ReadLine();

    PointGeneratorMethod chosenMethod;
    while (!Enum.TryParse(input, out chosenMethod) || !Enum.IsDefined(chosenMethod))
        input = Console.ReadLine();

    Console.WriteLine("How many points?");

    input = Console.ReadLine();

    int n;
    while (!int.TryParse(input, out n))
        input = Console.ReadLine();

    return chosenMethod switch
    {
        PointGeneratorMethod.Random => GetRandomPoints(n),
        PointGeneratorMethod.Circle => GetPointsOnCircle(n),
        _ => throw new NotImplementedException()
    };
}

// Get n random points
IEnumerable<Point> GetRandomPoints(int n) => new int[n]
    .Select(_ => new Point((rnd.NextDouble() - 0.5) * (rightBound - leftBound),
        (rnd.NextDouble() - 0.5) * (topBound - bottomBound)))
    .Distinct().ToList();

// Get n random points that lie on a circle
IEnumerable<Point> GetPointsOnCircle(int n)
    =>
        new int[n]
            .Select(_ =>
            {
                double angle = 2.0 * Math.PI * rnd.NextDouble();
                return new Point(
                    topBound * Math.Cos(angle),
                    topBound * Math.Sin(angle));
            })
            .Distinct().ToList();

// Let the user choose the algorithm they want to use
Algorithm ChooseAlgorithm()
{
    Console.WriteLine("What algorithm?");
    foreach (int i in Enum.GetValues(typeof(Algorithm)))
    {
        Console.WriteLine($"({i}) | {Enum.GetName(typeof(Algorithm), i)}");
    }

    string? input = Console.ReadLine();

    Algorithm chosenAlgorithm;
    while (!Enum.TryParse(input, out chosenAlgorithm) || !Enum.IsDefined(chosenAlgorithm))
        input = Console.ReadLine();

    return chosenAlgorithm;
}

// Perform the graham scan algorithm
IEnumerable<Point> GrahamScan(IEnumerable<Point> points)
{
    Stack<Point> sortedPoints = new (points.OrderBy(p => p.X).ToList());
    Stack<Point> reversedPoints = new (sortedPoints.ToList());


    Stack<Point> upper = new ();
    Stack<Point> lower = new ();

    int lowerI = 0;
    int upperI = 0;

    while (sortedPoints.Count > 0)
    {
        lower.Push(sortedPoints.Pop());
        if (lowerI < 2)
        {
            lowerI++;
            continue;
        }

        bool done = false;

        while (!done && lower.Count > 2)
        {
            Point c = lower.Pop();
            Point b = lower.Pop();
            Point a = lower.Peek();

            // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise 
            // Second answer, check if counterclockwise, but strictly
            if (!(b.X * a.Y + c.X * b.Y + a.X * c.Y < a.X * b.Y + b.X * c.Y + c.X * a.Y))
            {
                lower.Push(b);
                done = true;
            }

            lower.Push(c);
        }
    }

    while (reversedPoints.Count > 0)
    {
        upper.Push(reversedPoints.Pop());
        if (upperI < 2)
        {
            upperI++;
            continue;
        }

        bool done = false;

        while (!done && upper.Count > 2)
        {
            Point c = upper.Pop();
            Point b = upper.Pop();
            Point a = upper.Peek();

            // https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise 
            // Second answer, check if counterclockwise, but strictly
            if (!(b.X * a.Y + c.X * b.Y + a.X * c.Y < a.X * b.Y + b.X * c.Y + c.X * a.Y))
            {
                upper.Push(b);
                done = true;
            }

            upper.Push(c);
        }
    }

    return lower.Concat(upper).Distinct();
}

// Perform the Quickhull algorithm
IEnumerable<Point> QuickHull(IEnumerable<Point> points)
{
    // Sort the points to find the leftmost and rightmost point.
    IEnumerable<Point> sortedPoints = points.OrderBy(p => p.X).ToList();

    Point leftMost = sortedPoints.First();
    Point rightMost = sortedPoints.Last();

    // Find a and b for the line through the left and right points for the form y = ax + b
    (double a, double b) = FindAB(leftMost, rightMost);

    List<Point> belowSet = new ();
    List<Point> aboveSet = new ();

    // Get all the points above and below this line
    foreach (Point point in sortedPoints.Skip(1).Take(sortedPoints.Count() - 2))
        if (point.Y > point.X * a + b) aboveSet.Add(point);
        else belowSet.Add(point);

    // Find the hull for both parts and return
    return FindHull(aboveSet, leftMost, rightMost, true).Concat(FindHull(belowSet, leftMost, rightMost, false))
        .Append(leftMost).Append(rightMost);
}

// Subroutine of Quickhull to find the hull of a subset of points between p and q
IEnumerable<Point> FindHull(IEnumerable<Point> points, Point p, Point q, bool top)
{
    points = points.ToList();

    // If empty set, return this empty set.
    if (!points.Any()) return points;

    // Find the furthest point from the line, this point must be on the hull
    Point furthestPoint = points.MaxBy(x => DistanceToLineSegment(x, p, q))!;

    (double leftA, double leftB) = FindAB(p, furthestPoint);
    (double rightA, double rightB) = FindAB(furthestPoint, q);

    List<Point> leftPoints = new ();
    List<Point> rightPoints = new ();

    // Get the points that lie to the left or right of the furthest point, and not inside the triangle formed by the furthest point and p and q
    foreach (Point point in points)
        if (point == furthestPoint)
        {
        }
        else if (point.X < furthestPoint.X && point.Y > leftA * point.X + leftB == top) leftPoints.Add(point);
        else if (point.Y > rightA * point.X + rightB == top) rightPoints.Add(point);

    // Go into recursion
    return FindHull(leftPoints, p, furthestPoint, top).Concat(FindHull(rightPoints, furthestPoint, q, top))
        .Append(furthestPoint);
}

// Helper function to find the a and b of the line through two points such that y = ax + b
(double a, double b) FindAB(Point point1, Point point2)
{
    double a = (point2.Y - point1.Y) / (point2.X - point1.X);
    double b = point1.Y - a * point1.X;

    return (a, b);
}

// Helper function to find the minimum between a point p and a line segment v-w
// https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
double DistanceToLineSegment(Point p, Point v, Point w)
{
    double DistanceBetweenPoints(Point p1, Point p2) => Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);


    double l2 = DistanceBetweenPoints(v, w);
    if (l2 == 0) return DistanceBetweenPoints(p, v);
    double t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
    t = Math.Max(0, Math.Min(1, t));
    return Math.Sqrt(DistanceBetweenPoints(new Point(p.X, p.Y), new Point
    (v.X + t * (w.X - v.X),
        v.Y + t * (w.Y - v.Y)
    )));
}

// Draw the hull in the console, doesn't work great for real values, but still usefull during debugging
void DrawHull(IEnumerable<Point> points, IEnumerable<Point> convexHullPoints)
{
    convexHullPoints = convexHullPoints.ToList();

    // Group the points by rounded y, to draw on each line
    List<IGrouping<int, Point>> sortedPoints = points.OrderBy(x => (int)x.Y).Reverse().GroupBy(x => (int)x.Y).ToList();
    int lastLine = topBound;

    // Draw each line
    foreach (IGrouping<int, Point> grouping in sortedPoints)
    {
        // Write empty lines for empty y values
        while (grouping.Key < lastLine)
        {
            Console.WriteLine(" ");
            lastLine--;
        }

        IEnumerable<Point> sortedPointsOnLine = grouping.OrderBy(x => x.X).ToList();

        int lastChar = leftBound - 1;

        // Write all points in the line
        foreach (Point point in sortedPointsOnLine)
        {
            // Write empty characters for empty x values
            while (lastChar < point.X)
            {
                Console.Write(' ');
                lastChar++;
            }

            // They must be equal anyway
            // ReSharper disable twice CompareOfFloatsByEqualityOperator
            Console.Write(convexHullPoints.Any(x => x.X == point.X && x.Y == point.Y) ? 'X' : 'O');
            lastChar++;
        }

        Console.WriteLine();
        lastLine--;
    }
}

public record Point(double X, double Y);

public enum Algorithm
{
    GrahamScan,
    QuickHull
}

public enum PointGeneratorMethod
{
    Random,
    Circle
}