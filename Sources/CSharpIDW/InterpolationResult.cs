namespace CSharpIDW
{
    public class InterpolationResult
    {
        public enum ResultOptions
        {
            Hit,
            NearestNeighbor,
            Interpolated,
            Extrapolated,
            OutOfBounds
        }

        public ResultOptions Result { get; set; }

        public double Value { get; set; }

        public Point? Point { get; set; }
    }
}