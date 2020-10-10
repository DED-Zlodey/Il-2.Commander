using Il_2.Commander.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Il_2.Commander.Parser;
using Microsoft.EntityFrameworkCore;

namespace Il_2.Commander.Commander
{
    public delegate void EventLog(string Message);
    class CommanderCL
    {
        public event EventLog GetLogStr;
        private static RconCommunicator rcon;
        private static Random random = new Random();
        private HubMessenger messenger;
        private Process processGenerator;
        private Watcher watcher;
        private Process processDS;
        private Queue<RconCommand> RconCommands = new Queue<RconCommand>();
        private List<ColInput> ActiveColumn = new List<ColInput>();
        private Queue<DStrikeRed> redQ = new Queue<DStrikeRed>();
        private Queue<DStrikeBlue> blueQ = new Queue<DStrikeBlue>();
        /// <summary>
        /// Тут хранятся цели для атаки красных (сами цели синие) красные атакуют, синие оброняют
        /// </summary>
        private List<Target> redTarget = new List<Target>();
        /// <summary>
        /// Тут хранятся цели для атаки синих (сами цели красные) синие атакуют, красные оброняют
        /// </summary>
        private List<Target> blueTarget = new List<Target>();
        public static Queue<List<string>> qLog = new Queue<List<string>>();
        private List<AType12> Bridges = new List<AType12>();
        private string LastFile { get; set; }
        private string NameMission = string.Empty;
        private bool TriggerTime = true;
        private DateTime dt = DateTime.Now;
        private DateTime messDurTime = DateTime.Now;
        private int durmess = 30;
        private int DurationMission = 235; // Длительность миссии в минутах

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
            if(pathLog != LastFile)
            {
                LastFile = pathLog;
                if (pathLog.Contains("[0].txt"))
                {
                    Form1.busy = true;
                    TriggerTime = true;
                    messDurTime = DateTime.Now;
                    ClearPrevMission();
                    dt = DateTime.Now;
                    GetLogStr("Mission start: " + dt.ToShortDateString() + " " + dt.ToLongTimeString());
                    SetDurationMission(1);
                    SavedMissionTimeStart();
                    EnableFields();
                }
                ReadLogFile(pathLog);
            }
        }

