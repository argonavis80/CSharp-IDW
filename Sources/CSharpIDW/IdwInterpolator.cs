using System;
using System.Collections.Generic;
using System.Linq;
using KdTree;
using KdTree.Math;
// ReSharper disable SuggestBaseTypeForParameter

namespace CSharpIDW
{
    /// <summary>
    /// Inverse Distance Weighting (IDW) interpolator. 
    /// </summary>
    /// <remarks>
    /// The interpolator implements the modified Shepard's method, with only nearest neighbours.
    /// https://en.wikipedia.org/wiki/Inverse_distance_weighting
    /// </remarks>
    public class IdwInterpolator
    {
        private readonly int _dimensions;

        private const double DefaultPower = 2;
        private const int DefaultNumberOfNeighbours = 5;

        private readonly KdTree<double, Point> _tree;

        private double Power { get; }

        private int NumberOfNeighbours { get; }

        public IdwInterpolator(int dimensions) : this(dimensions, DefaultPower)
        {
        }

        public IdwInterpolator(int dimensions, double power) : this(dimensions, power, DefaultNumberOfNeighbours)
        {
        }

        public IdwInterpolator(int dimensions, double power, int numberOfNeighbours)
        {
            if (dimensions < 1)
                throw new ArgumentOutOfRangeException(nameof(dimensions), dimensions,
                    $"Parameter '{nameof(dimensions)}' must be a positive integer.");

            if (power <= 0)
                throw new ArgumentOutOfRangeException(nameof(power), power,
                    $"Parameter '{nameof(power)}' must be a positive real value.");

            if (numberOfNeighbours < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfNeighbours), numberOfNeighbours,
                    $"Parameter '{nameof(numberOfNeighbours)}' must be a positive integer.");

            _dimensions = dimensions;

            Power = power;
            NumberOfNeighbours = numberOfNeighbours;

            _tree = new KdTree<double, Point>(dimensions, new DoubleMath());
        }

        public void AddPoint(double value, params double[] coordinates)
        {
            if (coordinates == null)
                throw new ArgumentNullException(nameof(coordinates));

            if (coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(coordinates),
                    $"Size of {nameof(coordinates)} must match the dimension of the interpolator.");

            var point = new Point(value, coordinates);

            _tree.Add(coordinates, point);
        }

        public void AddPoint(Point point)
        {
            if (point.Coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(point.Coordinates),
                    $"Size of {nameof(point.Coordinates)} must match the dimension of the interpolator.");

            _tree.Add(point.Coordinates, point);
        }

        public void AddPointRange(IEnumerable<Point> points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));

            foreach (var point in points)
            {
                if (point.Coordinates.Length != _dimensions)
                {
                    _tree.Clear();

                    throw new ArgumentException(nameof(points),
                        $"Size of coordinates of all items in {nameof(points)} must match the dimension of the interpolator.");
                }

                _tree.Add(point.Coordinates, point);
            }
        }

        public InterpolationResult Interpolate(params double[] coordinates)
        {
            if (coordinates.Length != _dimensions)
                throw new ArgumentException(nameof(coordinates),
                    $"Size of {nameof(coordinates)} must match the dimension of the interpolator.");

            if (_tree.Count < NumberOfNeighbours)
            {
                throw new IndexOutOfRangeException(
                    $"The number of found points ({_tree.Count}) is less than " +
                    $"the number of required neighbours ({NumberOfNeighbours}). Consider reducing " +
                    $"the required number of Neighbours with '{nameof(NumberOfNeighbours)}' property.");
            }

            Point point;

            if (_tree.TryFindValueAt(coordinates, out point))
            {
                return new InterpolationResult
                {
                    Point = point,
                    Value = point.Value,
                    Result = InterpolationResult.ResultOptions.Hit
                };
            }

            var neighbours = _tree.GetNearestNeighbours(coordinates, NumberOfNeighbours);

            if (neighbours.Length == 1)
            {
                point = neighbours[0].Value;

                return new InterpolationResult
                {
                    Point = point,
                    Value = point.Value,
                    Result = InterpolationResult.ResultOptions.NearestNeighbor
                };
            }

            var value = CalculateWeightedAverage(neighbours.Select(n => n.Value), coordinates);

            var result = new InterpolationResult
            {
                Value = value,
                Result = InterpolationResult.ResultOptions.Interpolated
            };

            if (false) // TODO: Check convex hull here.
            {
                result.Result = InterpolationResult.ResultOptions.Extrapolated;
            }

            return result;
        }

        private double CalculateWeightedAverage(IEnumerable<Point> points, double[] target)
        {
            var nominator = 0.0;
            var denominator = 0.0;

            foreach (var point in points)
            {
                var distance = CalculateDistance(point.Coordinates, target);
                var weight = 1/Math.Pow(distance, Power);

                nominator += weight*point.Value;
                denominator += weight;
            }

            var result = nominator/denominator;

            return result;
        }

        private static double CalculateDistance(double[] pointA, double[] pointB)
        {
            var sum = 0.0;

            for (var i = 0; i < pointA.Length; i++)
            {
                var p = pointA[i];
                var q = pointB[i];

                sum += Math.Pow(p - q, 2);
            }

            var result = Math.Sqrt(sum);

            return result;
        }
    }
}
