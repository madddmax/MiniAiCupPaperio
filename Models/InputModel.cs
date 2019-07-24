using Newtonsoft.Json;

namespace MiniAiCupPaperio
{
    public class InputModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("params")]
        public InputModelParams Params { get; set; }
    }
}