        private void ReadLogFile(string pathLog)
        {
            var str = SetApp.GetFile(pathLog);
            if (str.Count > 1)
            {
                if (TriggerTime)
                {
                    qLog.Enqueue(str);
                }
                else
                {
                    SetVictoryLog(str, pathLog);
                }
            }
            var currentdt = DateTime.Now;
            var curmissend = currentdt - messDurTime;
            var ts = currentdt - dt;
            if (curmissend.TotalMinutes >= durmess)
            {
                messDurTime = DateTime.Now;
                var ostatok = Math.Round(DurationMission - ts.TotalMinutes, 0);
                var mess = "-=COMMANDER=- END of the mission: " + ostatok + " min.";
                RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                RconCommands.Enqueue(sendall);
                RconCommands.Enqueue(sendred);
                RconCommands.Enqueue(sendblue);
                GetLogStr(mess);
            }
            if (ts.TotalMinutes >= DurationMission)
            {
                if (TriggerTime)
                {
                    TriggerTime = false;
                    var mess = "-=COMMANDER=- MISSION END.";
                    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                    GetLogStr(mess);
                    RconCommands.Enqueue(sendall);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    ChangeCoalitionPoint();
                    SetEndMission(1);
                    StartGeneration(); // Старт генератора
                }
            }
            FileInfo fi = new FileInfo(pathLog);
            File.Move(pathLog, SetApp.Config.DirStatLogs + fi.Name);
        }
        /// <summary>
        /// Изменяет колалицию точек в БД
        /// </summary>
        private void ChangeCoalitionPoint()
        {
            // TODO:
        }
        /// <summary>
        /// Устанавливает длительность миссии исходя из погодных условий. Данные получает из БД.
        /// </summary>
        /// <param name="idServ">Принимает номер сервера</param>
        private void SetDurationMission(int idServ)
        {
            ExpertDB db = new ExpertDB();
            var maxTour = db.PreSetupMap.Where(x => x.idServ == idServ).Max(x => x.idTour);
            var actualMissId = db.PreSetupMap.Where(x => x.idServ == idServ && x.idMap == 2 && x.Played == false).Min(x => x.id);
            var isMU = db.PreSetupMap.First(x => x.id == actualMissId).PrecType;
            if(isMU > 0)
            {
                DurationMission = db.DurationMission.First(x => x.id == 1).DurmissRain;
            }
            else
            {
                DurationMission = db.DurationMission.First(x => x.id == 1).DurmissNotRain;
            }
            db.Dispose();
        }
        /// <summary>
        /// Сохранаяет в БД время старта миссии
        /// </summary>
        private void SavedMissionTimeStart()
        {
            ExpertDB db = new ExpertDB();
            var alltimer = db.MTimer.ToList();
            foreach(var item in alltimer)
            {
                db.MTimer.Remove(item);
            }
            db.MTimer.Add(new MTimer
            {
                Duration = DurationMission,
                idServer = 1,
                StartTime = DateTime.UtcNow
            });
            db.SaveChanges();
            db.Dispose();
            messenger.SpecSend("Timer");
        }
        /// <summary>
        /// Включение ПВО аэродромов
        /// </summary>
        private void EnableFields()
        {
            ExpertDB db = new ExpertDB();
            var fields = db.ServerInputs.Where(x => x.Name.Contains("AField-AAA-ON-"));
            foreach (var item in fields)
            {
                RconCommand onfield = new RconCommand(Rcontype.Input, item.Name);
                RconCommands.Enqueue(onfield);
                GetLogStr(item.Name);
            }
            db.Dispose();
        }
        public void StartGeneration()
        {
            GetLogStr("Start Generation...");
            processGenerator = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.Generator);
            processStartInfo.RedirectStandardOutput = true; //Выводить в родительское окно
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; // не создавать окно CMD
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            processGenerator.StartInfo = processStartInfo;
            processGenerator.EnableRaisingEvents = true;
            processGenerator.Exited += new EventHandler(Generation_complete);
            processGenerator.Start();
        }
        /// <summary>
        /// Вызывается после завершения генерации миссии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generation_complete(object sender, EventArgs e)
        {
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            foreach (var item in content)
            {
                GetLogStr(item);
            }
            GetLogStr("Restart Mission...");
            GetNameNextMission(1);
            ReWriteSDS(SetApp.Config.DirSDS);
            NextMission(SetApp.Config.DirSDS);
        }
        /// <summary>
        /// Запуск следующей миссии через ркон команду
        /// </summary>
        /// <param name="path">Принимает путь до файла *.sds</param>
        private void NextMission(string path)
        {
            FileInfo fi = new FileInfo(path);
            var nameFSDS = fi.Name;
            rcon.OpenSDS(nameFSDS);
            if (watcher == null)
            {
                StartWatcher();
            }
        }
        /// <summary>
        /// Подмена информации SecondaryTask на PrimaryTask в лог файле. Для отправки победы в стату Ваала
        /// </summary>
        /// <param name="str">Получает лог-файл списком строк</param>
        /// <param name="path">Получает путь к лог-файлу для его перезаписи</param>
        private void SetVictoryLog(List<string> str, string path)
        {
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("AType:8 "))
                {
                    var aType = new AType8(str[i]);
                    if (aType.ICTYPE == -1 && aType.TYPE == 1)
                    {
                        str[i] = str[i].Replace("TYPE:1", "TYPE:0");
                        File.WriteAllLines(path, str);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Очищает все данные оставшиеся от предыдущей миссии
        /// </summary>
        private void ClearPrevMission()
        {
            if (redQ.Count > 0)
            {
                redQ.Clear();
            }
            if (blueQ.Count > 0)
            {
                blueQ.Clear();
            }
            if (redTarget.Count > 0)
            {
                redTarget.Clear();
            }
            if (blueTarget.Count > 0)
            {
                blueTarget.Clear();
            }
            if (qLog.Count > 0)
            {
                qLog.Clear();
            }
            if (ActiveColumn.Count > 0)
            {
                ActiveColumn.Clear();
            }
            if (Bridges.Count > 0)
            {
                Bridges.Clear();
            }
            //if (atype10.Count > 0)
            //{
            //    atype10.Clear();
            //}
            if (RconCommands.Count > 0)
            {
                RconCommands.Clear();
            }
            //if (LRecon.Count > 0)
            //{
            //    LRecon.Clear();
            //}
            //if (capturedsRed.Count > 0)
            //{
            //    capturedsRed.Clear();
            //}
            //if (capturedsBlue.Count > 0)
            //{
            //    capturedsBlue.Clear();
            //}
            //if (curentPlayers.Count > 0)
            //{
            //    curentPlayers.Clear();
            //}
            //if (endmessage.Count > 0)
            //{
            //    endmessage.Clear();
            //}
        }
        /// <summary>
        /// Фиксирует завершение миссии в базе данных. Если миссий впереди не осталось, вызывает ReSetCompany
        /// </summary>
        /// <param name="idServ">Принимает номер сервера</param>
        private void SetEndMission(int idServ)
        {
            ExpertDB db = new ExpertDB();
            var maxTour = db.PreSetupMap.Where(x => x.idServ == idServ).Max(x => x.idTour);
            var actualMissId = db.PreSetupMap.Where(x => x.idServ == idServ && x.idMap == 2 && x.Played == false).Min(x => x.id);
            db.PreSetupMap.First(x => x.id == actualMissId).Played = true;
            db.SaveChanges();
            var countmissnext = db.PreSetupMap.Where(x => x.idServ == idServ && x.idMap == 2 && x.Played == false).ToList().Count;
            if (countmissnext == 0)
            {
                ReSetCompany reSet = new ReSetCompany();
                reSet.Start();
            }
            db.Dispose();
        }
    }
}
