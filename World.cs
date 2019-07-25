namespace MiniAiCupPaperio
{
    public class World
    {
        public int XCount { get; set; }

        public int YCount { get; set; }

        public int Speed { get; set; }

        public int Width { get; set; }

        public int HalfWidth => Width / 2;

        public int MinX => HalfWidth;

        public int MaxX => XCount * Width - HalfWidth;

        public int MinY => HalfWidth;

        public int MaxY => YCount * Width - HalfWidth;
    }
}
