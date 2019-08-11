namespace MiniAiCupPaperio
{
    public static class DirectionExtension
    {
        public static string[] All = { Direction.Left, Direction.Right, Direction.Up, Direction.Down };

        public static string[] GetPossible(string current)
        {
            if (current == Direction.Left)
            {
                return new[] { Direction.Left, Direction.Up, Direction.Down };
            }

            if (current == Direction.Right)
            {
                return new[] { Direction.Right, Direction.Up, Direction.Down };
            }

            if (current == Direction.Up)
            {
                return new[] { Direction.Left, Direction.Right, Direction.Up };
            }

            if (current == Direction.Down)
            {
                return new[] { Direction.Left, Direction.Right, Direction.Down };
            }

            return All;
        }

        public static bool IsOpposite(string d1, string d2)
        {
            if ((d1 == Direction.Left && d2 == Direction.Right) ||
                (d1 == Direction.Right && d2 == Direction.Left) ||
                (d1 == Direction.Up && d2 == Direction.Down) ||
                (d1 == Direction.Down && d2 == Direction.Up))
            {
                return true;
            }

            return false;
        }
    }
}
