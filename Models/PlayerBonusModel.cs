using System;
using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class PlayerBonusModel : ICloneable
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ticks")]
        public int Ticks { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}