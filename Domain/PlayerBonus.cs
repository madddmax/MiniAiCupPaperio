namespace MiniAiCupPaperio
{
    public class PlayerBonus
    {
        public string Type { get; set; }

        public int Ticks { get; set; }

        public PlayerBonus(PlayerBonusModel playerBonus)
        {
            Type = playerBonus.Type;
            Ticks = playerBonus.Ticks;
        }
    }
}
