using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static List<MapBonus> MapBonuses = new List<MapBonus>();
        public static List<EnemyPlayer> Enemies = new List<EnemyPlayer>();
        public static HashSet<Point> MyTerritory = new HashSet<Point>();
        public static HashSet<Point> EnemyTerritory = new HashSet<Point>();

        public static int LeftScore;
        public static int RightScore;
        public static int UpScore;
        public static int DownScore;
        public static int TotalScore;

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

                if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X - World.Width, myNext.Position.Y)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X - World.Width * 2, myNext.Position.Y)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }

                myNext.Score += GetDirectionScore(LeftScore);
            }
            else if(myNext.Direction == Direction.Right)
            {
                myNext.Position.X += World.Width;
                if (myNext.Position.X > World.MaxX)
                {
                    return null;
                }

                if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X + World.Width, myNext.Position.Y)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X + World.Width * 2, myNext.Position.Y)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }

                myNext.Score += GetDirectionScore(RightScore);
            }
            else if (myNext.Direction == Direction.Up)
            {
                myNext.Position.Y += World.Width;
                if (myNext.Position.Y > World.MaxY)
                {
                    return null;
                }

                if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y - World.Width)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y - World.Width * 2)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }

                myNext.Score += GetDirectionScore(UpScore);
            }
            else if (myNext.Direction == Direction.Down)
            {
                myNext.Position.Y -= World.Width;
                if (myNext.Position.Y < World.MinY)
                {
                    return null;
                }

                if (MyTerritory.Contains(myNext.Position) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y + World.Width)) &&
                         MyTerritory.Contains(new Point(myNext.Position.X, myNext.Position.Y + World.Width * 2)))
                {
                    // отталкивание от захваченной территории
                    myNext.Score -= 1;
                }

                myNext.Score += GetDirectionScore(DownScore);
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
            double myAverageSpeed = GetAverageSpeed(myNext.Bonus, depth);
            foreach (var e in Enemies)
            {
                double enemyAverageSpeed = GetAverageSpeed(e.Bonus, depth);

                foreach (var l in myNext.Lines)
                {
                    int enemyPath = GetPath(e.Position, e.Direction, l);
                    int myPath = depth * World.Width + World.Width;

                    if (enemyPath / enemyAverageSpeed - myPath / myAverageSpeed < 1)
                    {
                        // страх пересечения шлейфа
                        myNext.Score -= 500;
                    }
                }

                if (e.Lines.Count <= myNext.Lines.Count)
                {
                    int enemyPath = GetPath(e.Position, e.Direction, myNext.Position);
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

            foreach (var bonus in MapBonuses)
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

                        if (MyTerritory.Contains(n))
                        {
                            myBorder.Add(n);
                        }
                    }

                    if (myBorder.Count > 0)
                    {
                        position = myBorder.OrderBy(b => Distance(b, firstPosition)).First();
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
                        if (!MyTerritory.Contains(point) &&
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
                    if (EnemyTerritory.Contains(p))
                    {
                        captureScore = 5;
                    }

                    myNext.Score += captureScore;

                    foreach (var bonus in MapBonuses)
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

        private static int GetDirectionScore(int score)
        {
            var ratio = (double)score / TotalScore;
            if (ratio > 0.3)
            {
                return 1;
            }

            if (ratio > 0.6)
            {
                return 2;
            }

            return 0;
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
            foreach (var e in Enemies)
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

        public static double Distance(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
    }
}
