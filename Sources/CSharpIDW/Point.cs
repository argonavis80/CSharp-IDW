using System.Globalization;
using System.Linq;

namespace CSharpIDW
{
    public struct Point
    {
        public double Value { get; }

        public double[] Coordinates { get; }

        public Point(double value, params double[] coordinates)
        {
            Value = value;
            Coordinates = coordinates;
        }

        public override string ToString()
        {
            return $"{string.Join(";", Coordinates)} -> {Value}";
        }
    }
}