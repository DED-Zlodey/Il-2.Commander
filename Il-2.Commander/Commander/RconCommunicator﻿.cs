using Il_2.Commander.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Il_2.Commander.Commander
{
    class RconCommunicator﻿
    {
        /// <summary>
        /// Объект для TCP соединений
        /// </summary>
        private TcpConnec﻿tor connection;
        /// <summary>
        /// Крайний статус-код
        /// </summary>
        public StatusCod﻿e lastCommandStatus { get; private set; }
        /// <summary>
        /// Результат отправки последней ркон команды ДСерверу
        /// </summary>
        public string las﻿﻿tCommandResult { get; private set; }
        /// <summary>
        /// Список пилотов онлайн полученный ркон запросом на ДСервер
        /// </summary>
        public List<Player> Players { get; private set; }
        /// <summary>
        /// Конструктор по умолчания. Инициализирует переменную с пилотами онлайн
        /// </summary>
        public RconCommunicator()
        {
            Players = new List<Player>();
        }
        /// <summary>
        /// Разрывает соединение с ДСервром.
        /// </summary>
        public void DisConnectServer()
        {
            connection.Dispose();
        }
        /// <summary>
        /// Устанавливает TCP связь с ДСервером
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void ConnectServer(string host, ushort port)
        {
            connection = new TcpConnector(host, port);
        }
        /// <summary>
        /// Основной метод выполнения ркон команд
        /// </summary>
        /// <param name="cmd">Команда в формате строки</param>
        private void Execute(string cmd)
        {
            string response = connection.ExecuteCommand(cmd);
            string[] arr = { response, "" };
            if (response.Contains('&'))
            {
                arr = response.Split('&');
            }
            int lcom = 0;
            int.TryParse(arr[0].Substring(7), out lcom);
            lastCommandStatus = (StatusCode)lcom;
            lastCommandResult = Uri.UnescapeDataString(arr[1]);

        }
        /// <summary>
        /// Завершает текущую миссии (если таковая есть) и запускает новую с параметрами из фала .sds
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public string OpenSDS(string cmd)
        {
            string command = "opensds " + cmd;
            return connection.ExecuteCommand(command);
        }
        /// <summary>
        /// Сброс SPS.
        /// </summary>
        /// <returns></returns>
        public string ResetSPS()
        {
            return connection.ExecuteCommand("spsreset");
        }
        /// <summary>
        /// Возвращает статус авторизации. 1 если авторизован и 0 если нет.
        /// </summary>
        /// <returns></returns>
        public string My﻿Status()
        {
            //cmd: mystatus (no parameters)
            //response example: STATUS=1&authed=1
            string command = "mystatus";
            Execute(command);
            return lastCommandResult;
        }
        /// <summary>
        /// Выключение сервера.
        /// </summary>
        public void Shutdown()
        {
            string command = "shutdown";
            connection.ExecuteCommand(command);
            DisConnectServer();
        }
        /// <summary>
        /// Авторизация. Для отправки большинства команд необходима авторизация.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Auth(string user, string password)
        {
            //cmd: auth e-mail password
            //response exampl﻿e: STATUS=1
            string command = "auth " + user + " " + password;
            //Execute(command);
            return connection.ExecuteCommand(command);
        }
        /// <summary>
        /// Получает список пилотов онлайн
        /// </summary>
        /// <returns></returns>
        public List<Player> GetPlayerList()
        {
            string command = "getplayerlist";
            Execute(command);
            //SaveCommand(lastCommandResult);
            string[] p = lastCommandResult.Split('|').Skip(1).ToArray();
            Players = new List<Player>(p.Count());
            foreach (var s in p)
            {
                string[] playerInfo = s.Split(',');
                Players.Add(new Player(int.Parse(playerInfo[0]), GetGameStatus(playerInfo[1]), int.Parse(playerInfo[2]), playerInfo[3], playerInfo[4], playerInfo[5]));
            }
            return Players;
        }
        /// <summary>
        /// Возвращает игровой стату пилота
        /// </summary>
        /// <param name="status"></param>
        /// <returns>Возвращает игровой стату пилота</returns>
        private string GetGameStatus(string status)
        {
            if (int.Parse(status) == 0)
            {
                return GameStatusPilot.Spectator.ToString();
            }
            else
            {
                return GameStatusPilot.Spectator.ToString();
            }
        }
        /// <summary>
        /// Выкидывает игрока с сервера
        /// </summary>
        /// <param name="PlayerId"></param>
        public void Kick (string PlayerId)
        {
            string commonstr = "kick " + "playerid " + PlayerId;
            //command += commonstr;
            //command += value;
            Execute(commonstr);
        }
        /// <summary>
        /// Отправляет кмоанду ServerInput для инициирования какой-либо игровой логики.
        /// </summary>
        /// <param name="translatorName"></param>
        /// <returns></returns>
        public string ServerInput(string translatorName)
        {
            //cmd: serverin﻿put translator_name
            //response example: STATUS=1
            string command = "serverinput " + translatorName;
            Execute(co﻿mmand);
            return lastCommandResult;
        }
        /// <summary>
        /// Отправка сообщений на сервер.
        /// </summary>
        /// <param name="roomType"></param>
        /// <param name="message"></param>
        /// <param name="recipientId"></param>
        /// <returns></returns>
        public string ChatMsg(RoomType roomType, string message, int recipientId)
        {
            //cmd: chatmsg roomtype id Message to send
            //response example: STATUS=1
            //chatmsg 0 -1 msg      (all)
            //chatmsg 1 cid msg     (coalition message, 0 - neutral, 1 - allies, 2 - axis)
            //chatmsg 2 cid msg     (country message, 0 - all)
            //chatmsg 3 cid msg     (private message, cid - client id from playerlist)
            string com﻿mand;
            switch (roomType)
            {
                case RoomType.All:
                    co﻿﻿mmand = string.Format("chatmsg 0 -1 {0}", message);
                    break;
                case RoomTy﻿pe.Coalition:
                    command = string.Format("chatmsg 1 {0} {1}", recipientId, message);
                    break;
                case RoomType.Country:
                    command = string.Format("chatmsg 2 {0} {1}", recipientId, message);
                    break;
                case RoomType.ClientId:
                    command = string.Format("chatmsg 3 {0} {1}", recipientId, message);
                    break;
                default: return string.Empty;
            }
            Execute(command);
            return lastCommandResult;
        }
    }
    /// <summary>
    /// Статус-коды состояния сервера
    /// </summary>
    public enum StatusCode
    {
        RCR_OK = 1,
        RCR_ERR_UNKNOWN = 2,
        RCR_ERR_UNKNOWN_COMMAND = 3,
        RCR_ERR_PARAM_COUNT = 4,
        RCR_ERR_RECVBUFFER = 5,
        RCR_ERR_AUTH_INCORRECT = 6,
        RCR_ERR_SERVER_NOT_RUNNING = 7,
        RCR_ERR_SERVER_USER = 8,
        RCR_ERR_UNKNOWN_USER = 9,
    }
    /// <summary>
    /// Комнаты для отправки сообщений
    /// </summary>
    public enum RoomType
    {
        All = 0,
        Client = 1,
        Coalition = 2,
        Country = 3,
        ClientId = 4
    }
}
