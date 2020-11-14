using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable PossibleInvalidOperationException

namespace CSharpIDW.Test
{
    [ExcludeFromCodeCoverage]
    public class IdwInterpolatorTests
    {
        private List<Point> _pointRange;

        public IdwInterpolatorTests()
        {
            _pointRange = new List<Point>
            {
                new Point(1.0, 0, 0),
                new Point(1.1, 0, 1),
                new Point(1.2, 0, 2),
                new Point(2.0, 1, 0),
                new Point(2.2, 1, 1),
                new Point(2.4, 1, 2),
                new Point(3.0, 2, 0),
                new Point(3.3, 2, 1),
                new Point(3.6, 2, 2),
            };
        }

        [Fact]
        public void FailDimensionOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentOutOfRangeException>(() => new IdwInterpolator(dimensions: 0));
        }

        [Fact]
        public void FailPowerOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentOutOfRangeException>(() => new IdwInterpolator(dimensions: 2, power: 0));
        }

        [Fact]
        public void FailNumberOfNeighboursOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            Assert.Throws<ArgumentOutOfRangeException>(() => new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 0));
        }

        [Fact]
        public void FailWithTooFewPoints()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPointRange(_pointRange.Take(4).ToList()); // Not enough points.

            Assert.Throws<IndexOutOfRangeException>(() => target.Interpolate(1, 1));
        }

        [Fact]
        public void FailWithNullPointRange()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            Assert.Throws<ArgumentNullException>(() => target.AddPointRange(null));
        }

        [Fact]
        public void FailWithPointWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            Assert.Throws<ArgumentException>(() => target.AddPoint(new Point(1, 1, 1, 1))); // One dimension too much.
        }

        [Fact]
        public void FailWithPointAndMissingCoordinates()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            Assert.Throws<ArgumentNullException>(() => target.AddPoint(1, null)); // Coordinates missing.
        }

        [Fact]
        public void FailWithPointAndCoordinatesWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            Assert.Throws<ArgumentException>(() => target.AddPoint(1, 1, 1, 1)); // One dimension too much.
        }

        [Fact]
        public void FailWithPointRangeWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            var pointRange = new List<Point>
            {
                new Point(1.0, 0, 0),
                new Point(1.1, 0, 1),
                new Point(1.2, 0, 2),
                new Point(2.0, 1, 0),
                new Point(2.2, 1, 1),
                new Point(2.4, 1) // Dimension wrong.
            };

            Assert.Throws<ArgumentException>(() => target.AddPointRange(pointRange));
        }

        [Fact]
        public void FailInterpolateWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPointRange(_pointRange);

            Assert.Throws<ArgumentException>(() => target.Interpolate(1, 1, 1)); // One dimension to much.
        }

        [Fact]
        public void TestInterpolateWithPointHit()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 4);

            target.AddPointRange(_pointRange); // Test adding point range.

            var result = target.Interpolate(1, 1);

            Assert.Equal(2.2, result.Value);
            Assert.Equal(1, result.Point.Value.Coordinates[0]);
            Assert.Equal(1, result.Point.Value.Coordinates[1]);
            Assert.Equal(InterpolationResult.ResultOptions.Hit, result.Result);
        }

        [Fact]
        public void TestNearestNeighbourInterpolation()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 1);

            foreach (var point in _pointRange)
            {
                target.AddPoint(point); // Test adding points.
            }

            var result = target.Interpolate(1.1, 1.1);

            Assert.Equal(2.2, result.Value);
            Assert.Equal(1, result.Point.Value.Coordinates[0]);
            Assert.Equal(1, result.Point.Value.Coordinates[1]);
            Assert.Equal(InterpolationResult.ResultOptions.NearestNeighbor, result.Result);
        }

        

        [Fact]
        public void TestWeightedInterpolation()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 4);

            foreach (var point in _pointRange)
            {
                target.AddPoint(point.Value, point.Coordinates); // Test adding bare values.
            }

            var result = target.Interpolate(1.5, 1.5);

            Assert.Equal(2.875, result.Value, 3);
            Assert.Equal(InterpolationResult.ResultOptions.Interpolated, result.Result);
            Assert.Null(result.Point);
        }

        //[Fact]
        //public void TestWeightedExtrapolation()
        //{
        //    // TODO: Currently failing, because convex hull not yet checked.

        //    var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 4);

        //    target.AddPointRange(_pointRange);

        //    var result = target.Interpolate(2.1, 2.1);

        //    Assert.AreEqual(2.2, result.Value);
        //    Assert.AreEqual(InterpolationResult.ResultOptions.Extrapolated, result.Result);
        //    Assert.IsNull(result.Point);
        //}
    }
}
