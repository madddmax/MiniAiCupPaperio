using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class PlayerModel
    {
        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("position")]
        public int[] Position { get; set; }

        [JsonProperty("territory")]
        public int[][] Territory { get; set; }

        [JsonProperty("lines")]
        public int[][] Lines { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("bonuses")]
        public PlayerBonusModel[] Bonuses { get; set; }
    }
}