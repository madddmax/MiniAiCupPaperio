﻿using System.Collections.Generic;

namespace MiniAiCupPaperio
{
    public static class PointExtension
    {
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

        public static List<Point> GetDiagonals(Point point)
        {
            return new List<Point>
            {
                new Point(point.X + World.Width, point.Y + World.Width),
                new Point(point.X - World.Width, point.Y + World.Width),
                new Point(point.X + World.Width, point.Y - World.Width),
                new Point(point.X - World.Width, point.Y - World.Width)
            };
        }

        public static List<Point> GetNeighboring(Point point)
        {
            var neighboring = GetVertAndHoriz(point);
            neighboring.AddRange(GetDiagonals(point));
            return neighboring;
        }

        public static List<Point> GetBoundary(HashSet<Point> territory)
        {
            var boundary = new List<Point>();
            foreach (var point in territory)
            {
                var neighboring = GetNeighboring(point);
                foreach (var n in neighboring)
                {
                    if (territory.Contains(n) || boundary.Contains(n))
                    {
                        continue;
                    }

                    boundary.Add(n);
                }
            }

            return boundary;
        }
    }
}