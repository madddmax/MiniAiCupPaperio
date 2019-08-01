using System;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Direction
    {
        public const string Left = "left";
        public const string Right = "right";
        public const string Up = "up";
        public const string Down = "down";
        public static string[] All = {Left, Right, Up, Down};

        static Random rnd = new Random();
        public static string[] GetPossible(string current)
        {
            if (current == Left)
            {
                return new[] {Left, Up, Down}.OrderBy(x => rnd.Next()).ToArray();
            }

            if (current == Right)
            {
                return new[] {Right, Up, Down}.OrderBy(x => rnd.Next()).ToArray();
            }

            if (current == Up)
            {
                return new[] {Left, Right, Up}.OrderBy(x => rnd.Next()).ToArray();
            }

            if (current == Down)
            {
                return new[] {Left, Right, Down}.OrderBy(x => rnd.Next()).ToArray();
            }

            return All.OrderBy(x => rnd.Next()).ToArray();
        }
    }
}