using Newtonsoft.Json;

namespace Il_2.Commander
{
    public struct ConfigJson
    {
        /// <summary>
        /// Строка подключения к БД
        /// </summary>
        [JsonProperty("ConnectionString")]
        public string ConnectionString { get; private set; }
        /// <summary>
        /// Адрес хаба SignalR
        /// </summary>
        [JsonProperty("HostSignalR")]
        public string HostSignalR { get; private set; }
        /// <summary>
        /// Кол-во целей
        /// </summary>
        [JsonProperty("NumTarget")]
        public int NumTarget { get; private set; }
        /// <summary>
        /// Хост сервера для подключения к RCon
        /// </summary>
        [JsonProperty("HostRcon")]
        public string HostRcon { get; private set; }
        /// <summary>
        /// Порт сервера для подключения к RCon
        /// </summary>
        [JsonProperty("PortRcon")]
        public ushort PortRcon { get; private set; }
        /// <summary>
        /// Путь до файла *.sds
        /// </summary>
        [JsonProperty("DirSDS")]
        public string DirSDS { get; private set; }
        /// <summary>
        /// Логин для подключения к РКон
        /// </summary>
        [JsonProperty("LoginRcon")]
        public string LoginRcon { get; private set; }
        /// <summary>
        /// Пароль для подключения к РКон
        /// </summary>
        [JsonProperty("PassRcon")]
        public string PassRcon { get; private set; }
        /// <summary>
        /// Папка куда DServer записывает лог-файлы
        /// </summary>
        [JsonProperty("DirLogs")]
        public string DirLogs { get; private set; }
        /// <summary>
        /// Папка, куда следует перемещать лог-файлы после обработки коммандером (например, для дальнейшей обработки статой Ваала)
        /// </summary>
        [JsonProperty("DirStatLogs")]
        public string DirStatLogs { get; private set; }
        /// <summary>
        /// Путь до исполняемого файла DServer`a
        /// </summary>
        [JsonProperty("DServer")]
        public string DServer { get; private set; }
        /// <summary>
        /// Путь до рабочей папки DServer`a
        /// </summary>
        [JsonProperty("DServerWorkingDirectory")]
        public string DServerWorkingDirectory { get; private set; }
        /// <summary>
        /// Путь до исполняемого файла генератора миссий
        /// </summary>
        [JsonProperty("Generator")]
        public string Generator { get; private set; }
        /// <summary>
        /// Путь до рабочей папки генератора миссий
        /// </summary>
        [JsonProperty("GeneratorWorkingDirectory")]
        public string GeneratorWorkingDirectory { get; private set; }
        /// <summary>
        /// Максимальная вместимость одного полевого склада
        /// </summary>
        [JsonProperty("BattlePoints")]
        public int BattlePoints { get; private set; }
        /// <summary>
        /// Мультипликатор привозимого груза для одной ед. техники транспортной колонны
        /// </summary>
        [JsonProperty("TransportMult")]
        public double TransportMult { get; private set; }
        /// <summary>
        /// Директория в которой лежит SRS server
        /// </summary>
        [JsonProperty("DirSRS")]
        public string DirSRS { get; private set; }
    }
}
