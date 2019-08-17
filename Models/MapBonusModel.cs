using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class MapBonusModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("position")]
        public int[] Position { get; set; }

        [JsonProperty("active_ticks")]
        public int ActiveTicks { get; set; }
    }
}