using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable PossibleInvalidOperationException

namespace CSharpIDW.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IdwInterpolatorTests
    {
        private List<Point> _pointRange;

        [TestInitialize]
        public void Setup()
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailDimensionOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            var target = new IdwInterpolator(dimensions: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailPowerOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            var target = new IdwInterpolator(dimensions: 2, power: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailNumberOfNeighboursOutOfRange()
        {
            // ReSharper disable once UnusedVariable
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void FailWithTooFewPoints()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPointRange(_pointRange.Take(4).ToList()); // Not enough points.

            target.Interpolate(1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailWithNullPointRange()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPointRange(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FailWithPointWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPoint(new Point(1, 1, 1, 1)); // One dimension too much.
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailWithPointAndMissingCoordinates()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPoint(1, null); // Coordinates missing.
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FailWithPointAndCoordinatesWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPoint(1, 1, 1, 1); // One dimension too much.
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
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

            target.AddPointRange(pointRange);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void FailInterpolateWithWrongDimension()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 5);

            target.AddPointRange(_pointRange);

            target.Interpolate(1, 1, 1); // One dimension to much.
        }

        [TestMethod]
        public void TestInterpolateWithPointHit()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 4);

            target.AddPointRange(_pointRange); // Test adding point range.

            var result = target.Interpolate(1, 1);

            Assert.AreEqual(2.2, result.Value);
            Assert.AreEqual(1, result.Point.Value.Coordinates[0]);
            Assert.AreEqual(1, result.Point.Value.Coordinates[1]);
            Assert.AreEqual(InterpolationResult.ResultOptions.Hit, result.Result);
        }

        [TestMethod]
        public void TestNearestNeighbourInterpolation()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 1);

            foreach (var point in _pointRange)
            {
                target.AddPoint(point); // Test adding points.
            }

            var result = target.Interpolate(1.1, 1.1);

            Assert.AreEqual(2.2, result.Value);
            Assert.AreEqual(1, result.Point.Value.Coordinates[0]);
            Assert.AreEqual(1, result.Point.Value.Coordinates[1]);
            Assert.AreEqual(InterpolationResult.ResultOptions.NearestNeighbor, result.Result);
        }

        

        [TestMethod]
        public void TestWeightedInterpolation()
        {
            var target = new IdwInterpolator(dimensions: 2, power: 2, numberOfNeighbours: 4);

            foreach (var point in _pointRange)
            {
                target.AddPoint(point.Value, point.Coordinates); // Test adding bare values.
            }

            var result = target.Interpolate(1.5, 1.5);

            Assert.AreEqual(2.875, result.Value, 0.001);
            Assert.AreEqual(InterpolationResult.ResultOptions.Interpolated, result.Result);
            Assert.IsNull(result.Point);
        }

        //[TestMethod]
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
