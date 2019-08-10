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
            }
            else if(myNext.Direction == Direction.Right)
            {
                myNext.Position.X += World.Width;
                if (myNext.Position.X > World.MaxX)
                {
                    return null;
                }
            }
            else if (myNext.Direction == Direction.Up)
            {
                myNext.Position.Y += World.Width;
                if (myNext.Position.Y > World.MaxY)
                {
                    return null;
                }
            }
            else if (myNext.Direction == Direction.Down)
            {
                myNext.Position.Y -= World.Width;
                if (myNext.Position.Y < World.MinY)
                {
                    return null;
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
            var depthModificator = depth < 3 ? depth : 3;
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
                var maxX = myNext.Lines.Max(l => l.X);
                var maxY = myNext.Lines.Max(l => l.Y);
                var minX = myNext.Lines.Min(l => l.X);
                var minY = myNext.Lines.Min(l => l.Y);

                var captured = new List<Point>();
                for (var i = maxX; i >= minX; i -= World.Width)
                {
                    for (var j = maxY; j >= minY; j -= World.Width)
                    {
                        var point = new Point(i, j);
                        if (!MyTerritory.Contains(point) &&
                            Point.InPolygon(myNext.Lines.ToList(), point))
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

            if (onMyTerritory)
            {
                myNext.Score -= 1;
            }

            return myNext;
        }
    }
}
