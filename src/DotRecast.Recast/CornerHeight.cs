namespace DotRecast.Recast
{
    public class CornerHeight
    {
        public readonly int height;
        public readonly bool borderVertex;

        public CornerHeight(int height, bool borderVertex)
        {
            this.height = height;
            this.borderVertex = borderVertex;
        }
    }
}