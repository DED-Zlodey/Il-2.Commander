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
        [JsonProperty("HostRcon")]
        public string HostRcon { get; private set; }
        [JsonProperty("PortRcon")]
        public ushort PortRcon { get; private set; }
        [JsonProperty("DirSDS")]
        public string DirSDS { get; private set; }
        [JsonProperty("LoginRcon")]
        public string LoginRcon { get; private set; }
        [JsonProperty("PassRcon")]
        public string PassRcon { get; private set; }
        [JsonProperty("DirLogs")]
        public string DirLogs { get; private set; }
        [JsonProperty("DirStatLogs")]
        public string DirStatLogs { get; private set; }
        [JsonProperty("DServer")]
        public string DServer { get; private set; }
        [JsonProperty("DServerWorkingDirectory")]
        public string DServerWorkingDirectory { get; private set; }
    }
}
