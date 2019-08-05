using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static List<PlayerModel> Enemies = new List<PlayerModel>();

        public static PlayerModel GetNext(PlayerModel my, string direction, int depth)
        {
            var myNext = (PlayerModel) my.Clone();

            int x = myNext.Position[0];
            int y = myNext.Position[1];

            if (direction == Direction.Left)
            {
                x -= World.Width;
                if (x < World.MinX)
                {
                    return null;
                }
            }
            else if(direction == Direction.Right)
            {
                x += World.Width;
                if (x > World.MaxX)
                {
                    return null;
                }
            }
            else if (direction == Direction.Up)
            {
                y += World.Width;
                if (y > World.MaxY)
                {
                    return null;
                }
            }
            else if (direction == Direction.Down)
            {
                y -= World.Width;
                if (y < World.MinY)
                {
                    return null;
                }
            }

            myNext.Direction = direction;
            myNext.Position[0] = x;
            myNext.Position[1] = y;

            if (myNext.Lines.Any(l => l[0] == x && l[1] == y))
            {
                // пересекает свой шлейф
                return null;
            }

            var onMyTerritory = myNext.Territory.Any(t => t[0] == x && t[1] == y);
            if (!onMyTerritory)
            {
                // вне своей территорий - увеличиваем шлейф
                var lines = myNext.Lines.ToList();
                lines.Add(myNext.Position);
                myNext.Lines = lines.ToArray();
            }

            var allEnemyTerritory = new List<int[]>();
            foreach (var e in Enemies)
            {
                if (depth >= 1 && 
                    myNext.Lines.Any(l => Math.Abs(l[0] - e.Position[0]) + Math.Abs(l[1] - e.Position[1]) <= World.Width * (depth + 2)))
                {
                    // страх пересечения шлейфа
                    return null;
                }

                if (e.Lines.Any(l => l[0] == x && l[1] == y))
                {
                    // пересекает шлейф противника
                    myNext.Score += 50;
                }

                allEnemyTerritory.AddRange(e.Territory);
            }

            if (onMyTerritory && myNext.Lines.Length > 0)
            {
                // завершает шлейф на своей территории
                foreach (var l in myNext.Lines)
                {
                    int captureScore = 1;
                    if (allEnemyTerritory.Any(t => t[0] == l[0] && t[1] == l[1]))
                    {
                        captureScore = 5;
                    }
                    myNext.Score += ((Math.Abs(l[0] - x) + Math.Abs(l[1] - y)) / World.Width) * captureScore;
                }

                myNext.Lines = new int[0][];
            }

            return myNext;
        }
    }
}
