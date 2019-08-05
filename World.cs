namespace MiniAiCupPaperio
{
    public static class World
    {
        public static int XCount { get; set; }

        public static int YCount { get; set; }

        public static int Speed { get; set; }

        public static int Width { get; set; }

        public static int HalfWidth => Width / 2;

        public static int OneMoveTicks => Width / Speed;

        public static int MinX => HalfWidth;

        public static int MaxX => XCount * Width - HalfWidth;

        public static int MinY => HalfWidth;

        public static int MaxY => YCount * Width - HalfWidth;
    }
}
