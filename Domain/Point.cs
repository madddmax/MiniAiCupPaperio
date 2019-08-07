using System.Collections.Generic;

namespace MiniAiCupPaperio
{
    public class Point
    {
        public int X;

        public int Y;

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

        public static bool IsInPolygon(List<Point> poly, Point pnt)
        {
            int i, j;
            int nvert = poly.Count;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((poly[i].Y > pnt.Y) != (poly[j].Y > pnt.Y)) &&
                    (pnt.X < (poly[j].X - poly[i].X) * (pnt.Y - poly[i].Y) / (poly[j].Y - poly[i].Y) + poly[i].X))
                    c = !c;
            }
            return c;
        }
    }
}
