using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public class Player : ICloneable
    {
        public int Score { get; set; }

        public Point Position { get; set; }

        public HashSet<Point> Lines { get; set; }

        public string Direction { get; set; }

        public bool HasCapture { get; set; }

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
