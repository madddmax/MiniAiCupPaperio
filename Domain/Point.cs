using System;
using System.Collections.Generic;

namespace MiniAiCupPaperio
{
    public class Point : IEquatable<Point>
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

        public static bool InPolygon(List<Point> polygon, Point pnt)
        {
            bool c = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > pnt.Y) != (polygon[j].Y > pnt.Y)) &&
                    (pnt.X < (polygon[j].X - polygon[i].X) * (pnt.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) +
                     polygon[i].X))
                {
                    c = !c;
                }
            }
            return c;
        }

        public bool Equals(Point other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Y == other.Y && X == other.X;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Y * 397) ^ X;
            }
        }
    }
}
