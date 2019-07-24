using System.Collections.Generic;
using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class InputModelParams
    {
        [JsonProperty("x_cells_count")]
        public int XCount { get; set; }

        [JsonProperty("y_cells_count")]
        public int YCount { get; set; }

        [JsonProperty("speed")]
        public int Speed { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("tick_num")]
        public int TickNum { get; set; }

        [JsonProperty("bonuses")]
        public MapBonusModel[] Bonuses { get; set; }

        [JsonProperty("players")]
        public Dictionary<string, PlayerModel> Players { get; set; }
    }
}