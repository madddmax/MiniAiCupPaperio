using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniAiCupPaperio
{
    public class EnemyPlayer : Player
    {
        public HashSet<Point> Territory;

        public EnemyPlayer(PlayerModel player)
            : base(player)
        {
            Territory = new HashSet<Point>(player.Territory.Select(t => new Point(t)));
        }
    }
}
