using Newtonsoft.Json;

namespace Il_2.Commander
{
    public struct ConfigJson
    {
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; private set; }
        [JsonProperty("HostSignalR")]
        public string HostSignalR { get; private set; }
        [JsonProperty("NumTarget")]
        public int NumTarget { get; private set; }
    }
}
