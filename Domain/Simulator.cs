using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
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

            myNext.Score += MacroLevel.GetDirectionScore(myNext.Position, myNext.Direction);

            var prevOnMyTerritory = Global.MyTerritory.Contains(my.Position);
            if (!prevOnMyTerritory)
            {
                // вне своей территорий - увеличиваем шлейф
                myNext.Lines.Add(my.Position);
            }

            var onMyTerritory = Global.MyTerritory.Contains(myNext.Position);
            double myAverageSpeed = GetAverageSpeed(myNext.Bonus, depth);
            foreach (var e in Global.Enemies)
            {
                double enemyAverageSpeed = GetAverageSpeed(e.Bonus, depth);

                foreach (var l in myNext.Lines)
                {
                    int enemyPath = PointExtension.GetPath(e.Position, e.Direction, l);
                    int myPath = depth * World.Width + World.Width;

                    if (enemyPath / enemyAverageSpeed - myPath / myAverageSpeed < 1)
                    {
                        // страх пересечения шлейфа
                        myNext.Score -= 500;
                    }
                }

                if (e.Lines.Count <= myNext.Lines.Count)
                {
                    int enemyPath = PointExtension.GetPath(e.Position, e.Direction, myNext.Position);
                    int myPath = depth * World.Width + World.Width;

                    if (enemyPath / enemyAverageSpeed - myPath / myAverageSpeed < 1)
                    {
                        // страх столкновения с головой
                        myNext.Score -= 500;
                    }
                }

                if (e.Lines.Contains(myNext.Position))
                {
                    // пересекает шлейф противника
                    myNext.Score += 50;
                }
            }

            foreach (var bonus in Global.MapBonuses)
            {
                if (bonus.Position.Equals(myNext.Position))
                {
                    myNext.Score += GetBonusScore(bonus, myNext.Direction, false, myNext.Position);
                }
            }

            if (onMyTerritory && myNext.Lines.Count > 0)
            {
                // завершает шлейф на своей территории
                var polygon = new List<Point>(myNext.Lines);
                var firstPosition = myNext.Lines.First();
                var position = myNext.Position;
                for (int i = 0; i < 30; i++)
                {
                    polygon.Add(position);

                    var myBorder = new List<Point>();
                    foreach (var n in PointExtension.GetNeighboring(position))
                    {
                        if (n.Equals(firstPosition))
                        {
                            i = 30;
                            break;
                        }

                        if (Global.MyTerritory.Contains(n))
                        {
                            myBorder.Add(n);
                        }
                    }

                    if (myBorder.Count > 0)
                    {
                        position = myBorder.OrderBy(b => PointExtension.Distance(b, firstPosition)).First();
                    }
                    else
                    {
                        break;
                    }
                }

                var maxX = polygon.Max(l => l.X);
                var maxY = polygon.Max(l => l.Y);
                var minX = polygon.Min(l => l.X);
                var minY = polygon.Min(l => l.Y);

                var captured = new HashSet<Point>();
                for (var i = maxX; i >= minX; i -= World.Width)
                {
                    for (var j = maxY; j >= minY; j -= World.Width)
                    {
                        var point = new Point(i, j);
                        if (!Global.MyTerritory.Contains(point) &&
                            PointExtension.InPolygon(polygon, point))
                        {
                            captured.Add(point);
                        }
                    }
                }
                captured.ExceptWith(myNext.Lines);

                foreach (var p in captured)
                {
                    int captureScore = 1;
                    if (Global.EnemyTerritory.Contains(p))
                    {
                        captureScore = 5;
                    }

                    myNext.Score += captureScore;

                    foreach (var bonus in Global.MapBonuses)
                    {
                        if (bonus.Position.Equals(p))
                        {
                            myNext.Score += GetBonusScore(bonus, myNext.Direction, true, myNext.Position);
                        }
                    }
                }

                myNext.HasCapture = true;
            }

            return myNext;
        }

        public static int GetBonusScore(MapBonus bonus, string direction, bool isCaptured, Point player)
        {
            if (bonus.Type == Bonus.Nitro)
            {
                return bonus.ActiveTicks;
            }
            if (bonus.Type == Bonus.Slow)
            {
                return -2 * bonus.ActiveTicks;
            }

            var sawScore = 0;
            var firePosition = isCaptured ? player : bonus.Position;
            foreach (var e in Global.Enemies)
            {
                if (direction == Direction.Left)
                {
                    sawScore += e.Territory.Any(t => t.Y == firePosition.Y && t.X < firePosition.X) ? 30 : 0;
                }
                else if(direction == Direction.Right)
                {
                    sawScore += e.Territory.Any(t => t.Y == firePosition.Y && t.X > firePosition.X) ? 30 : 0;
                }
                else if (direction == Direction.Up)
                {
                    sawScore += e.Territory.Any(t => t.X == firePosition.X && t.Y > firePosition.Y) ? 30 : 0;
                }
                else if (direction == Direction.Down)
                {
                    sawScore += e.Territory.Any(t => t.X == firePosition.X && t.Y < firePosition.Y) ? 30 : 0;
                }
            }


            return sawScore;
        }

        public static double GetAverageSpeed(PlayerBonus bonus, int depth)
        {
            if (bonus == null)
            {
                return 5;
            }

            if (bonus.Type == Bonus.Nitro)
            {
                int speedSum = 0;
                for (int t = 1; t <= depth; t++)
                {
                    if (t <= bonus.Ticks)
                    {
                        speedSum += 6;
                    }
                    else
                    {
                        speedSum += 5;
                    }
                }

                return (double)speedSum / depth;
            }

            if (bonus.Type == Bonus.Slow)
            {
                int speedSum = 0;
                for (int t = 1; t <= depth; t++)
                {
                    if (t <= bonus.Ticks)
                    {
                        speedSum += 3;
                    }
                    else
                    {
                        speedSum += 5;
                    }
                }

                return (double)speedSum / depth;
            }

            return 5;
        }
    }
}
