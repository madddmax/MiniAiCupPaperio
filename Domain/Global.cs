using System.Collections.Generic;

namespace MiniAiCupPaperio
{
    public static class Global
    {
        public static List<MapBonus> MapBonuses = new List<MapBonus>();
        public static List<EnemyPlayer> Enemies = new List<EnemyPlayer>();
        public static HashSet<Point> MyTerritory = new HashSet<Point>();
        public static HashSet<Point> EnemyTerritory = new HashSet<Point>();
    }
}
