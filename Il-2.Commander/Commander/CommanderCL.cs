using Il_2.Commander.Data;
using Il_2.Commander.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <summary>
        /// Список активных колонн
        /// </summary>
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
        /// Очередь из лог-файлов
        /// </summary>
        public static Queue<List<string>> qLog = new Queue<List<string>>();
        /// <summary>
        /// Список инпутов мостов
        /// </summary>
        private List<InputsBridge> Bridges = new List<InputsBridge>();
        /// <summary>
        /// Список объектов колонн
        /// </summary>
        private List<AType12> ColumnAType12 = new List<AType12>();
        /// <summary>
        /// Список статиков засветившихся в логах
        /// </summary>
        private List<AType12> Blocks = new List<AType12>();
        /// <summary>
        /// Список активированных инпутов разведки
        /// </summary>
        private List<ServerInputs> LRecon = new List<ServerInputs>();
        /// <summary>
        /// Активные цели.
        /// </summary>
        private List<CompTarget> ActiveTargets = new List<CompTarget>();
        /// <summary>
        /// Отложенная отправка сообщений в чат. Имитируется задержка связи. О нападении сообщается не сразу.
        /// </summary>
        private List<DeferredCommand> deferredCommands = new List<DeferredCommand>();
        /// <summary>
        /// Состояние складов
        /// </summary>
        private List<BattlePonts> battlePonts = new List<BattlePonts>();
        /// <summary>
        /// Список занятых населенных пунктов в миссии
        /// </summary>
        private List<Victory> victories = new List<Victory>();
        /// <summary>
        /// Список воздушного снабжения
        /// </summary>
        private List<HandlingAirSupply> airSupplies = new List<HandlingAirSupply>();
        private List<AType12> AllLogs = new List<AType12>();
        /// <summary>
        /// крайний лог-файл
        /// </summary>
        private string LastFile { get; set; }
        /// <summary>
        /// Имя текущей миссии.
        /// </summary>
        private string NameMission = string.Empty;
        private bool TriggerTime = true;
        private DateTime dt = DateTime.Now;
        private DateTime messDurTime = DateTime.Now;
        private int durmess = 30;
        private int DurationMission = 235; // Длительность миссии в минутах
        bool qrcon = true;
        private List<AType10> pilotsList = new List<AType10>();
        private List<Player> onlinePlayers = new List<Player>();

        #region Регулярки
        private static Regex reg_brackets = new Regex(@"(?<={).*?(?=})");
        #endregion

        public CommanderCL()
        {
            messenger = new HubMessenger();
            messenger.SpecStart();
        }

        /// <summary>
        /// Основной метод программы. Отсюда ничанается старт всех действий.
        /// </summary>
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
                            var currentdt = DateTime.Now;
                            var ts = currentdt - dt;
                            var ostatok = Math.Round(DurationMission - ts.TotalMinutes, 0);
                            if (ostatok <= 0)
                            {
                                ostatok = 0;
                            }
                            var mess = "-=COMMANDER=- " + player.Name + " there are " + ostatok + " minutes left until the end of the mission";
                            RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                            RconCommands.Enqueue(wrap);
                            if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                            {
                                pilotsList.First(x => x.LOGIN == player.PlayerId).Player = player;
                            }
                        }
                    }
                    if (result != null && result.Type == Rcontype.ReSetSPS)
                    {
                        Color myRgbColor = new Color();
                        myRgbColor = Color.FromArgb(0, 166, 23);
                        rcon.ResetSPS();
                        GetLogStr("Reset SPS: " + DateTime.Now.ToLongTimeString(), myRgbColor);
                    }
                    qrcon = true;
                }
            }
            if (deferredCommands.Count > 0 && qrcon)
            {
                qrcon = false;
                var localdt = DateTime.Now;
                List<DeferredCommand> deletes = new List<DeferredCommand>();
                foreach (var item in deferredCommands)
                {
                    var elapsed = localdt - item.CreateTime;
                    if (elapsed.TotalMinutes >= item.Duration)
                    {
                        RconCommand sendmess = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, item.Command, item.Coalition);
                        RconCommands.Enqueue(sendmess);
                        deletes.Add(item);
                    }
                }
                if (deletes.Count > 0)
                {
                    foreach (var item in deletes)
                    {
                        deferredCommands.Remove(item);
                    }
                }
                qrcon = true;
            }
            if (airSupplies.Count > 0 && qrcon)
            {
                qrcon = false;
                var localdt = DateTime.Now;
                List<HandlingAirSupply> deletes = new List<HandlingAirSupply>();
                ExpertDB db = new ExpertDB();
                foreach (var item in airSupplies)
                {
                    var elapsed = localdt - item.CreateTime;
                    if (elapsed.TotalSeconds >= item.Duration)
                    {
                        if (item.TypeSupply == TypeSupply.ForCity)
                        {
                            if (pilotsList.Exists(x => x.LOGIN == item.Pilot.LOGIN))
                            {
                                var ent = db.GraphCity.FirstOrDefault(x => x.IndexCity == item.IndexCity);
                                if (ent != null)
                                {
                                    db.GraphCity.First(x => x.IndexCity == item.IndexCity).PointsKotel = ent.PointsKotel + item.Pilot.Cargo;
                                    var online = rcon.GetPlayerList();
                                    var pilot = online.FirstOrDefault(x => x.PlayerId == item.Pilot.LOGIN);
                                    if (pilot != null)
                                    {
                                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, item.Command, pilot.Cid);
                                        RconCommands.Enqueue(sendall);
                                    }
                                    GetLogStr(item.Command, Color.DarkGreen);
                                    pilotsList.First(x => x.LOGIN == item.Pilot.LOGIN).Cargo = 0;
                                    deletes.Add(item);
                                    db.SaveChanges();
                                }
                            }
                        }
                        if (item.TypeSupply == TypeSupply.ForPilot)
                        {
                            if (pilotsList.Exists(x => x.LOGIN == item.Pilot.LOGIN))
                            {
                                var online = rcon.GetPlayerList();
                                var pilot = online.FirstOrDefault(x => x.PlayerId == item.Pilot.LOGIN);
                                if (pilot != null)
                                {
                                    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, item.Command, pilot.Cid);
                                    RconCommands.Enqueue(sendall);
                                }
                                pilotsList.First(x => x.LOGIN == item.Pilot.LOGIN).Cargo = 0.35;
                                GetLogStr(item.Command, Color.DarkGreen);
                                deletes.Add(item);
                            }
                        }
                    }
                }
                foreach (var item in deletes)
                {
                    airSupplies.Remove(item);
                }
                db.Dispose();
                qrcon = true;
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
                    SetStateWH();
                    ClearPrevMission();
                    dt = DateTime.Now;
                    GetLogStr("Mission start: " + dt.ToShortDateString() + " " + dt.ToLongTimeString(), Color.Black);
                    SetDurationMission(1);
                    SavedMissionTimeStart();
                    InitDirectPoints();
                    SetAttackPoint();
                    EnableTargetsToCoalition(201);
                    EnableTargetsToCoalition(101);
                    EnableWareHouse();
                }
                ReadLogFile(pathLog);
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
                    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                    GetLogStr(mess, Color.Red);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    CreateVictoryCoalition();
                    StartGeneration("pregen");
                }
            }
            FileInfo fi = new FileInfo(pathLog);
            File.Move(pathLog, SetApp.Config.DirStatLogs + fi.Name);
            UpdateCurrentPlayers();
            if (TriggerTime)
            {
                StartColumn(101);
                StartColumn(201);
            }
        }
        /// <summary>
        /// Обработка логфайла из очереди.
        /// </summary>
        /// <param name="str">Лог файл в видет списка строк</param>
        public void HandleLogFile(List<string> str)
        {
            bool updateTarget = false;
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("AType:10 "))
                {
                    AType10 aType = new AType10(str[i]);
                    RconCommand wrap = new RconCommand(Rcontype.Players, aType);
                    RconCommands.Enqueue(wrap);
                    if (aType.TYPE.Contains("Ju 52 3mg4e"))
                    {
                        aType.Cargo = 0.35;
                    }
                    pilotsList.Add(aType);
                    if (onlinePlayers.Exists(x => x.PlayerId == aType.LOGIN))
                    {
                        onlinePlayers.First(x => x.PlayerId == aType.LOGIN).IngameStatus = GameStatusPilot.Parking.ToString();
                        onlinePlayers.First(x => x.PlayerId == aType.LOGIN).Coalition = aType.COUNTRY;
                    }
                }
                if (str[i].Contains("AType:16 "))
                {
                    var aType = new AType16(str[i]);
                    if (pilotsList.Exists(x => x.PID == aType.BOTID))
                    {
                        var playerid = pilotsList.First(x => x.PID == aType.BOTID).LOGIN;
                        if (onlinePlayers.Exists(x => x.PlayerId == playerid))
                        {
                            onlinePlayers.First(x => x.PlayerId == playerid).IngameStatus = GameStatusPilot.Spectator.ToString();
                            onlinePlayers.First(x => x.PlayerId == playerid).Coalition = 0;
                        }
                        var ent = pilotsList.FirstOrDefault(x => x.PID == aType.BOTID);
                        if (ent != null)
                        {
                            pilotsList.Remove(pilotsList.First(x => x.PLID == ent.PLID));
                        }
                    }
                }
                if (str[i].Contains("AType:3 "))
                {
                    AType3 aType = new AType3(str[i]);
                    if (!pilotsList.Exists(x => x.PID == aType.TID) && !pilotsList.Exists(x => x.PLID == aType.TID))
                    {
                        CheckDestroyTarget(aType);
                        updateTarget = true;
                    }
                }
                if (str[i].Contains("AType:5 "))
                {
                    var aType = new AType5(str[i]);
                    if (pilotsList.Exists(x => x.PLID == aType.PID))
                    {
                        pilotsList.First(x => x.PLID == aType.PID).GameStatus = GameStatusPilot.InFlight.ToString();
                        var playerid = pilotsList.First(x => x.PLID == aType.PID).LOGIN;
                        if (onlinePlayers.Exists(x => x.PlayerId == playerid))
                        {
                            onlinePlayers.First(x => x.PlayerId == playerid).IngameStatus = GameStatusPilot.InFlight.ToString();
                        }
                    }
                    if (airSupplies.Exists(x => x.Pilot.PLID == aType.PID))
                    {
                        var ent = airSupplies.First(x => x.Pilot.PLID == aType.PID);
                        airSupplies.Remove(ent);
                    }
                }
                if (str[i].Contains("AType:6 "))
                {
                    var aType = new AType6(str[i]);
                    if (pilotsList.Exists(x => x.PLID == aType.PID))
                    {
                        pilotsList.First(x => x.PLID == aType.PID).GameStatus = GameStatusPilot.Parking.ToString();
                        var playerid = pilotsList.First(x => x.PLID == aType.PID).LOGIN;
                        if (onlinePlayers.Exists(x => x.PlayerId == playerid))
                        {
                            onlinePlayers.First(x => x.PlayerId == playerid).IngameStatus = GameStatusPilot.InFlight.ToString();
                        }
                        var ent = pilotsList.First(x => x.PLID == aType.PID);
                        if (ent.TYPE.Contains("Ju 52 3mg4e"))
                        {
                            SupplyCauldron(aType, ent);
                        }
                    }
                }
                if (str[i].Contains("AType:8 "))
                {
                    var aType = new AType8(str[i]);
                    if (aType.ICTYPE == -1 && aType.TYPE == 15)
                    {
                        DisableColumn(aType);
                    }
                    if (aType.ICTYPE == -1 && aType.TYPE == 13)
                    {
                        DisableColumnBridge(aType);
                    }
                    var mess = "Objective: " + aType.OBJID.ToString() + " TYPE:" + aType.TYPE + " ICTYPE:" + aType.ICTYPE + " Quad: " + GetQuadForMap(aType);
                    GetLogStr(mess, Color.DarkGreen);
                }
                if (str[i].Contains("AType:12 "))
                {
                    AType12 aType = new AType12(str[i]);
                    AllLogs.Add(aType);
                    if (ActiveColumn.Exists(x => x.NameCol.Equals(aType.NAME)))
                    {
                        aType.Unit = ActiveColumn.Find(x => x.NameCol.Equals(aType.NAME)).Unit;
                        ColumnAType12.Add(aType);
                    }
                    if (aType.TYPE.Contains("bridge"))
                    {
                        var mess = "Spawn Bridge: " + aType.TYPE + " " + aType.COUNTRY;
                        GetLogStr(mess, Color.DarkGreen);
                    }
                    if (aType.NAME.Equals("BlocksArray"))
                    {
                        Blocks.Add(aType);
                    }
                }
            }
            //RconCommand resetSDS = new RconCommand(Rcontype.ReSetSPS);
            //RconCommands.Enqueue(resetSDS);
            if (updateTarget)
            {
                if (messenger != null)
                {
                    messenger.SpecSend("Targets");
                }
            }
            if (TriggerTime || RconCommands.Count > 0)
            {
                Form1.busy = true;
            }
        }
        /// <summary>
        /// Инициализация состояния складов
        /// </summary>
        private void SetStateWH()
        {
            battlePonts.Clear();
            ExpertDB db = new ExpertDB();
            var bpl = db.BattlePonts.ToList();
            battlePonts.AddRange(bpl);
            db.Dispose();
        }
        /// <summary>
        /// Старт колонн
        /// </summary>
        /// <param name="coal">номер коалиции</param>
        private void StartColumn(int coal)
        {
            ExpertDB db = new ExpertDB();
            var bp = battlePonts.Where(x => x.Coalition == coal).OrderBy(x => x.Point).ToList();
            var allcolumn = db.ColInput.Where(x => x.Coalition == coal && x.Permit).ToList();
            var ActivCol = allcolumn.Where(x => x.Coalition == coal && x.Active).ToList();
            foreach (var item in ActivCol)
            {
                var entBP = bp.FirstOrDefault(x => x.WHID == item.NWH && x.Coalition == coal);
                if (entBP != null)
                {
                    bp.Remove(entBP);
                }
            }
            if (ActivCol.Count < 3 && allcolumn.Count > 0)
            {
                int iter = 3 - ActivCol.Count;
                for (int i = 0; i < iter; i++)
                {

                    var allwhcol = allcolumn.Where(x => x.NWH == bp[i].WHID && x.Coalition == bp[i].Coalition).ToList();
                    if (allwhcol.Count > 0)
                    {
                        int rindex = random.Next(0, allwhcol.Count);
                        var inputmess = allwhcol[rindex].NameCol;
                        var ent = allwhcol[rindex];
                        ent.Active = true;
                        ActiveColumn.Add(ent);
                        RconCommand command = new RconCommand(Rcontype.Input, ent.NameCol);
                        RconCommands.Enqueue(command);
                        db.ColInput.First(x => x.NameCol == inputmess).Active = true;
                    }
                }
                db.SaveChanges();
            }
            db.Dispose();
        }
        /// <summary>
        /// Обработка всех киллов в списке
        /// </summary>
        /// <param name="atype3">Список объектов AType:3</param>
        private void CheckDestroyTarget(List<AType3> atype3)
        {
            bool reviewMapTarget = false;
            foreach (var item in atype3)
            {
                if (ColumnAType12.Exists(x => x.ID == item.TID))
                {
                    KillUnitColumn(item);
                }
                else
                {
                    bool isTarget = false;
                    bool isBridge = false;
                    for (int i = 0; i < ActiveTargets.Count; i++)
                    {
                        if (ActiveTargets[i].GroupInput != 8)
                        {
                            var Xres = item.XPos - ActiveTargets[i].XPos;
                            var Zres = item.ZPos - ActiveTargets[i].ZPos;
                            double min = -0.1;
                            double max = 0.1;
                            if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                            {
                                isTarget = true;
                                KillTargetObj(ActiveTargets[i], item);
                            }
                        }
                    }
                    if (isTarget)
                    {
                        reviewMapTarget = true;
                        continue;
                    }
                    for (int i = 0; i < Bridges.Count; i++)
                    {
                        var Xres = Bridges[i].XPos - item.XPos;
                        var Zres = Bridges[i].ZPos - item.ZPos;
                        double min = -0.1;
                        double max = 0.1;
                        if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                        {
                            isBridge = true;
                            KillBridge(Bridges[i]);
                            break;
                        }
                    }
                    if (isBridge)
                    {
                        continue;
                    }
                    if (HandlingForWH(item))
                    {
                        reviewMapTarget = true;
                        continue;
                    }
                    CheckDisableTarget(item);
                }
            }
            if (reviewMapTarget)
            {
                if (messenger != null)
                {
                    messenger.SpecSend("Targets");
                }
            }
        }
        private void CheckDestroyTarget(AType3 aType)
        {
            if (ColumnAType12.Exists(x => x.ID == aType.TID))
            {
                KillUnitColumn(aType);
            }
            else
            {
                bool isBridge = false;
                if (!KillTargetObj(aType))
                {
                    for (int i = 0; i < Bridges.Count; i++)
                    {
                        var Xres = Bridges[i].XPos - aType.XPos;
                        var Zres = Bridges[i].ZPos - aType.ZPos;
                        double min = -0.1;
                        double max = 0.1;
                        if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                        {
                            isBridge = true;
                            KillBridge(Bridges[i]);
                            break;
                        }
                    }
                    if (!isBridge)
                    {
                        HandlingForWH(aType);
                        CheckDisableTarget(aType);
                    }
                }
            }
        }
        /// <summary>
        /// Проверка атаки неактивной цели
        /// </summary>
        /// <param name="aType">Событие kill</param>
        private void CheckDisableTarget(AType3 aType)
        {
            ExpertDB db = new ExpertDB();
            var targets = db.CompTarget.Where(x => x.Enable == false && x.GroupInput != 8).ToList();
            foreach (var item in targets)
            {
                var Xres = aType.XPos - item.XPos;
                var Zres = aType.ZPos - item.ZPos;
                double min = -0.1;
                double max = 0.1;
                if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                {
                    var ent = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-"));
                    if (ent.Enable == 0)
                    {
                        var pilot = pilotsList.FirstOrDefault(x => x.PLID == aType.AID || x.PID == aType.AID);
                        if (pilot != null)
                        {
                            var mess = "-=COMMANDER=-: Attention " + pilot.NAME + "! You are attacking a target that is forbidden to attack!";
                            GetLogStr(mess, Color.DarkOrange);
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                            RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                            RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                            RconCommands.Enqueue(sendall);
                            RconCommands.Enqueue(sendred);
                            RconCommands.Enqueue(sendblue);
                        }
                    }
                }
            }
            db.Dispose();
        }
        /// <summary>
        /// Уничтожение объекта внутри цели. А так же проверка уничтожена ли цель целиком. Если уничтожена цель выключается.
        /// </summary>
        /// <param name="aType">Событие kill</param>
        /// <returns></returns>
        private bool KillTargetObj(AType3 aType)
        {
            var evMess = "AType:3 " + GetQuadForMap(aType.ZPos, aType.XPos);
            GetLogStr(evMess, Color.DarkGoldenrod);
            bool output = false;
            ExpertDB db = new ExpertDB();
            var targets = db.CompTarget.Where(x => x.Enable && x.InernalWeight > x.Destroed).ToList();
            foreach (var item in targets)
            {
                var Xres = aType.XPos - item.XPos;
                var Zres = aType.ZPos - item.ZPos;
                double min = -0.1;
                double max = 0.1;
                if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                {
                    var ent = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-OFF-") && !x.Name.Contains("Icon-"));
                    var entON = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-"));
                    if (entON.Enable == 1)
                    {
                        int destroy = item.Destroed + 1;
                        targets.First(x => x.id == item.id).Destroed = destroy;
                        db.CompTarget.First(x => x.id == item.id).Destroed = destroy;
                        var DestroyedMess = "-=COMMNDER=- " + item.Name + " " + item.Model + " " + entON.Coalition + " destroyed";
                        GetLogStr(DestroyedMess, Color.DarkGoldenrod);
                        var countMandatory = targets.Where(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Mandatory).ToList().Count - 2;
                        if (countMandatory < 0)
                        {
                            countMandatory = 0;
                        }
                        var countDestroyedMandatory = targets.Where(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Mandatory).Sum(x => x.Destroed);
                        var countDestroyed = targets.Where(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex).Sum(x => x.Destroed);
                        if (countMandatory <= countDestroyedMandatory)
                        {
                            if (item.TotalWeigth <= countDestroyed)
                            {
                                RconCommand command = new RconCommand(Rcontype.Input, ent.Name);
                                RconCommands.Enqueue(command);
                                var color = Color.Black;
                                var coalstr = string.Empty;
                                if (ent.Coalition == 201)
                                {
                                    color = Color.DarkRed;
                                    coalstr = "Allies destroyed ";
                                }
                                if (ent.Coalition == 101)
                                {
                                    color = Color.DarkBlue;
                                    coalstr = "Axis destroyed ";
                                }
                                var mess = "-=COMMANDER=-: " + coalstr + GetNameTarget(ent.GroupInput) + " Regiment: " + ent.IndexPoint + " Batalion: " + ent.SubIndex;
                                GetLogStr(mess, color);
                                db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-")).Enable = -1;
                                RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                                RconCommands.Enqueue(sendall);
                                RconCommands.Enqueue(sendred);
                                RconCommands.Enqueue(sendblue);
                            }
                        }
                        db.SaveChanges();
                        var alltargets = db.ServerInputs.Where(x => x.IndexPoint == ent.IndexPoint && x.Enable == 1).ToList();
                        db.Dispose();
                        if (alltargets.Count == 0)
                        {
                            int invcoal = InvertedCoalition(ent.Coalition);
                            ChangeCoalitionPoint(ent.IndexPoint);
                            SetAttackPoint(invcoal);
                            EnableTargetsToCoalition(invcoal);
                        }
                        output = true;
                        break;
                    }
                }
            }
            if (db != null)
            {
                db.Dispose();
            }
            return output;
        }
        /// <summary>
        /// Уничтожение объекта внутри цели. А так же проверка уничтожена ли цель целиком. Если уничтожена цель выключается.
        /// </summary>
        /// <param name="target">Объект цели</param>
        private void KillTargetObj(CompTarget target, AType3 aType)
        {
            ExpertDB db = new ExpertDB();
            var ent = db.ServerInputs.First(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex && x.Name.Contains("-OFF-") && !x.Name.Contains("Icon-"));
            var entON = db.ServerInputs.First(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-"));
            if (entON.Enable == 1)
            {
                var activeTarget = db.CompTarget.FirstOrDefault(x => x.ZPos == target.ZPos && x.XPos == target.XPos && x.InernalWeight > x.Destroed);
                if (activeTarget != null)
                {
                    if (activeTarget.InernalWeight > activeTarget.Destroed)
                    {
                        ActiveTargets.First(x => x.id == activeTarget.id && x.ZPos == activeTarget.ZPos && x.XPos == activeTarget.XPos).Destroed = activeTarget.Destroed + 1;
                        db.CompTarget.First(x => x.id == activeTarget.id && x.ZPos == activeTarget.ZPos && x.XPos == activeTarget.XPos).Destroed = activeTarget.Destroed + 1;
                    }
                    var countMandatory = ActiveTargets.Where(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex && x.Mandatory).ToList().Count;
                    var countDestroyedMandatory = ActiveTargets.Where(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex && x.Mandatory).Sum(x => x.Destroed);
                    var countDestroyed = ActiveTargets.Where(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex).Sum(x => x.Destroed);
                    if (countMandatory <= countDestroyedMandatory)
                    {
                        if (target.TotalWeigth <= countDestroyed)
                        {
                            RconCommand command = new RconCommand(Rcontype.Input, ent.Name);
                            RconCommands.Enqueue(command);
                            var color = Color.Black;
                            var coalstr = string.Empty;
                            if (ent.Coalition == 201)
                            {
                                color = Color.DarkBlue;
                                coalstr = "Allies destroyed ";
                            }
                            if (ent.Coalition == 101)
                            {
                                color = Color.DarkRed;
                                coalstr = "Axis destroyed ";
                            }
                            var mess = "-=COMMANDER=-: " + coalstr + GetNameTarget(ent.GroupInput) + " Regiment: " + ent.IndexPoint + " Batalion: " + ent.SubIndex;
                            GetLogStr(mess, color);
                            db.ServerInputs.First(x => x.IndexPoint == target.IndexPoint && x.SubIndex == target.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-")).Enable = -1;
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                            RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                            RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                            RconCommands.Enqueue(sendall);
                            RconCommands.Enqueue(sendred);
                            RconCommands.Enqueue(sendblue);
                        }
                    }
                    db.SaveChanges();
                    var alltargets = db.ServerInputs.Where(x => x.IndexPoint == ent.IndexPoint && x.Enable == 1).ToList();
                    db.Dispose();
                    if (alltargets.Count == 0)
                    {
                        int invcoal = InvertedCoalition(ent.Coalition);
                        ChangeCoalitionPoint(ent.IndexPoint);
                        SetAttackPoint(invcoal);
                        EnableTargetsToCoalition(invcoal);
                    }
                }
            }
            else
            {
                db.Dispose();
            }
        }
        /// <summary>
        /// Уничтожение моста. Остановка колонны перед мостом, а так же включение запрета на отправку данной колонны
        /// </summary>
        /// <param name="aType">Объект AType:3</param>
        private void KillBridge(InputsBridge bridge)
        {
            ExpertDB db = new ExpertDB();
            var allbridges = db.InputsBridge.Where(x => x.XPos == bridge.XPos && x.ZPos == bridge.ZPos && !x.Destroyed).ToList();
            foreach (var item in allbridges)
            {
                RconCommand command = new RconCommand(Rcontype.Input, item.NameBridge);
                RconCommands.Enqueue(command);
                var mess = "-=COMMANDER=-: The bridge in square [" + GetQuadForMap(bridge.ZPos, bridge.XPos) + "] is destroyed.";
                RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                RconCommands.Enqueue(sendall);
                RconCommands.Enqueue(sendred);
                RconCommands.Enqueue(sendblue);
                GetLogStr(mess, Color.DarkMagenta);
                string namecol = bridge.NameColumn;
                var ent = db.ColInput.First(x => x.NameCol == namecol);
                if (ActiveColumn.Exists(x => x.NameCol == namecol))
                {
                    var allArrivalCol = ent.ArrivalCol + 1;
                    var allArrivalUnits = ent.Unit + ent.ArrivalUnit - ent.DestroyedUnits;
                    db.ColInput.First(x => x.NameCol == namecol).ArrivalUnit = allArrivalUnits;
                    db.ColInput.First(x => x.NameCol == namecol).ArrivalCol = allArrivalCol;
                    db.ColInput.First(x => x.NameCol == namecol).Active = false;
                }
                db.ColInput.First(x => x.NameCol == namecol).Permit = false;
                db.InputsBridge.First(x => x.XPos == bridge.XPos && x.ZPos == bridge.ZPos && !x.Destroyed).Destroyed = true;
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Обработка уничтожения транспортной единицы
        /// </summary>
        /// <param name="aType">Объект AType:3</param>
        private void KillUnitColumn(AType3 aType)
        {
            var ent = ColumnAType12.FindLast(x => x.ID == aType.TID && !x.Destroyed);
            if (ent != null)
            {
                var column = ActiveColumn.First(x => x.NameCol == ent.NAME);
                column.DestroyedUnits = column.DestroyedUnits + 1;
                ActiveColumn.First(x => x.NameCol == ent.NAME).DestroyedUnits = column.DestroyedUnits;
                ColumnAType12.FindLast(x => x.ID == aType.TID && !x.Destroyed).Destroyed = true;
                var column12 = ColumnAType12.Where(x => x.NAME.Equals(column.NameCol)).ToList();
                var column12Dead = ColumnAType12.Where(x => x.NAME.Equals(column.NameCol) && x.Destroyed).ToList();
                if (ent.Unit <= column12Dead.Count)
                {
                    if (pilotsList.Exists(x => x.PLID == aType.AID || x.PID == aType.AID))
                    {
                        var pilot = pilotsList.First(x => x.PLID == aType.AID || x.PID == aType.AID);
                        var mess = "Pilot: " + pilot.NAME + " Coalition: " + pilot.COUNTRY + " Destroyed: " + ent.TYPE;
                        GetLogStr(mess, Color.DarkViolet);
                    }
                }
                if (pilotsList.Exists(x => x.PLID == aType.AID || x.PID == aType.AID))
                {
                    var pilot = pilotsList.First(x => x.PLID == aType.AID || x.PID == aType.AID);
                    var mess = "Pilot: " + pilot.NAME + " Coalition: " + pilot.COUNTRY + " Destroyed: " + ent.TYPE;
                    GetLogStr(mess, Color.DarkGreen);
                }
                if (column.DestroyedUnits >= column.Unit)
                {
                    string coal = string.Empty;
                    if (column.Coalition == 201)
                    {
                        coal = "Axis";
                    }
                    if (column.Coalition == 101)
                    {
                        coal = "Allies";
                    }
                    var mess = "-=COMMANDER=-: Column for warehouse: " + column.NWH + " Coalition: " + coal + " Destroyed units: " + column.DestroyedUnits;
                    GetLogStr(mess, Color.DarkRed);
                    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                    RconCommands.Enqueue(sendall);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    DisableColumn(column);
                    //ActiveColumn.Remove(column);
                }
            }
        }
        /// <summary>
        /// Записывает в БД сколько уничтожено транспорта в колонне и маркирует колонну как выключенную.
        /// </summary>
        /// <param name="column">Объект колонна</param>
        private void DisableColumn(ColInput column)
        {
            ExpertDB db = new ExpertDB();
            db.ColInput.First(x => x.id == column.id).Active = false;
            db.ColInput.First(x => x.id == column.id).DestroyedUnits = column.DestroyedUnits;
            db.SaveChanges();
            db.Dispose();
            RestoreWareHouseInMemory(column.NWH, column.Coalition);
            ActiveColumn.Remove(column);
        }
        private void DisableColumnBridge(AType8 aType)
        {
            ExpertDB db = new ExpertDB();
            var mobj = db.MissionObj.Where(x => x.TaskType == 13).ToList();
            foreach (var item in mobj)
            {
                var Xres = aType.XPos - item.XPos;
                var Zres = aType.ZPos - item.ZPos;
                double min = -0.1;
                double max = 0.1;
                if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                {
                    if (ActiveColumn.Exists(x => x.NameCol == item.NameObjective))
                    {
                        var ent = ActiveColumn.First(x => x.NameCol == item.NameObjective);
                        var column12 = ColumnAType12.Where(x => x.NAME.Equals(ent.NameCol)).ToList();
                        var column12Dead = ColumnAType12.Where(x => x.NAME.Equals(ent.NameCol) && x.Destroyed).ToList();
                        var altmess = "-=COMMANDER=-:  Сargo convoy for warehouse: " + ent.NWH + " Coalition: " + ent.Coalition + " was stopped due to the destruction of the bridge. Destroyed unit: " + column12Dead.Count;
                        GetLogStr(altmess, Color.DarkViolet);
                        var allArrivalCol = ent.ArrivalCol + 1;
                        var allArrivalUnits = (int)(ent.Unit / 2) - column12Dead.Count;
                        string namecol = ent.NameCol;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalUnit = ent.ArrivalUnit + allArrivalUnits;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalCol = allArrivalCol;
                        db.ColInput.First(x => x.NameCol == namecol).Active = false;
                        db.ColInput.First(x => x.NameCol == namecol).DestroyedUnits = ent.DestroyedUnits + column12Dead.Count + (int)(ent.Unit / 2);
                        //var mess = "-=COMMANDER=-:  Сargo convoy for warehouse: " + ent.NWH + " Coalition: " + ent.Coalition + " arrived at its destination ";
                        //GetLogStr(mess, Color.DarkRed);
                        ActiveColumn.Remove(ent);
                        RestoreWareHouseInMemory(ent.NWH, ent.Coalition, db);
                    }
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Выключает колонну
        /// </summary>
        /// <param name="aType">Объект AType:8</param>
        private void DisableColumn(AType8 aType)
        {
            ExpertDB db = new ExpertDB();
            var mobj = db.MissionObj.Where(x => x.TaskType == 15).ToList();
            foreach (var item in mobj)
            {
                var Xres = aType.XPos - item.XPos;
                var Zres = aType.ZPos - item.ZPos;
                double min = -0.1;
                double max = 0.1;
                if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                {
                    if (ActiveColumn.Exists(x => x.NameCol == item.NameObjective))
                    {
                        var ent = ActiveColumn.First(x => x.NameCol == item.NameObjective);
                        var column12 = ColumnAType12.Where(x => x.NAME.Equals(ent.NameCol)).ToList();
                        var column12Dead = ColumnAType12.Where(x => x.NAME.Equals(ent.NameCol) && x.Destroyed).ToList();
                        var altmess = "-=COMMANDER=-:  Сargo convoy for warehouse: " + ent.NWH + " Coalition: " + ent.Coalition + " arrived at its destination. Destroyed unit: " + column12Dead.Count;
                        GetLogStr(altmess, Color.DarkViolet);
                        var allArrivalCol = ent.ArrivalCol + 1;
                        var allArrivalUnits = ent.Unit - column12Dead.Count;
                        string namecol = ent.NameCol;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalUnit = ent.ArrivalUnit + allArrivalUnits;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalCol = allArrivalCol;
                        db.ColInput.First(x => x.NameCol == namecol).Active = false;
                        db.ColInput.First(x => x.NameCol == namecol).DestroyedUnits = ent.DestroyedUnits + column12Dead.Count;
                        ActiveColumn.Remove(ent);
                        RestoreWareHouseInMemory(ent.NWH, ent.Coalition, db);
                    }
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Восстанавливает склады в памяти, чтобы точнее управлять колоннами.
        /// </summary>
        /// <param name="numtarget">Номер склада</param>
        /// <param name="coal">Номер коалиции</param>
        /// <param name="db">БД</param>
        private void RestoreWareHouseInMemory(int numtarget, int coal, ExpertDB db)
        {
            var damage = db.DamageLog.Where(x => x.WHID == numtarget && x.Coalition == coal).ToList();
            var bp = db.BattlePonts.First(x => x.Coalition == coal && x.WHID == numtarget);
            var columns = db.ColInput.Where(x => x.IndexPoint == numtarget && x.ArrivalUnit > 0 && x.Coalition == coal).ToList();
            var arrivalBP = 0.00;
            if (columns.Count > 0)
            {
                foreach (var item in columns)
                {
                    double koef = 1;
                    if (item.TypeCol == (int)TypeColumn.Armour)
                    {
                        koef = 1.5;
                    }
                    if (item.TypeCol == (int)TypeColumn.Mixed)
                    {
                        koef = 1.2;
                    }
                    if (item.TypeCol == (int)TypeColumn.Transport)
                    {
                        koef = 1;
                    }
                    arrivalBP += item.ArrivalUnit * koef;
                }
                if (arrivalBP < damage.Count)
                {
                    //var counter = (int)arrivalBP;
                    //for (int i = 0; i < counter; i++)
                    //{
                    //    int rindex = random.Next(0, damage.Count);
                    //    var ent = damage[rindex];
                    //}
                }
                else
                {
                    foreach (var item in damage)
                    {
                        arrivalBP = arrivalBP - 1;
                    }
                    var finalBP = bp.Point + (int)arrivalBP;
                    if (finalBP > SetApp.Config.BattlePoints)
                    {
                        finalBP = SetApp.Config.BattlePoints;
                    }
                    battlePonts.First(x => x.Coalition == coal && x.WHID == numtarget).Point = finalBP;
                }
            }
        }
        /// <summary>
        /// Восстанавливает склады в памяти, чтобы точнее управлять колоннами.
        /// </summary>
        /// <param name="numtarget">Номер склада</param>
        /// <param name="coal">Номер коалиции</param>
        private void RestoreWareHouseInMemory(int numtarget, int coal)
        {
            ExpertDB db = new ExpertDB();
            var damage = db.DamageLog.Where(x => x.WHID == numtarget && x.Coalition == coal).ToList();
            var bp = db.BattlePonts.First(x => x.Coalition == coal && x.WHID == numtarget);
            var columns = db.ColInput.Where(x => x.IndexPoint == numtarget && x.ArrivalUnit > 0 && x.Coalition == coal).ToList();
            var arrivalBP = 0.00;
            if (columns.Count > 0)
            {
                foreach (var item in columns)
                {
                    double koef = 1;
                    if (item.TypeCol == (int)TypeColumn.Armour)
                    {
                        koef = 1.5;
                    }
                    if (item.TypeCol == (int)TypeColumn.Mixed)
                    {
                        koef = 1.2;
                    }
                    if (item.TypeCol == (int)TypeColumn.Transport)
                    {
                        koef = 1;
                    }
                    arrivalBP += item.ArrivalUnit * koef;
                }
                if (arrivalBP < damage.Count)
                {
                    //for (int i = 0; i < arrivalBP; i++)
                    //{
                    //    int rindex = random.Next(0, damage.Count);
                    //    var ent = damage[rindex];
                    //}
                }
                else
                {
                    foreach (var item in damage)
                    {
                        arrivalBP = arrivalBP - 1;
                    }
                    var finalBP = bp.Point + (int)arrivalBP;
                    if (finalBP > SetApp.Config.BattlePoints)
                    {
                        finalBP = SetApp.Config.BattlePoints;
                    }
                    battlePonts.First(x => x.Coalition == coal && x.WHID == numtarget).Point = finalBP;
                }
            }
            db.Dispose();
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
            var bridges = db.InputsBridge.ToList();
            Bridges.AddRange(bridges);
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
            if (coal == 201 && blueQ.Count > 0)
            {
                currentBluePoint = blueQ.Dequeue();
            }
            if (coal == 101 && redQ.Count > 0)
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
                var inputs = db.ServerInputs.Where(x => x.Coalition != coal && x.IndexPoint == currentBluePoint.IndexPoint && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
                foreach (var item in inputs)
                {
                    RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                    RconCommands.Enqueue(command);
                    int id = item.id;
                    db.ServerInputs.First(x => x.id == id).Enable = 1;
                    var targets = db.CompTarget.Where(x => x.GroupInput == item.GroupInput && x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex).ToList();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        int tarid = targets[i].id;
                        db.CompTarget.First(x => x.id == tarid).Enable = true;
                        targets[i].Enable = true;
                    }
                    ActiveTargets.AddRange(targets);
                }
            }
            if (coal == 101 && currentRedPoint != null)
            {
                var inputs = db.ServerInputs.Where(x => x.Coalition != coal && x.IndexPoint == currentRedPoint.IndexPoint && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
                foreach (var item in inputs)
                {
                    RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                    RconCommands.Enqueue(command);
                    int id = item.id;
                    db.ServerInputs.First(x => x.id == id).Enable = 1;
                    var targets = db.CompTarget.Where(x => x.GroupInput == item.GroupInput && x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex).ToList();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        int tarid = targets[i].id;
                        db.CompTarget.First(x => x.id == tarid).Enable = true;
                        targets[i].Enable = true;
                    }
                    ActiveTargets.AddRange(targets);
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        private bool HandlingForWH(AType3 aType)
        {
            ExpertDB db = new ExpertDB();
            var targets = db.TargetBlock.ToList();
            bool isWH = false;
            foreach (var item in targets)
            {
                var Xres = aType.XPos - item.XPos;
                var Zres = aType.ZPos - item.ZPos;
                double min = -0.1;
                double max = 0.1;
                if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                {
                    if (Blocks.Exists(x => x.ID == aType.TID))
                    {
                        var ent = Blocks.FindLast(x => x.ID == aType.TID);
                        if (ent.TYPE.Contains("[") && ent.TYPE.Contains("]"))
                        {
                            var res = ent.TYPE.Replace("[", "{").Replace("]", "}");
                            var result = reg_brackets.Match(res).Value;
                            var array = result.Split(',');
                            var index = int.Parse(array[1]);
                            db.DamageLog.Add(new DamageLog
                            {
                                BID = item.BID,
                                StructId = index,
                                WHID = item.WHID,
                                Damage = 1,
                                Coalition = item.Coalition
                            });
                            var entbp = db.BattlePonts.First(x => x.WHID == item.WHID && x.Coalition == item.Coalition);
                            int countbp = 0;
                            if ((entbp.Point - 1) > 0)
                            {
                                countbp = entbp.Point - 1;
                            }
                            db.BattlePonts.First(x => x.WHID == item.WHID && x.Coalition == item.Coalition).Point = countbp;
                            int messcoal = 0;
                            if (item.Coalition == 101)
                            {
                                messcoal = 1;
                            }
                            if (item.Coalition == 201)
                            {
                                messcoal = 2;
                            }
                            var mess = "-=COMMANDER=-: Warehouse #" + item.WHID + " attacked";
                            if (!deferredCommands.Exists(x => x.Command == mess))
                            {
                                deferredCommands.Add(new DeferredCommand(mess, 2, messcoal));
                            }
                        }
                        isWH = true;
                        break;
                    }
                }
            }
            db.SaveChanges();
            db.Dispose();
            return isWH;
        }
        /// <summary>
        /// Включает все полевые склады
        /// </summary>
        private void EnableWareHouse()
        {
            ExpertDB db = new ExpertDB();
            var sinputWH = db.ServerInputs.Where(x => x.GroupInput == 8 && !x.Name.Contains("Icon-") && !x.Name.Contains("-OFF-")).ToList();
            foreach (var item in sinputWH)
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
        /// Изменяет коалицию точки в БД
        /// </summary>
        /// <param name="city">Индекс точки</param>
        private void ChangeCoalitionPoint(int city)
        {
            ExpertDB db = new ExpertDB();
            var ent = db.GraphCity.First(x => x.IndexCity == city);
            var victoryCoal = InvertedCoalition(ent.Coalitions);
            victories.Add(new Victory(ent, victoryCoal));
            var afields = db.AirFields.Where(x => x.IndexCity == city).ToList();
            db.GraphCity.First(x => x.IndexCity == city).Coalitions = InvertedCoalition(ent.Coalitions);
            var array = ent.Targets.Split(',');
            foreach (var item in array)
            {
                int index = int.Parse(item);
                var subpoint = db.GraphCity.FirstOrDefault(x => x.IndexCity == index);
                if (subpoint != null)
                {
                    if (subpoint.Coalitions != ent.Coalitions && subpoint.Name_en.Equals("Outside"))
                    {
                        db.GraphCity.First(x => x.IndexCity == index).Coalitions = InvertedCoalition(subpoint.Coalitions);
                    }
                }
            }
            foreach (var item in afields)
            {
                db.AirFields.First(x => x.id == item.id).Coalitions = InvertedCoalition(item.Coalitions);
            }
            db.SaveChanges();
            db.Dispose();
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
        /// Включает иконки разведки
        /// </summary>
        /// <param name="type6">AType:6 объект</param>
        /// <param name="type10">AType:10 объект</param>
        private void EnableRecon(AType6 type6, AType10 type10)
        {
            ExpertDB db = new ExpertDB();
            var allrecon = db.ServerInputs.Where(x => x.Name.Contains("Recon_") && x.Coalition != type10.COUNTRY && x.Enable == 0).ToList();
            for (int i = 0; i < allrecon.Count; i++)
            {
                var dist = SetApp.GetDistance(type6.ZPos, type6.XPos, allrecon[i].ZPos, allrecon[i].XPos);
                if (dist <= 3000 && !LRecon.Exists(x => x.Name.Equals(allrecon[i].Name)))
                {
                    RconCommand wrap = new RconCommand(Rcontype.Input, allrecon[i].Name);
                    RconCommands.Enqueue(wrap);
                    LRecon.Add(allrecon[i]);
                    var ereconname = allrecon[i].Name;
                    db.ServerInputs.First(x => x.Name.Equals(ereconname)).Enable = 1;
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Включает все точки разведки.
        /// </summary>
        private void EnableRecon()
        {
            ExpertDB db = new ExpertDB();
            var allrecon = db.ServerInputs.Where(x => x.Name.Contains("Recon_") && x.Enable == 0).ToList();
            for (int i = 0; i < allrecon.Count; i++)
            {
                RconCommand wrap = new RconCommand(Rcontype.Input, allrecon[i].Name);
                RconCommands.Enqueue(wrap);
                LRecon.Add(allrecon[i]);
                var ereconname = allrecon[i].Name;
                db.ServerInputs.First(x => x.Name.Equals(ereconname)).Enable = 1;
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Снабжение котлов
        /// </summary>
        /// <param name="type6">Событие посадки</param>
        /// <param name="pilot">Пилот</param>
        private void SupplyCauldron(AType6 type6, AType10 pilot)
        {
            if (!AddSupplyPointForCauldron(type6, pilot))
            {
                AddSupplyPointForPilot(type6, pilot);
            }
        }
        private bool AddSupplyPointForCauldron(AType6 type6, AType10 pilot)
        {
            ExpertDB db = new ExpertDB();
            List<GraphCity> SupplyPoint = new List<GraphCity>();
            var citys = db.GraphCity.Where(x => x.Kotel && x.Coalitions == pilot.COUNTRY).ToList();
            foreach (var item in citys)
            {
                if (!item.Name_en.Equals("Outside"))
                {
                    var dist = SetApp.GetDistance(type6.ZPos, type6.XPos, item.ZPos, item.XPos);
                    if (dist <= 2300)
                    {
                        SupplyPoint.Add(item);
                    }
                }
            }
            if (SupplyPoint.Count > 0)
            {
                var minDist = double.MaxValue;
                GraphCity point = new GraphCity();
                foreach (var item in SupplyPoint)
                {
                    var dist = SetApp.GetDistance(type6.ZPos, type6.XPos, item.ZPos, item.XPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        point = item;
                    }
                }
                if (!string.IsNullOrEmpty(point.Name_en))
                {
                    var ent = onlinePlayers.FirstOrDefault(x => x.Name == pilot.NAME);
                    if (pilot.Cargo > 0)
                    {
                        if (ent != null)
                        {
                            var messMoment = "-=COMMANDER=- " + pilot.NAME + " Successful landing. Wait for the plane to unload (~1 min). Takeoff is prohibited.";
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, messMoment, ent.Cid);
                            RconCommands.Enqueue(sendall);
                        }
                        var mess = "-=COMMANDER=- " + pilot.NAME + " the plane is unloaded and you can take off." + " Supplies of food and ammunition have been replenished in the city of " + point.Name_en;
                        airSupplies.Add(new HandlingAirSupply(mess, DateTime.Now, TypeSupply.ForCity, 60, point.IndexCity, pilot));
                        db.Dispose();
                        return true;
                    }
                    else
                    {
                        if (ent != null)
                        {
                            var messMoment = "-=COMMANDER=- " + pilot.NAME + " The plane does not contain cargo.";
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, messMoment, ent.Cid);
                            RconCommands.Enqueue(sendall);
                        }
                    }
                }
            }
            db.Dispose();
            return false;
        }
        private bool AddSupplyPointForPilot(AType6 type6, AType10 pilot)
        {
            ExpertDB db = new ExpertDB();
            List<AirFields> SupplyFields = new List<AirFields>();
            var afields = db.AirFields.Where(x => x.Coalitions == pilot.COUNTRY && !x.NameEn.Equals("Outside")).ToList();
            foreach (var item in afields)
            {
                var dist = SetApp.GetDistance(type6.ZPos, type6.XPos, item.ZPos, item.XPos);
                if (dist <= 1300)
                {
                    SupplyFields.Add(item);
                    break;
                }
            }
            if (SupplyFields.Count > 0)
            {
                var citys = db.GraphCity.FirstOrDefault(x => x.IndexCity == SupplyFields[0].IndexCity);
                var ent = onlinePlayers.FirstOrDefault(x => x.PlayerId == pilot.LOGIN);
                if (!citys.Kotel)
                {
                    if (ent != null)
                    {
                        var messMoment = "-=COMMANDER=- " + pilot.NAME + " Successful landing. Wait for the plane to load (~1 min). Takeoff is prohibited.";
                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, messMoment, ent.Cid);
                        RconCommands.Enqueue(sendall);
                    }
                    var mess = "-=COMMANDER=- " + pilot.NAME + " the plane is loaded, you can take off.";
                    airSupplies.Add(new HandlingAirSupply(mess, DateTime.Now, TypeSupply.ForPilot, 60, 0, pilot));
                    db.Dispose();
                    return true;
                }
                else
                {
                    if (ent != null)
                    {
                        var mess = "-=COMMANDER=- It is impossible to supply the cauldron from this airfield.";
                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Client, mess, ent.Cid);
                        RconCommands.Enqueue(sendall);
                    }
                }
            }
            db.Dispose();
            return false;
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
            messenger.SpecSend("FrontLine");
            messenger.SpecSend("Targets");
            SetEndMission(1);
        }
        /// <summary>
        /// Выявляет победителя текущей миссии
        /// </summary>
        private void CreateVictoryCoalition()
        {
            var countVicRed = victories.Where(x => x.Coalition == 101).ToList().Count;
            var countVicBlue = victories.Where(x => x.Coalition == 201).ToList().Count;
            foreach (var item in victories)
            {
                var mess = "-=COMMANDER=- " + GetNameCoalition(item.Coalition) + " captured the locality of " + item.NameCity;
                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                GetLogStr(mess, Color.Red);
                RconCommands.Enqueue(sendred);
                RconCommands.Enqueue(sendblue);
            }
            if (countVicBlue > countVicRed)
            {
                RconCommand command = new RconCommand(Rcontype.Input, "Victory201");
                RconCommands.Enqueue(command);
            }
            if (countVicBlue < countVicRed)
            {
                RconCommand command = new RconCommand(Rcontype.Input, "Victory101");
                RconCommands.Enqueue(command);
            }
        }
        /// <summary>
        /// Вызывается после завершения генерации миссии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Generation_complete(object sender, EventArgs e)
        {
            bool Loading = false;
            bool PrepareLoc = false;
            bool SavingLoc = false;
            bool SavingList = false;
            bool SavingBin = false;
            string[] content = processGenerator.StandardOutput.ReadToEnd().Split('\r');
            foreach (var item in content)
            {
                if (item.Contains("Loading ") && item.Contains(" DONE"))
                {
                    Loading = true;
                }
                if (item.Contains("Prepare localisation data DONE"))
                {
                    PrepareLoc = true;
                }
                if (item.Contains("Saving localisation data DONE"))
                {
                    SavingLoc = true;
                }
                if (item.Contains("Saving binary data DONE"))
                {
                    SavingBin = true;
                }
                if (item.Contains("Saving .list DONE"))
                {
                    SavingList = true;
                }
            }
            if (Loading && PrepareLoc && SavingBin && SavingList && SavingLoc)
            {
                GetLogArray(content);
                GetLogStr("Restart Mission...", Color.Black);
                NameMission = GetNameNextMission(1);
                ReWriteSDS(SetApp.Config.DirSDS);
                NextMission(SetApp.Config.DirSDS);
            }
            else
            {
                GetLogStr("Generation error. Restart generation. Please wait...", Color.Red);
                StartGeneration();
            }
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
            if (pilotsList.Count > 0)
            {
                pilotsList.Clear();
            }
            if (RconCommands.Count > 0)
            {
                RconCommands.Clear();
            }
            if (LRecon.Count > 0)
            {
                LRecon.Clear();
            }
            if (onlinePlayers.Count > 0)
            {
                onlinePlayers.Clear();
            }
            if (ActiveTargets.Count > 0)
            {
                ActiveTargets.Clear();
            }
            if (victories.Count > 0)
            {
                victories.Clear();
            }
            if (airSupplies.Count > 0)
            {
                airSupplies.Clear();
            }
            if (AllLogs.Count > 0)
            {
                AllLogs.Clear();
            }
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
        private void UpdateCurrentPlayers()
        {
            if (rcon != null && qrcon)
            {
                qrcon = false;
                var players = rcon.GetPlayerList();
                for (int i = 0; i < players.Count; i++)
                {
                    if (!onlinePlayers.Exists(x => x.PlayerId == players[i].PlayerId))
                    {
                        onlinePlayers.Add(players[i]);
                        //CheckRegistration(players[i]);
                    }
                }
                List<Player> deleteplayers = new List<Player>();
                for (int i = 0; i < onlinePlayers.Count; i++)
                {
                    if (!players.Exists(x => x.PlayerId == onlinePlayers[i].PlayerId))
                    {
                        deleteplayers.Add(onlinePlayers[i]);
                    }
                    if (pilotsList.Exists(x => x.LOGIN == onlinePlayers[i].PlayerId) && !deleteplayers.Exists(x => x.PlayerId == onlinePlayers[i].PlayerId))
                    {
                        var pilot = pilotsList.FirstOrDefault(x => x.LOGIN == onlinePlayers[i].PlayerId);
                        if (pilot != null)
                        {
                            onlinePlayers[i].Coalition = pilot.COUNTRY;
                            onlinePlayers[i].IngameStatus = pilot.GameStatus;
                        }
                    }
                }
                for (int i = 0; i < deleteplayers.Count; i++)
                {
                    onlinePlayers.Remove(deleteplayers[i]);
                }
                UpdateDBPlayerList();
                qrcon = true;
            }
        }
        private void UpdateDBPlayerList()
        {
            ExpertDB db = new ExpertDB();
            var dbonline = db.OnlinePilots.ToList();
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                if (onlinePlayers[i].Cid != 0)
                {
                    if (dbonline.Exists(x => x.PlayerId == onlinePlayers[i].PlayerId))
                    {
                        if (dbonline.First(x => x.PlayerId == onlinePlayers[i].PlayerId).IngameStatus != onlinePlayers[i].IngameStatus)
                        {
                            db.OnlinePilots.First(x => x.PlayerId == onlinePlayers[i].PlayerId).IngameStatus = onlinePlayers[i].IngameStatus;
                        }
                        if (dbonline.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Ping != onlinePlayers[i].Ping)
                        {
                            db.OnlinePilots.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Ping = onlinePlayers[i].Ping;
                        }
                        if (dbonline.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Cid != onlinePlayers[i].Cid)
                        {
                            db.OnlinePilots.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Cid = onlinePlayers[i].Cid;
                        }
                        if (dbonline.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Coalition != onlinePlayers[i].Coalition)
                        {
                            db.OnlinePilots.First(x => x.PlayerId == onlinePlayers[i].PlayerId).Coalition = onlinePlayers[i].Coalition;
                        }
                    }
                    else
                    {
                        db.OnlinePilots.Add(new OnlinePilots
                        {
                            Cid = onlinePlayers[i].Cid,
                            IngameStatus = onlinePlayers[i].IngameStatus,
                            PilotName = onlinePlayers[i].Name,
                            Ping = onlinePlayers[i].Ping,
                            PlayerId = onlinePlayers[i].PlayerId,
                            ProfileId = onlinePlayers[i].ProfileId,
                            Coalition = onlinePlayers[i].Coalition
                        });
                    }
                }
            }
            foreach (var item in dbonline)
            {
                if (!onlinePlayers.Exists(x => x.PlayerId == item.PlayerId))
                {
                    db.OnlinePilots.Remove(item);
                }
            }
            db.SaveChanges();
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
            if (rcon != null)
            {
                rcon.Shutdown();
            }
        }
        /// <summary>
        /// Возвращает название цели
        /// </summary>
        /// <param name="GroupInput">Номер цели</param>
        /// <returns>Возвращает название цели</returns>
        private string GetNameTarget(int GroupInput)
        {
            switch (GroupInput)
            {
                case 2:
                    return "Artillery";
                case 3:
                    return "Bridge";
                case 5:
                    return "Fort area";
                case 6:
                    return "Warehouse fuel";
                case 7:
                    return "Tanks reserve";
                case 9:
                    return "Infantry";
                case 10:
                    return "Airfield";
                default:
                    return string.Empty;
            }
        }
        /// <summary>
        /// Получает номер коалиции и возвращает название коалиции в формате строки.
        /// </summary>
        /// <param name="coal">Номер коалиции</param>
        /// <returns></returns>
        private string GetNameCoalition(int coal)
        {
            if (coal == 101)
            {
                return "Allies";
            }
            else
            {
                return "Axis";
            }
        }
        /// <summary>
        /// Меняет коалицию на противоположную
        /// </summary>
        /// <param name="coal">Номер коалиции</param>
        /// <returns>Возвращает противоположную коалицию</returns>
        private int InvertedCoalition(int coal)
        {
            if (coal == 201)
            {
                return 101;
            }
            else
            {
                return 201;
            }
        }
        /// <summary>
        /// Возвращает квадрат в котором произощло событие.
        /// </summary>
        /// <param name="aType">Событие</param>
        /// <returns>Возвращает квадрат в котором произощло событие.</returns>
        private string GetQuadForMap(AType8 aType)
        {
            var firstquad = string.Format("{0:00}", Math.Ceiling((230400 - aType.XPos) / 10000));
            var secondquad = string.Format("{0:00}", Math.Ceiling(aType.ZPos / 10000));
            return firstquad + secondquad;
        }
        /// <summary>
        /// Возвращает квадрат в котором произощло событие.
        /// </summary>
        /// <param name="ZPos">Координата Х</param>
        /// <param name="XPos">Координата Y</param>
        /// <returns>Возвращает квадрат в формате строки, в котором произощло событие.</returns>
        private string GetQuadForMap(double ZPos, double XPos)
        {
            var firstquad = string.Format("{0:00}", Math.Ceiling((230400 - XPos) / 10000));
            var secondquad = string.Format("{0:00}", Math.Ceiling(ZPos / 10000));
            return firstquad + secondquad;
        }
        /// <summary>
        /// Обработка пользовательских направлений атаки. И следом сразу запуск основной генерации миссии.
        /// </summary>
        public void HandleUserDirect()
        {
            SelectUserDirect(101);
            SelectUserDirect(201);
            StartGeneration();
        }
        /// <summary>
        /// Отсев всех лишних пользовательских направлений атаки, если они вообще есть.
        /// </summary>
        /// <param name="coal"></param>
        private void SelectUserDirect(int coal)
        {
            ExpertDB db = new ExpertDB();
            var pd = db.PilotDirect.Where(x => x.Coalition == coal).ToList();
            if (pd.Count > 0)
            {
                var maxVote = pd.Max(x => x.NVote);
                var allMaxVote = pd.Where(x => x.NVote == maxVote).ToList();
                if (allMaxVote.Count > 0)
                {
                    int index = random.Next(0, allMaxVote.Count);
                    var ent = allMaxVote[index];
                    foreach (var item in pd)
                    {
                        if (item.UserId != ent.UserId)
                        {
                            db.PilotDirect.Remove(item);
                        }
                    }
                    db.SaveChanges();
                }
            }
            db.Dispose();
        }
    }
    enum TypeColumn
    {
        Armour = 1,
        Mixed = 2,
        Transport = 3
    }
}
