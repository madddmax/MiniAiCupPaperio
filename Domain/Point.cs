using System;
using System.Collections.Generic;

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

        public static List<Point> GetVertAndHoriz(Point point)
        {
            return new List<Point>
            {
                new Point(point.X, point.Y + World.Width),
                new Point(point.X - World.Width, point.Y),
                new Point(point.X, point.Y - World.Width),
                new Point(point.X + World.Width, point.Y)
            };
        }

        public static bool InPolygon(List<Point> polygon, Point point)
        {
            bool c = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y)) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) +
                     polygon[i].X))
                {
                    c = !c;
                }
            }
            return c;
        }
    }
}
