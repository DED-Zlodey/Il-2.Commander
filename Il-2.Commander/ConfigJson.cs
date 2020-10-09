using Newtonsoft.Json;

namespace Il_2.Commander
{
    public struct ConfigJson
    {
        [JsonProperty("NumTarget")]
        public int NumTarget { get; private set; }
    }
}
