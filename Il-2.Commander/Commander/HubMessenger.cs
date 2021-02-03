using Il_2.Commander.Data;
using Microsoft.AspNet.SignalR.Client;
using System.Linq;

namespace Il_2.Commander.Commander
{
    class HubMessenger
    {
        /// <summary>
        /// Хост на который будут направляться вызовы SignalR
        /// </summary>
        private string Host { get; set; }
        /// <summary>
        /// Объект мониторинга таблиц БД
        /// </summary>
        SqlWatcher sqlWatcher;
        /// <summary>
        /// Токен доступа для авторизованного вызова методов внутри хаба
        /// </summary>
        private string Token { get; set; }
        /// <summary>
        /// Стартовый метод.
        /// </summary>
        public void Start()
        {
            Host = SetApp.Config.HostSignalR;
            ExpertDB db = new ExpertDB();
            Token = db.Tokens.First(x => x.id == "Map").Token;
            sqlWatcher = new SqlWatcher();
            sqlWatcher.EventDBChange += SqlWatcher_EventDBChange;
            db.Dispose();
        }
        /// <summary>
        /// Метод остановки.
        /// </summary>
        public void Stop()
        {
            if (sqlWatcher != null)
            {
                sqlWatcher.EventDBChange -= SqlWatcher_EventDBChange;
            }
        }
        /// <summary>
        /// Специализированный старт.
        /// </summary>
        public void SpecStart()
        {
            Host = SetApp.Config.HostSignalR;
            ExpertDB db = new ExpertDB();
            Token = db.Tokens.First(x => x.id == "Map").Token;
            db.Dispose();
        }
        /// <summary>
        /// Отправка сообщения в метод хаба
        /// </summary>
        /// <param name="eventname"></param>
        public async void SpecSend(string eventname)
        {
            using (var hubConnection = new HubConnection(Host, useDefaultUrl: false))
            {
                try
                {
                    IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("hubMap");
                    await hubConnection.Start();
                    await stockTickerHubProxy.Invoke("CommanderDoor", new object[] { Token, eventname });
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// Вызывается при изменении в таблицах БД
        /// </summary>
        /// <param name="eventname"></param>
        private void SqlWatcher_EventDBChange(string eventname)
        {
            if (eventname.Equals("FrontLine"))
            {
                SendMessage(eventname);
            }
            if (eventname.Equals("Targets"))
            {
                SendMessage(eventname);
            }
        }
        /// <summary>
        /// Отправка сообщения в метод хаба
        /// </summary>
        /// <param name="eventname">Собщение в формате текста</param>
        private async void SendMessage(string eventname)
        {
            using (var hubConnection = new HubConnection(Host, useDefaultUrl: false))
            {
                try
                {
                    IHubProxy stockTickerHubProxy = hubConnection.CreateHubProxy("hubMap");
                    await hubConnection.Start();
                    await stockTickerHubProxy.Invoke("CommanderDoor", new object[] { Token, eventname });
                }
                catch
                {

                }
            }
        }
    }
}