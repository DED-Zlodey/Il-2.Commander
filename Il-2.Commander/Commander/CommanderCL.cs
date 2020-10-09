using Il_2.Commander.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Il_2.Commander.Commander
{
    public delegate void EventLog(string Message);
    class CommanderCL
    {
        public event EventLog GetLogStr;
        private static RconCommunicator rcon;
        private static Random random = new Random();
        private HubMessenger messenger;
        private Watcher watcher;
        Process processDS;
        private Queue<RconCommand> RconCommands = new Queue<RconCommand>();
        private string LastFile { get; set; }
        private string NameMission = string.Empty;

        public void Start()
        {
            ReSetInputs();
            NameMission = GetNameNextMission(1);
            ReWriteSDS(SetApp.Config.DirSDS);
            messenger = new HubMessenger();
            messenger.SpecStart();
            StartServer();
        }
        /// <summary>
        /// Приведение базы данных в исходное, стартовое положение. Все инпуты и прочее приводятся в положение "ВЫКЛ"
        /// </summary>
        private void ReSetInputs()
        {
            ExpertDB db = new ExpertDB();
            var inputs = db.ServerInputs.ToList();
            var bridge = db.InputsBridge.ToList();
            var transport = db.ColInput.ToList();
            var targets = db.CompTarget.ToList();
            foreach (var item in inputs)
            {
                int id = item.id;
                if (item.Enable != 0)
                {
                    db.ServerInputs.First(x => x.id == id).Enable = 0;
                }
            }
            foreach (var item in bridge)
            {
                int id = item.id;
                if (item.Destroyed)
                {
                    db.InputsBridge.First(x => x.id == id).Destroyed = false;
                }
            }
            foreach (var item in transport)
            {
                int id = item.id;
                if (item.ArrivalCol != 0)
                {
                    db.ColInput.First(x => x.id == id).ArrivalCol = 0;
                }
                if (item.ArrivalUnit != 0)
                {
                    db.ColInput.First(x => x.id == id).ArrivalUnit = 0;
                }
                if (item.DestroyedUnits != 0)
                {
                    db.ColInput.First(x => x.id == id).DestroyedUnits = 0;
                }
                if (item.Active)
                {
                    db.ColInput.First(x => x.id == id).Active = false;
                }
                if (!item.Permit)
                {
                    db.ColInput.First(x => x.id == id).Permit = true;
                }
            }
            foreach (var item in targets)
            {
                int id = item.id;
                if (item.Enable)
                {
                    db.CompTarget.First(x => x.id == id).Enable = false;
                }
                if (item.Destroed != 0)
                {
                    db.CompTarget.First(x => x.id == id).Destroed = 0;
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Получение имени файла миссии.
        /// </summary>
        /// <param name="srv">Принимает номер сервера</param>
        /// <returns>Возвращает имя файла следующей миссии.</returns>
        private string GetNameNextMission(int srv)
        {
            ExpertDB db = new ExpertDB();
            var maxTour = db.PreSetupMap.Where(x => x.idServ == srv).Max(x => x.idTour);
            var actualMissId = db.PreSetupMap.Where(x => x.idServ == srv && x.idMap == 2 && x.Played == false).Min(x => x.id);
            var output = db.PreSetupMap.First(x => x.id == actualMissId).NameMiss;
            db.Dispose();
            return output;
        }
        /// <summary>
        /// Перезаписывает файл .sds
        /// </summary>
        /// <param name="pglobalSDS"></param>
        private void ReWriteSDS(string pglobalSDS)
        {
            var str = SetApp.GetFile(pglobalSDS);
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("file = "))
                {
                    str[i] = "   file = \"Dogfight\\Expert\\" + NameMission + "\"";
                }
            }
            File.WriteAllLines(pglobalSDS, str);
        }
        /// <summary>
        /// Старт DServer`a
        /// </summary>
        private void StartServer()
        {
            if (rcon != null)
            {
                rcon.DisConnectServer();
            }
            processDS = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.DServer, SetApp.Config.DirSDS);
            processStartInfo.WorkingDirectory = SetApp.Config.DServerWorkingDirectory;
            processDS.StartInfo = processStartInfo;
            processDS.Start();
            if (watcher == null)
            {
                StartWatcher();
            }
            processDS.WaitForExit(11000);
            StartRConService();
        }
        /// <summary>
        /// Старт Rcon сервиса
        /// </summary>
        private void StartRConService()
        {
            RconCommands.Clear();
            rcon = new RconCommunicator();
            rcon.ConnectServer(SetApp.Config.HostRcon, SetApp.Config.PortRcon);
            if (rcon.MyStatus() == "authed=0")
            {
                rcon.Auth(SetApp.Config.LoginRcon, SetApp.Config.PassRcon);
            }
        }
        /// <summary>
        /// Старт мониторинга папки с логами
        /// </summary>
        private void StartWatcher()
        {
            if (watcher == null)
            {
                watcher = new Watcher(SetApp.Config.DirLogs);
                watcher.LogEvents += Watcher_Events;
            }
            else
            {
                watcher.LogEvents += Watcher_Events;
            }
        }
        /// <summary>
        /// Вызывается при возникновении события LogEvents.
        /// </summary>
        /// <param name="pathLog">Принимает путь до лог-файла</param>
        private void Watcher_Events(string pathLog)
        {
            
        }
    }
}
