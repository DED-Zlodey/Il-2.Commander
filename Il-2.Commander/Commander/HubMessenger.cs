using Il_2.Commander.Data;
using Microsoft.AspNet.SignalR.Client;
using System.Linq;

namespace Il_2.Commander.Commander
{
    class HubMessenger
    {
        private string Host { get; set; }
        SqlWatcher sqlWatcher;
        private string Token { get; set; }
        public void Start()
        {
            Host = SetApp.Config.HostSignalR;
            ExpertDB db = new ExpertDB();
            Token = db.Tokens.First(x => x.id == "Map").Token;
            sqlWatcher = new SqlWatcher();
            sqlWatcher.EventDBChange += SqlWatcher_EventDBChange;
            db.Dispose();
        }
        public void Stop()
        {
            if (sqlWatcher != null)
            {
                sqlWatcher.EventDBChange -= SqlWatcher_EventDBChange;
            }
        }
        public void SpecStart()
        {
            Host = SetApp.Config.HostSignalR;
            ExpertDB db = new ExpertDB();
            Token = db.Tokens.First(x => x.id == "Map").Token;
            db.Dispose();
        }
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