using Microsoft.Data.SqlClient;

namespace Il_2.Commander.Commander
{
    public delegate void DlegateChangeDB(string eventname);
    class SqlWatcher
    {
        public event DlegateChangeDB EventDBChange;
        private string ConnectionString { get; set; }
        public SqlWatcher()
        {
            ConnectionString = GetConnString();
            StartWatching();
        }
        private string GetConnString()
        {
            if (string.IsNullOrEmpty(SetApp.Config.ConnectionString))
            {
                SetApp.SetUp();
            }
            return SetApp.Config.ConnectionString;
        }
        private void StartWatching()
        {
            SqlDependency.Stop(ConnectionString);
            SqlDependency.Start(ConnectionString);
            ExecuteWatchingQueryInputs();
            ExecuteWatchingQueryFronline();
        }
        private void ExecuteWatchingQueryInputs()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("select Enable from dbo.ServerInputs", connection))
                {
                    var sqlDependency = new SqlDependency(command);
                    sqlDependency.OnChange += new OnChangeEventHandler(OnDatabaseChange_Targets);
                    command.ExecuteReader();
                }
            }
        }
        private void ExecuteWatchingQueryFronline()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("select IndexFiled from dbo.RearFields", connection))
                {
                    var sqlDependency = new SqlDependency(command);
                    sqlDependency.OnChange += SqlDependency_OnChangeFrontLine;
                    command.ExecuteReader();
                }
            }
        }
        private void ExecuteWatchingQueryTimer()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("select StartTime from dbo.MTimer", connection))
                {
                    var sqlDependency = new SqlDependency(command);
                    sqlDependency.OnChange += SqlDependency_OnChangeTimer;
                    command.ExecuteReader();
                }
            }
        }

        private void SqlDependency_OnChangeTimer(object sender, SqlNotificationEventArgs e)
        {
            if (SqlNotificationInfo.Insert.Equals(e.Info) || SqlNotificationInfo.Update.Equals(e.Info))
            {
                EventDBChange("Timer");
            }
            ExecuteWatchingQueryTimer();
        }

        private void SqlDependency_OnChangeFrontLine(object sender, SqlNotificationEventArgs e)
        {
            if (SqlNotificationInfo.Insert.Equals(e.Info) || SqlNotificationInfo.Update.Equals(e.Info))
            {
                EventDBChange("FrontLine");
            }
            ExecuteWatchingQueryFronline();
        }

        private void OnDatabaseChange_Targets(object sender, SqlNotificationEventArgs args)
        {
            if (SqlNotificationInfo.Update.Equals(args.Info) || SqlNotificationInfo.Truncate.Equals(args.Info))
            {
                EventDBChange("Targets");
            }
            ExecuteWatchingQueryInputs();
        }
    }
}
