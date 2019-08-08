namespace MiniAiCupPaperio
{
    public class MapBonus
    {
        public string Type { get; set; }

        public Point Position { get; set; }

        public MapBonus()
        {
        }

        public MapBonus(MapBonusModel mapBonus)
        {
            Type = mapBonus.Type;
            Position = new Point(mapBonus.Position);
        }
    }
}
