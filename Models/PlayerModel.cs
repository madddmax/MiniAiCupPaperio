using System;
using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class PlayerModel : ICloneable
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

        public object Clone()
        {
            return new PlayerModel
            {
                Score = Score,
                Position = (int[]) Position.Clone(),
                Territory = (int[][]) Territory.Clone(),
                Lines = (int[][]) Lines.Clone(),
                Direction = Direction,
                Bonuses = (PlayerBonusModel[]) Bonuses.Clone()
            };
        }
    }
}