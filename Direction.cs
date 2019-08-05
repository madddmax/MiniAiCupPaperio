namespace MiniAiCupPaperio
{
    public static class Direction
    {
        public const string Left = "left";
        public const string Right = "right";
        public const string Up = "up";
        public const string Down = "down";
        public static string[] All = {Left, Right, Up, Down};

        public static string[] GetPossible(string current)
        {
            if (current == Left)
            {
                return new[] {Left, Up, Down};
            }

            if (current == Right)
            {
                return new[] {Right, Up, Down};
            }

            if (current == Up)
            {
                return new[] {Left, Right, Up};
            }

            if (current == Down)
            {
                return new[] {Left, Right, Down};
            }

            return All;
        }
    }
}