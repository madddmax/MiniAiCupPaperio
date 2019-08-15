﻿using System;
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
        public static Dictionary<Point, int> EnemyFearDic = new Dictionary<Point, int>();

        public static int BorderPushingScore = 5;

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
                    myNext.Score -= BorderPushingScore;
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
                    myNext.Score -= BorderPushingScore;
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
                    myNext.Score -= BorderPushingScore;
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
                    myNext.Score -= BorderPushingScore;
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


            foreach (var l in myNext.Lines)
            {
                // проблема, т.к. глубина тут не причем на существующем шлейфе
                if (EnemyFearDic[l] <= depth + 1)
                {
                    // страх пересечения шлейфа
                    //return null;
                    myNext.Score -= 500;
                }
            }

            var onMyTerritory = MyTerritory.Contains(myNext.Position);
            if (!onMyTerritory)
            {
                if (EnemyFearDic[myNext.Position] <= depth + 1)
                {
                    // страх столкновения с головой
                    //return null;
                    myNext.Score -= 500;
                }
            }

            foreach (var e in Enemies)
            {
                if (e.Lines.Contains(myNext.Position))
                {
                    // пересекает шлейф противника
                    myNext.Score += 50;
                }

                if (onMyTerritory && e.Position.Equals(myNext.Position))
                {
                    // бьет противника по голове
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

                    foreach (var bonus in Bonuses)
                    {
                        if (bonus.Position.Equals(p))
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
                }

                myNext.Lines = new HashSet<Point>();
                myNext.HasCapture = true;
            }

            return myNext;
        }

        public static double Distance(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
    }
}
