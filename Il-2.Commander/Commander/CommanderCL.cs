using Il_2.Commander.Data;
using Il_2.Commander.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Il_2.Commander.Commander
{
    public delegate void EventLog(string Message, Color color);
    public delegate void EventLogArray(string[] array);
    class CommanderCL
    {
        public event EventLog GetLogStr;
        public event EventLog GetOfficerTime;
        public event EventLogArray GetLogArray;
        private static RconCommunicator rcon;
        private static Random random = new Random();
        private HubMessenger messenger;
        private Process processGenerator;
        private Watcher watcher;
        private Process processDS;
        /// <summary>
        /// Очередь Ркон комманд
        /// </summary>
        private Queue<RconCommand> RconCommands = new Queue<RconCommand>();
        private List<ColInput> ActiveColumn = new List<ColInput>();
        /// <summary>
        /// Направление атаки красных. Точки синие.
        /// </summary>
        private Queue<DStrikeRed> redQ = new Queue<DStrikeRed>();
        /// <summary>
        /// Направление атаки синих. Точки красные.
        /// </summary>
        private Queue<DStrikeBlue> blueQ = new Queue<DStrikeBlue>();
        /// <summary>
        /// Текущая точка атаки красных
        /// </summary>
        private DStrikeRed currentRedPoint;
        /// <summary>
        /// Текущая точка атаки синих
        /// </summary>
        private DStrikeBlue currentBluePoint;
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
        bool qrcon = true;
        private List<AType10> pilotsList = new List<AType10>();

        public CommanderCL()
        {
            messenger = new HubMessenger();
            messenger.SpecStart();
        }

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
        /// Отправка Ркон комманд из очереди
        /// </summary>
        public void SendRconCommand()
        {
            if (RconCommands.Count > 0 && qrcon)
            {
                if (rcon != null && RconCommands.Count > 0)
                {
                    qrcon = false;
                    var result = RconCommands.Dequeue();
                    if (result != null && result.Type == Rcontype.ChatMsg)
                    {
                        rcon.ChatMsg(result.TypeRoom, result.Command, result.RecipientId);
                    }
                    if (result != null && result.Type == Rcontype.Input)
                    {
                        rcon.ServerInput(result.Command);
                        GetLogStr("Server Input: " + result.Command, Color.Black);
                    }
                    if (result != null && result.Type == Rcontype.Players)
                    {
                        var players = rcon.GetPlayerList();
                        var player = players.FirstOrDefault(x => x.PlayerId == result.aType.LOGIN);
                        if (player != null)
                        {
                            var mess = "-=COMMANDER=- " + player.Name + " your ping: " + player.Ping;
                            RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                            RconCommands.Enqueue(wrap);
                            if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                            {
                                pilotsList.First(x => x.LOGIN == player.PlayerId).Player = player;
                            }
                        }
                    }
                    qrcon = true;
                }
            }
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
            if (pathLog != LastFile)
            {
                LastFile = pathLog;
                if (pathLog.Contains("[0].txt"))
                {
                    Form1.busy = true;
                    TriggerTime = true;
                    messDurTime = DateTime.Now;
                    ClearPrevMission();
                    dt = DateTime.Now;
                    GetLogStr("Mission start: " + dt.ToShortDateString() + " " + dt.ToLongTimeString(), Color.Black);
                    SetDurationMission(1);
                    SavedMissionTimeStart();
                    EnableFields();
                    InitDirectPoints();
                    SetAttackPoint();
                    EnableTargetsToCoalition(201);
                    EnableTargetsToCoalition(101);
                    EnableWareHouse();
                }
                ReadLogFile(pathLog);
            }
        }
        public void HandleLogFile(List<string> str)
        {
            for (int i = 0; i < str.Count; i++)
            {

            }
        }
        /// <summary>
        /// Чтение лог-файла, постановка его в очередь на обработку
        /// </summary>
        /// <param name="pathLog">Принимает путь до лог-файла</param>
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
                GetLogStr(mess, Color.Black);
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
                    GetLogStr(mess, Color.Red);
                    RconCommands.Enqueue(sendall);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    ChangeCoalitionPoint();
                    SetEndMission(1);
                    StartGeneration("pregen"); // Старт генератора
                }
            }
            FileInfo fi = new FileInfo(pathLog);
            File.Move(pathLog, SetApp.Config.DirStatLogs + fi.Name);
        }
        private void InitDirectPoints()
        {
            ExpertDB db = new ExpertDB();
            var blue = db.DStrikeBlue.ToList();
            blue.Sort();
            foreach (var item in blue)
            {
                if (item.SerialNumber != 1)
                    blueQ.Enqueue(item);
            }
            var red = db.DStrikeRed.ToList();
            red.Sort();
            foreach (var item in red)
            {
                if (item.SerialNumber != 1)
                    redQ.Enqueue(item);
            }
            db.Dispose();
        }
        /// <summary>
        /// Устанавливает текущие точки атаки для красных и синих одновременно.
        /// </summary>
        private void SetAttackPoint()
        {
            if (blueQ.Count > 0)
            {
                currentBluePoint = blueQ.Dequeue();
            }
            if (redQ.Count > 0)
            {
                currentRedPoint = redQ.Dequeue();
            }
        }
        /// <summary>
        /// Устанавливает текущую точку атаки в заивисимости от коалиции
        /// </summary>
        /// <param name="coal">Принимает номер коалиции для которой требуется установить текущую точку атаки</param>
        private void SetAttackPoint(int coal)
        {
            if (coal == 201)
            {
                currentBluePoint = blueQ.Dequeue();
            }
            if (coal == 101)
            {
                currentRedPoint = redQ.Dequeue();
            }
        }
        /// <summary>
        /// Включение целей вокруг актуальной точки атаки
        /// </summary>
        /// <param name="coal">Коалиция</param>
        private void EnableTargetsToCoalition(int coal)
        {
            ExpertDB db = new ExpertDB();
            if (coal == 201 && currentBluePoint != null)
            {
                var targets = db.ServerInputs.Where(x => x.Coalition != coal && x.IndexPoint == currentBluePoint.IndexPoint && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
                foreach (var item in targets)
                {
                    RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                    RconCommands.Enqueue(command);
                    int id = item.id;
                    db.ServerInputs.First(x => x.id == id).Enable = 1;
                }
            }
            if (coal == 101 && currentRedPoint != null)
            {
                var targets = db.ServerInputs.Where(x => x.Coalition != coal && x.IndexPoint == currentRedPoint.IndexPoint && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
                foreach (var item in targets)
                {
                    RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                    RconCommands.Enqueue(command);
                    int id = item.id;
                    db.ServerInputs.First(x => x.id == id).Enable = 1;
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        private void EnableWareHouse()
        {
            ExpertDB db = new ExpertDB();
            var sinputWH = db.ServerInputs.Where(x => x.GroupInput == 8 && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
            foreach(var item in sinputWH)
            {
                RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                RconCommands.Enqueue(command);
                int id = item.id;
                db.ServerInputs.First(x => x.id == id).Enable = 1;
            }
            db.SaveChanges();
            db.Dispose();
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
            if (isMU > 0)
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
            foreach (var item in alltimer)
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
                //GetLogStr(item.Name, Color.Black);
            }
            db.Dispose();
        }
        /// <summary>
        /// Старт генератора. Запускает полную генерацию миссии
        /// </summary>
        public void StartGeneration()
        {
            GetLogStr("Start Generation...", Color.Black);
            processGenerator = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.Generator);
            processStartInfo.WorkingDirectory = SetApp.Config.GeneratorWorkingDirectory;
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
        /// Запускает предварительную генерацию миссии.
        /// </summary>
        /// <param name="predGen">pregen</param>
        public void StartGeneration(string predGen)
        {
            GetLogStr("Start PredGen...", Color.Red);
            processGenerator = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(SetApp.Config.Generator, predGen);
            processStartInfo.WorkingDirectory = SetApp.Config.GeneratorWorkingDirectory;
            processStartInfo.RedirectStandardOutput = true; //Выводить в родительское окно
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true; // не создавать окно CMD
            processStartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
            processGenerator.StartInfo = processStartInfo;
            processGenerator.EnableRaisingEvents = true;
            processGenerator.Exited += new EventHandler(PredGen_complete);
            processGenerator.Start();
        }
        /// <summary>
        /// Вызывается после завершения предварительной генерации миссии.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PredGen_complete(object sender, EventArgs e)
        {
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            GetLogArray(content);
            GetLogStr("Wait start generation...", Color.Black);
            GetOfficerTime("StartTimeOfficer", Color.Black);
        }
        /// <summary>
        /// Вызывается после завершения генерации миссии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generation_complete(object sender, EventArgs e)
        {
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            GetLogArray(content);
            GetLogStr("Restart Mission...", Color.Black);
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
            if (rcon != null)
            {
                rcon.OpenSDS(nameFSDS);
            }
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
        /// <summary>
        /// Проверка регистрации пилота
        /// </summary>
        /// <param name="player">Объект пилота</param>
        private void CheckRegistration(Player player)
        {
            ExpertDB db = new ExpertDB();
            if (!db.ProfileUser.ToList().Exists(x => x.GameId == player.PlayerId))
            {
                if (!db.LinkedAccount.ToList().Exists(x => x.GameID == player.PlayerId))
                {
                    var code = GenerationCode(db);
                    db.LinkedAccount.Add(new LinkedAccount
                    {
                        CheckCode = code,
                        GameID = player.PlayerId,
                        PilotName = player.Name
                    });
                    db.SaveChanges();
                    var mess = "-=COMMANDER=- " + player.Name + " your registration code: " + code;
                    RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                    RconCommands.Enqueue(wrap);
                }
                else
                {
                    var code = db.LinkedAccount.First(x => x.GameID == player.PlayerId).CheckCode;
                    var mess = "-=COMMANDER=- " + player.Name + " your registration code: " + code;
                    RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                    RconCommands.Enqueue(wrap);
                }
            }
            db.Dispose();
        }
        /// <summary>
        /// Генерирует строку случайных символов
        /// </summary>
        /// <param name="db">Объект базы данных</param>
        /// <returns>Возвращает строку случайных симолов</returns>
        private string GenerationCode(ExpertDB db)
        {
            var code = GenerateName(6);
            if (!db.LinkedAccount.ToList().Exists(x => x.CheckCode == code))
            {
                return code;
            }
            else
            {
                return GenerationCode(db);
            }
        }
        /// <summary>
        /// Генерирует строку случайных символов
        /// </summary>
        /// <param name="rnd">Объект рандом</param>
        /// <param name="passlength">Длинна строки</param>
        /// <returns>Возвращает случайную строку</returns>
        private string GenerateName(int passlength)
        {
            string iPass = "";
            string[] arr = { "_", "%", "@", "!", "#", "$", "^", "&", "*", "(", ")", "{", "}",
                "-", "+", "=", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "B", "C", "D", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W",
                "X", "Z", "b", "c", "d", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z", "A", "E", "U", "Y", "a", "e", "i", "o", "u", "y" };
            for (int i = 0; i < passlength; i++)
            {
                iPass = iPass + arr[random.Next(0, arr.Length)];
            }
            return iPass;
        }
        /// <summary>
        /// Останавливает сервер и закрывает приложение DServer
        /// </summary>
        public void Stop()
        {
            if (watcher != null)
            {
                watcher.LogEvents -= Watcher_Events;
            }
            ClearPrevMission();
            if(rcon != null)
            {
                rcon.Shutdown();
            }
        }
    }
}
