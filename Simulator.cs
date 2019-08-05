using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static PlayerModel GetNext(PlayerModel my, string direction, List<PlayerModel> enemies, int depth)
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
                return null;
            }

            if (!myNext.Territory.Any(t => t[0] == x && t[1] == y))
            {
                // вне своей территорий
                var lines = myNext.Lines.ToList();
                lines.Add(myNext.Position);
                myNext.Lines = lines.ToArray();
            }
            else if (myNext.Lines.Length > 0)
            {
                // завершает шлейф на своей территории
                myNext.Lines = new int[0][]; 

                // todo считает шлейф, а надо шлейф + то что внутри
                //myNext.Score += my.Lines.GroupBy(l => l[0]).Count() + my.Lines.GroupBy(l => l[1]).Count();

                foreach (var l in my.Lines)
                {
                    myNext.Score += (Math.Abs(l[0] - x) + Math.Abs(l[1] - y)) / 30;
                }
            }

            foreach (var e in enemies)
            {
                if (depth >= 1 && myNext.Lines.Any(l =>
                    Math.Abs(l[0] - e.Position[0]) + Math.Abs(l[1] - e.Position[1]) <= World.Width * (depth + 2))) // depth + 5
                {
                    // страх перерезания шлейфа
                    return null;
                }

                if (e.Lines.Any(l => l[0] == x && l[1] == y))
                {
                    // перерезает шлейф противника
                    myNext.Score += 50;
                }
            }

            return myNext;
        }
    }
}
