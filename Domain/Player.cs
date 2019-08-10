using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public class Player : ICloneable
    {
        public int Score;

        public int RoundedTo2Score => Score % 2 != 0 ? Score + 1 : Score;

        public Point Position;

        public HashSet<Point> Lines;

        public string Direction;

        public bool HasCapture;

        public Player()
        {
        }

        public Player(PlayerModel player)
        {
            Score = player.Score;
            Position = new Point(player.Position);
            Lines = new HashSet<Point>(player.Lines.Select(l => new Point(l)));
            Direction = player.Direction;
            HasCapture = false;
        }

        public object Clone()
        {
            return new Player
            {
                Score = Score,
                Position = new Point(Position),
                Lines = new HashSet<Point>(Lines),
                Direction = Direction,
                HasCapture = HasCapture
            };
        }
    }
}
