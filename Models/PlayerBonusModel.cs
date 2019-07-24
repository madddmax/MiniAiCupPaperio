using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class PlayerBonusModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ticks")]
        public int Ticks { get; set; }
    }
}