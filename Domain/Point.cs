using System;

namespace MiniAiCupPaperio
{
    public struct Point : IEquatable<Point>
    {
        public int X;

        public int Y;

        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public Point(int[] point)
        {
            X = point[0];
            Y = point[1];
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }
}
