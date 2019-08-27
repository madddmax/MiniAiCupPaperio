using System.Collections.Generic;

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

        public static double Distance(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }

        public static bool IsOnTheWorld(Point point)
        {
            return point.X >= World.MinX && point.X <= World.MaxX &&
                   point.Y >= World.MinY && point.Y <= World.MaxY;
        }

        public static int GetPath(Point start, string direction, Point end)
        {
            int path = 0;
            int deltaX = end.X - start.X;
            int deltaY = end.Y - start.Y;
            int penaltyPath = 2 * World.Width;

            // движение направо
            if (deltaX > 0)
            {
                path += direction == Direction.Left && deltaY == 0 ? deltaX + penaltyPath : deltaX;
            }

            // движение налево
            if (deltaX < 0)
            {
                path += direction == Direction.Right && deltaY == 0 ? -deltaX + penaltyPath : -deltaX;
            }

            // движение вверх
            if (deltaY > 0)
            {
                path += direction == Direction.Down && deltaX == 0 ? deltaY + penaltyPath : deltaY;
            }

            // движение вниз
            if (deltaY < 0)
            {
                path += direction == Direction.Up && deltaX == 0 ? -deltaY + penaltyPath : -deltaY;
            }

            return path;
        }
    }
}
