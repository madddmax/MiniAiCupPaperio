using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static List<MapBonus> Bonuses = new List<MapBonus>();
        public static List<Player> Enemies = new List<Player>();
        public static HashSet<Point> MyTerritory = new HashSet<Point>();
        public static HashSet<Point> EnemyTerritory = new HashSet<Point>();

        public static Player GetNext(Player my, string direction, int depth)
        {
            var myNext = (Player) my.Clone();
            myNext.Direction = direction;

            if (myNext.Direction == Direction.Left)
            {
                myNext.Position.X -= World.Width;
                if (myNext.Position.X < World.MinX)
                {
                    return null;
                }

                if (myNext.Position.X <= World.MinX + World.Width)
                {
                    // отталкивание от границы
                    myNext.Score -= 5;
                }
                else if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X - World.Width, myNext.Position.Y)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X - World.Width * 2, myNext.Position.Y)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }
            }
            else if(myNext.Direction == Direction.Right)
            {
                myNext.Position.X += World.Width;
                if (myNext.Position.X > World.MaxX)
                {
                    return null;
                }

                if (myNext.Position.X >= World.MaxX - World.Width)
                {
                    // отталкивание от границы
                    myNext.Score -= 5;
                }
                else if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X + World.Width, myNext.Position.Y)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X + World.Width * 2, myNext.Position.Y)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }
            }
            else if (myNext.Direction == Direction.Up)
            {
                myNext.Position.Y += World.Width;
                if (myNext.Position.Y > World.MaxY)
                {
                    return null;
                }

                if (myNext.Position.Y >= World.MaxY - World.Width)
                {
                    // отталкивание от границы
                    myNext.Score -= 5;
                }
                else if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y - World.Width)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y - World.Width * 2)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }
            }
            else if (myNext.Direction == Direction.Down)
            {
                myNext.Position.Y -= World.Width;
                if (myNext.Position.Y < World.MinY)
                {
                    return null;
                }

                if (myNext.Position.Y <= World.MinY + World.Width)
                {
                    // отталкивание от границы
                    myNext.Score -= 5;
                }
                else if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y + World.Width)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y + World.Width * 2)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }
            }

            if (myNext.Lines.Contains(myNext.Position))
            {
                // пересекает свой шлейф
                return null;
            }

            var prevOnMyTerritory = MyTerritory.Contains(my.Position);
            if (!prevOnMyTerritory)
            {
                // вне своей территорий - увеличиваем шлейф
                myNext.Lines.Add(my.Position);
            }

            var onMyTerritory = MyTerritory.Contains(myNext.Position);
            var depthModificator = depth < 5 ? depth : 5;
            foreach (var e in Enemies)
            {
                if (myNext.Lines.Any(l => Math.Abs(l.X - e.Position.X) + Math.Abs(l.Y - e.Position.Y) <= World.Width * (depthModificator + 2)))
                {
                    // страх пересечения шлейфа
                    myNext.Score -= 500;
                }

                if (!onMyTerritory &&
                    Math.Abs(myNext.Position.X - e.Position.X) + Math.Abs(myNext.Position.Y - e.Position.Y) <= World.Width * (depthModificator + 2))
                {
                    // страх столкновения с головой
                    myNext.Score -= 500;
                }

                if (e.Lines.Contains(myNext.Position))
                {
                    // пересекает шлейф противника
                    myNext.Score += 50;
                }
            }

            foreach (var bonus in Bonuses)
            {
                if (bonus.Position.Equals(myNext.Position))
                {
                    if (bonus.Type == Bonus.Nitro)
                    {
                        myNext.Score += 10;
                    }
                    else if (bonus.Type == Bonus.Slow)
                    {
                        myNext.Score -= 10;
                    }
                    else if (bonus.Type == Bonus.Saw)
                    {
                        myNext.Score += 25;
                    }
                }
            }

            if (onMyTerritory && myNext.Lines.Count > 0)
            {
                // завершает шлейф на своей территории
                var polygon = new List<Point>(myNext.Lines);

                var maxX = polygon.Max(l => l.X);
                var maxY = polygon.Max(l => l.Y);
                var minX = polygon.Min(l => l.X);
                var minY = polygon.Min(l => l.Y);

                var captured = new List<Point>();
                for (var i = maxX; i >= minX; i -= World.Width)
                {
                    for (var j = maxY; j >= minY; j -= World.Width)
                    {
                        var point = new Point(i, j);
                        if (!MyTerritory.Contains(point) &&
                            PointExtension.InPolygon(polygon, point))
                        {
                            captured.Add(point);
                        }
                    }
                }

                foreach (var p in captured)
                {
                    int captureScore = 1;
                    if (EnemyTerritory.Contains(p))
                    {
                        captureScore = 5;
                    }

                    myNext.Score += captureScore;
                }

                myNext.Lines = new HashSet<Point>();
                myNext.HasCapture = true;
            }

            return myNext;
        }
    }
}
