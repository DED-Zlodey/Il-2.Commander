using Il_2.Commander.Data;
using Il_2.Commander.Parser;
using Il_2.Commander.SRS;
using Newtonsoft.Json;
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
    public delegate void ChangeLog();
    class CommanderCL
    {
        /// <summary>
        /// Событие для передачи строки на форму
        /// </summary>
        public event EventLog GetLogStr;
        /// <summary>
        /// Событие старта планирования атаки на следующую миссию
        /// </summary>
        public event EventLog GetOfficerTime;
        /// <summary>
        /// Событие передачи массива строк на форму
        /// </summary>
        public event EventLogArray GetLogArray;
        /// <summary>
        /// Событие завершения обработки лог фала
        /// </summary>
        public event ChangeLog SetChangeLog;
        /// <summary>
        /// Объект для работы с РКон командами
        /// </summary>
        private static RconCommunicator rcon;
        /// <summary>
        /// Случайности))
        /// </summary>
        private static Random random = new Random();
        /// <summary>
        /// Хаб
        /// </summary>
        private HubMessenger messenger;
        /// <summary>
        /// Процесс генератора
        /// </summary>
        private Process processGenerator;
        /// <summary>
        /// Процесс ДСервера
        /// </summary>
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
        /// <summary>
        /// Все 12 Атайпы в миссии
        /// </summary>
        private List<AType12> AllLogs = new List<AType12>();
        private List<GraphCity> UnlockCauldron = new List<GraphCity>();
        /// <summary>
        /// Список самолетов
        /// </summary>
        private List<PlaneSet> Planeset;
        /// <summary>
        /// Список активных аэродромов в миссии
        /// </summary>
        private List<ATC> ActiveFields = new List<ATC>();
        /// <summary>
        /// Все фразы для голосовых сообщение
        /// </summary>
        private List<SpeechPhrase> Phrases = new List<SpeechPhrase>();
        /// <summary>
        /// Список отправленных голосовых сообщений об атаке цели.
        /// </summary>
        private List<DeferrdedSpeech> DefSpeech = new List<DeferrdedSpeech>();

        /// <summary>
        /// Имя текущей миссии.
        /// </summary>
        private string NameMission = string.Empty;
        /// <summary>
        /// Дата на текущий момент времени
        /// </summary>
        private DateTime dt = DateTime.Now;
        /// <summary>
        /// Дата завершения миссии
        /// </summary>
        private DateTime messDurTime = DateTime.Now;
        /// <summary>
        /// Длительность отсечки, после последнего сообщения об остатке времени до конца миссии, в минутах
        /// </summary>
        private int durmess = 30;
        /// <summary>
        /// Длительность миссии в минутах
        /// </summary>
        private int DurationMission = 235;
        /// <summary>
        /// Если true, можно обрабатывать очередь РКон команд, если fakse нельзя.
        /// </summary>
        bool qrcon = true;
        /// <summary>
        /// Список пилотов
        /// </summary>
        private List<AType10> pilotsList = new List<AType10>();
        /// <summary>
        /// Список пилотов онлайн
        /// </summary>
        private List<Player> onlinePlayers = new List<Player>();
        /// <summary>
        /// Игровая дата
        /// </summary>
        private string GameDate = string.Empty;

        #region Регулярки
        private static Regex reg_brackets = new Regex(@"(?<={).*?(?=})");
        //private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
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
                            var playerdt = DateTime.Now;
                            var playerts = playerdt - dt;
                            var ostatok = Math.Round(DurationMission - playerts.TotalMinutes, 0);
                            if (ostatok <= 0)
                            {
                                ostatok = 0;
                            }
                            var mess = "-=COMMANDER=- " + player.Name + " there are " + ostatok + " minutes left until the end of the mission";
                            RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                            RconCommands.Enqueue(wrap);
                            if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                            {
                                var locpilot = pilotsList.First(x => x.LOGIN == player.PlayerId);
                                if (locpilot.Player == null)
                                {
                                    pilotsList.First(x => x.LOGIN == player.PlayerId).Player = player;
                                }
                                else
                                {
                                    pilotsList.First(x => x.LOGIN == player.PlayerId).Player = player;
                                }
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
                    if (result != null && result.Type == Rcontype.CheckBans)
                    {
                        ExpertDB db = new ExpertDB();
                        var ent = db.BanList.FirstOrDefault(x => x.PlayerId == result.Bans.USERID);
                        if (ent != null)
                        {
                            var DateEndBan = ent.CreateDate.AddHours(ent.HoursBan);
                            if (DateEndBan <= DateTime.Now)
                            {
                                db.BanList.Remove(ent);
                                db.SaveChanges();
                            }
                            else
                            {
                                rcon.Kick(ent.PlayerId);
                                GetLogStr("Pilot BANNED: " + ent.PilotName, Color.Red);
                            }
                        }
                        db.Dispose();
                    }
                    if (result != null && result.Type == Rcontype.CheckRegistration)
                    {
                        ExpertDB db = new ExpertDB();
                        if (result.aType != null)
                        {
                            var profile = db.ProfileUser.FirstOrDefault(x => x.GameId == result.aType.LOGIN);
                            if (profile != null)
                            {
                                if (profile.Coalition > 0)
                                {
                                    if (profile.Coalition == result.aType.COUNTRY)
                                    {
                                        var players = rcon.GetPlayerList();
                                        var player = players.FirstOrDefault(x => x.PlayerId == result.aType.LOGIN);
                                        if (player != null)
                                        {
                                            var mess = "-=COMMANDER=- " + player.Name + " Take-off is allowed.";
                                            //RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                                            //RconCommands.Enqueue(wrap);
                                            GetLogStr(mess, Color.Indigo);
                                            if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                                            {
                                                var locpilot = pilotsList.FirstOrDefault(x => x.LOGIN == player.PlayerId);
                                                if (locpilot != null)
                                                {
                                                    pilotsList.First(x => x.LOGIN == player.PlayerId).TakeOffAllowed = true;
                                                }
                                            }
                                            int indexemo = random.Next(0, 3);
                                            var emo = (EmotionSRS)indexemo;
                                            var ruPhrase = Phrases.Where(x => x.Lang.Equals("ru-RU") && x.Group == 0).ToList();
                                            var enPhrase = Phrases.Where(x => x.Lang.Equals("en-US") && x.Group == 0).ToList();
                                            var indexRuPhrase = random.Next(0, ruPhrase.Count);
                                            var indexEnPhrase = random.Next(0, enPhrase.Count);
                                            var ruMessage = ruPhrase[indexRuPhrase].Message;
                                            var enMessage = enPhrase[indexEnPhrase].Message;
                                            SaveSpeechMessage(result.aType, ruMessage, enMessage, emo);
                                        }
                                    }
                                    else
                                    {
                                        var players = rcon.GetPlayerList();
                                        var player = players.FirstOrDefault(x => x.PlayerId == result.aType.LOGIN);
                                        if (player != null)
                                        {
                                            var mess = "-=COMMANDER=- " + player.Name + " You chose the wrong coalition. Take-off is PROHIBITED!!!";
                                            //RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                                            //RconCommands.Enqueue(wrap);
                                            GetLogStr(mess, Color.Indigo);
                                            if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                                            {
                                                var locpilot = pilotsList.FirstOrDefault(x => x.LOGIN == player.PlayerId);
                                                if (locpilot != null)
                                                {
                                                    pilotsList.First(x => x.LOGIN == player.PlayerId).TakeOffAllowed = true;
                                                }
                                            }
                                            int indexemo = random.Next(0, 3);
                                            var emo = (EmotionSRS)indexemo;
                                            var ruPhrase = Phrases.Where(x => x.Lang.Equals("ru-RU") && x.Group == 0).ToList();
                                            var enPhrase = Phrases.Where(x => x.Lang.Equals("en-US") && x.Group == 0).ToList();
                                            var indexRuPhrase = random.Next(0, ruPhrase.Count);
                                            var indexEnPhrase = random.Next(0, enPhrase.Count);
                                            var ruMessage = ruPhrase[indexRuPhrase].Message;
                                            var enMessage = enPhrase[indexEnPhrase].Message;
                                            SaveSpeechMessage(result.aType, ruMessage, enMessage, emo);
                                        }
                                    }
                                }
                                else
                                {
                                    var players = rcon.GetPlayerList();
                                    var player = players.FirstOrDefault(x => x.PlayerId == result.aType.LOGIN);
                                    if (player != null)
                                    {
                                        var mess = "-=COMMANDER=- " + player.Name + " You didn't choose a coalition. Take-off is PROHIBITED!!!";
                                        //RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                                        //RconCommands.Enqueue(wrap);
                                        GetLogStr(mess, Color.Indigo);
                                        if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                                        {
                                            var locpilot = pilotsList.FirstOrDefault(x => x.LOGIN == player.PlayerId);
                                            if (locpilot != null)
                                            {
                                                pilotsList.First(x => x.LOGIN == player.PlayerId).TakeOffAllowed = true;
                                            }
                                        }
                                        int indexemo = random.Next(0, 3);
                                        var emo = (EmotionSRS)indexemo;
                                        var ruPhrase = Phrases.Where(x => x.Lang.Equals("ru-RU") && x.Group == 0).ToList();
                                        var enPhrase = Phrases.Where(x => x.Lang.Equals("en-US") && x.Group == 0).ToList();
                                        var indexRuPhrase = random.Next(0, ruPhrase.Count);
                                        var indexEnPhrase = random.Next(0, enPhrase.Count);
                                        var ruMessage = ruPhrase[indexRuPhrase].Message;
                                        var enMessage = enPhrase[indexEnPhrase].Message;
                                        SaveSpeechMessage(result.aType, ruMessage, enMessage, emo);
                                    }
                                }
                            }
                            else
                            {
                                var players = rcon.GetPlayerList();
                                var player = players.FirstOrDefault(x => x.PlayerId == result.aType.LOGIN);
                                if (player != null)
                                {
                                    var mess = "-=COMMANDER=- " + player.Name + " You are not registered on the site. Take-off is PROHIBITED!!!";
                                    //RconCommand wrap = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, player.Cid);
                                    //RconCommands.Enqueue(wrap);
                                    GetLogStr(mess, Color.Indigo);
                                    if (pilotsList.Exists(x => x.LOGIN == player.PlayerId))
                                    {
                                        var locpilot = pilotsList.FirstOrDefault(x => x.LOGIN == player.PlayerId);
                                        if (locpilot != null)
                                        {
                                            pilotsList.First(x => x.LOGIN == player.PlayerId).TakeOffAllowed = true;
                                        }
                                    }
                                    int indexemo = random.Next(0, 3);
                                    var emo = (EmotionSRS)indexemo;
                                    var ruPhrase = Phrases.Where(x => x.Lang.Equals("ru-RU") && x.Group == 0).ToList();
                                    var enPhrase = Phrases.Where(x => x.Lang.Equals("en-US") && x.Group == 0).ToList();
                                    var indexRuPhrase = random.Next(0, ruPhrase.Count);
                                    var indexEnPhrase = random.Next(0, enPhrase.Count);
                                    var ruMessage = ruPhrase[indexRuPhrase].Message;
                                    var enMessage = enPhrase[indexEnPhrase].Message;
                                    SaveSpeechMessage(result.aType, ruMessage, enMessage, emo);
                                }
                            }
                        }
                        if (result.Bans != null)
                        {
                            var players = rcon.GetPlayerList();
                            var player = players.FirstOrDefault(x => x.PlayerId == result.Bans.USERID);
                            if (player != null)
                            {
                                CheckRegistration(player);
                            }
                        }
                        db.SaveChanges();
                        db.Dispose();
                    }
                    if (result != null && result.Type == Rcontype.Kick)
                    {
                        if (result.aType != null)
                        {
                            rcon.Kick(result.aType.LOGIN);
                            GetLogStr("Pilot Kicked: " + result.aType.NAME, Color.Red);
                        }
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
                        var mess = item.Command + " Coalition: " + item.Coalition;
                        GetLogStr(mess, Color.IndianRed);
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
                                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, item.Command, pilot.Cid);
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
                                    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, item.Command, pilot.Cid);
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
            if (DefSpeech.Count > 0 && qrcon)
            {
                qrcon = false;
                List<DeferrdedSpeech> delete = new List<DeferrdedSpeech>();
                foreach (var item in DefSpeech)
                {
                    if (item.CreateTime.AddSeconds(item.DurationInSec) < DateTime.Now)
                    {
                        delete.Add(item);
                    }
                }
                foreach (var item in delete)
                {
                    DefSpeech.Remove(item);
                }
                qrcon = true;
            }
            var currentdt = DateTime.Now;
            var curmissend = currentdt - messDurTime;
            var ts = currentdt - dt;
            if (curmissend.TotalMinutes >= durmess)
            {
                messDurTime = DateTime.Now;
                var ostatok = Math.Round(DurationMission - ts.TotalMinutes, 0);
                if (ostatok < 0)
                {
                    ostatok = 0;
                }
                if (ostatok > 0)
                {
                    var mess = "-=COMMANDER=- END of the mission: " + ostatok + " min.";
                    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                    RconCommands.Enqueue(sendall);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    GetLogStr(mess, Color.Black);
                }
            }
            if (ts.TotalMinutes >= DurationMission)
            {
                if (Form1.TriggerTime)
                {
                    Form1.TriggerTime = false;
                    var mess = "-=COMMANDER=- MISSION END.";
                    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                    GetLogStr(mess, Color.Red);
                    RconCommands.Enqueue(sendred);
                    RconCommands.Enqueue(sendblue);
                    CollapsedCauldron();
                    CreateVictoryCoalition();
                    SetUnlockKotel();
                    StartGeneration("pregen");
                }
            }
            if (Form1.TriggerTime)
            {
                Form1.busy = true;
                SetChangeLog();
            }
        }
        /// <summary>
        /// Записывает в БД сообщение для дальнейшей отправки в SRS
        /// </summary>
        /// <param name="aType">AType10 данные о пилоте</param>
        /// <param name="MessageRu">Сообщение на русском языке</param>
        /// <param name="MessageEng">Сообщение на английском языке</param>
        /// <param name="emotion">Эмоциональный окрас сообщения</param>
        private void SaveSpeechMessage(AType10 aType, string MessageRu, string MessageEng, EmotionSRS emotion)
        {
            ExpertDB db = new ExpertDB();
            int coal = 0;
            if (aType.COUNTRY == 101)
            {
                coal = 1;
            }
            if (aType.COUNTRY == 201)
            {
                coal = 2;
            }
            var enATC = GetMinDistanceForATC(aType, "en-US");
            MessageEng = MessageEng.Replace("{RecipientMessage}", aType.NAME).Replace("{AuthorMessage}", enATC.WhosTalking);
            db.Speech.Add(new Speech
            {
                Coalition = coal,
                CreateDate = DateTime.Now,
                Emotion = emotion.ToString(),
                Frequency = 251,
                Lang = "en-US",
                NameSpeaker = enATC.WhosTalking,
                RecipientMessage = aType.NAME,
                Speed = 1.1,
                Voice = enATC.VoiceName,
                Message = MessageEng,
            });
            var ruATC = GetMinDistanceForATC(aType, "ru-RU");
            MessageRu = MessageRu.Replace("{RecipientMessage}", aType.NAME).Replace("{AuthorMessage}", ruATC.WhosTalking);
            db.Speech.Add(new Speech
            {
                Coalition = coal,
                CreateDate = DateTime.Now,
                Emotion = emotion.ToString(),
                Frequency = 252,
                Lang = "ru-RU",
                NameSpeaker = ruATC.WhosTalking,
                RecipientMessage = aType.NAME,
                Speed = 1.1,
                Voice = ruATC.VoiceName,
                Message = MessageRu,
            });
            db.SaveChanges();
            db.Dispose();
        }
        private ATC GetMinDistanceForATC(AType10 aType, string lang)
        {
            var locfields = ActiveFields.Where(x => x.Lang == lang).ToList();
            var MinDist = double.MaxValue;
            ATC localATC = new ATC();
            foreach (var item in locfields)
            {
                var dist = SetApp.GetDistance(aType.ZPos, aType.XPos, item.ZPos, item.XPos);
                if (MinDist > dist)
                {
                    MinDist = dist;
                    localATC = item;
                }
            }
            return localATC;
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
            var ent = db.PreSetupMap.First(x => x.id == actualMissId);
            GameDate = ent.GameDate;
            var output = "Stalingrad-" + ent.GameDate;
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
        /// Старт миссии
        /// </summary>
        public void StartMission()
        {
            UpdateDispatchersATC();
            UpdateActiveFields();
            Planeset = GetPlaneSet();
            messDurTime = DateTime.Now;
            ClearPrevMission();
            dt = DateTime.Now;
            GetLogStr("Mission start: " + dt.ToShortDateString() + " " + dt.ToLongTimeString(), Color.Black);
            GetLogStr("Game date: " + GameDate, Color.DarkCyan);
            SetPhase(0);
            SetDurationMission(1);
            SavedMissionTimeStart();
            InitDirectPoints();
            SetAttackPoint();
            EnableTargetsToCoalition(201);
            EnableTargetsToCoalition(101);
            EnableWareHouse();
            EnableTankBat();
            StartColumn(101);
            StartColumn(201);
            Form1.busy = true;
            Form1.TriggerTime = true;
            SetChangeLog();
        }
        private void UpdateDispatchersATC()
        {
            Phrases.Clear();
            ExpertDB db = new ExpertDB();
            var phr = db.SpeechPhrase.ToList();
            Phrases = phr;
            db.Dispose();
        }
        /// <summary>
        /// Обновление войсов на активных аэродромах
        /// </summary>
        private void UpdateActiveFields()
        {
            var ruDisp = GetNameDispatchers(101);
            var enDisp = GetNameDispatchers(201);
            ActiveFields.Clear();
            ExpertDB db = new ExpertDB();
            var rear = db.RearFields.ToList();
            foreach (var item in rear)
            {
                var indexRuVoice = random.Next(0, ruDisp.Count);
                var indexEnVoice = random.Next(0, enDisp.Count);
                var ruVoice = ruDisp[indexRuVoice];
                var enVoice = enDisp[indexEnVoice];
                ActiveFields.Add(new ATC(item, ruVoice, "ru-RU"));
                ActiveFields.Add(new ATC(item, enVoice, "en-US"));
                ruDisp.Remove(ruVoice);
                enDisp.Remove(enVoice);
            }
            db.Dispose();
        }
        /// <summary>
        /// Получает сетап самолетов из базы данных
        /// </summary>
        /// <returns>Возвращает список самолетов</returns>
        private List<PlaneSet> GetPlaneSet()
        {
            ExpertDB db = new ExpertDB();
            var planeset = db.PlaneSet.ToList();
            db.Dispose();
            return planeset;
        }
        /// <summary>
        /// Включает танковые батальоны (эксперементально)
        /// </summary>
        private void EnableTankBat()
        {
            ExpertDB db = new ExpertDB();
            var blueTB = db.ServerInputs.Where(x => x.Coalition == 201 && x.GroupInput == 15 && x.Name.Contains("_On_")).ToList();
            var redTB = db.ServerInputs.Where(x => x.Coalition == 101 && x.GroupInput == 15 && x.Name.Contains("_On_")).ToList();
            foreach (var item in blueTB)
            {
                RconCommand command = new RconCommand(Rcontype.Input, item.Name);
                RconCommands.Enqueue(command);
                int id = item.id;
                db.ServerInputs.First(x => x.id == id).Enable = 1;
            }
            foreach (var item in redTB)
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
        /// Задает текущую игровую фазу, записывает значение фазы и отправляет ее значение в хаб для отображения всем пользователям о текущем состоянии игры.
        /// </summary>
        /// <param name="phase">Принимает номер фазы</param>
        private void SetPhase(int phase)
        {
            string mess = string.Empty;
            ExpertDB db = new ExpertDB();
            var phases = db.PhaseGen.ToList();
            foreach (var item in phases)
            {
                db.PhaseGen.Remove(item);
            }
            db.SaveChanges();
            if (phase == 0)
            {
                db.PhaseGen.Add(new PhaseGen
                {
                    CreateDate = DateTime.Now,
                    NPhase = 0
                });
                db.SaveChanges();
                messenger.SpecSend("Phase0");
            }
            if (phase == 1)
            {
                mess = "-=COMMANDER=- Planning for an attack on the next mission began. 3 minutes";
                RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                RconCommands.Enqueue(sendall);
                RconCommands.Enqueue(sendred);
                RconCommands.Enqueue(sendblue);
                db.PhaseGen.Add(new PhaseGen
                {
                    CreateDate = DateTime.Now,
                    NPhase = 1
                });
                db.SaveChanges();
                messenger.SpecSend("Phase1");
            }
            if (phase == 2)
            {
                mess = "-=COMMANDER=- Start generation next mission";
                RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
                RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
                RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
                RconCommands.Enqueue(sendall);
                RconCommands.Enqueue(sendred);
                RconCommands.Enqueue(sendblue);
                db.PhaseGen.Add(new PhaseGen
                {
                    CreateDate = DateTime.Now,
                    NPhase = 2
                });
                db.SaveChanges();
                messenger.SpecSend("Phase2");
            }
            if (phase == 3)
            {
                db.PhaseGen.Add(new PhaseGen
                {
                    CreateDate = DateTime.Now,
                    NPhase = 3
                });
                db.SaveChanges();
                messenger.SpecSend("Phase3");
            }
            db.Dispose();
        }
        /// <summary>
        /// Вызывается при приходе каждого лога?
        /// </summary>
        public void CheckEveryLog()
        {
            UpdateCurrentPlayers();
            if (Form1.TriggerTime)
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
                if (str[i].Contains("AType:20 "))
                {
                    AType20 aType = new AType20(str[i]);
                    RconCommand wrap = new RconCommand(Rcontype.CheckBans, aType);
                    RconCommands.Enqueue(wrap);
                    RconCommand wrap1 = new RconCommand(Rcontype.CheckRegistration, aType);
                    RconCommands.Enqueue(wrap1);
                }
                if (str[i].Contains("AType:10 "))
                {
                    AType10 aType = new AType10(str[i]);
                    if (AllLogs.Exists(x => x.ID == aType.PLID && x.TypeVeh == TypeAtype12.AirCraft))
                    {
                        var ent = AllLogs.FindLast(x => x.ID == aType.PLID && x.TypeVeh == TypeAtype12.AirCraft);
                        if (ent != null)
                        {
                            aType.ParenEnt.Add(ent);
                            AllLogs.Remove(ent);
                        }
                    }
                    if (AllLogs.Exists(x => x.ID == aType.PID && x.TypeVeh == TypeAtype12.BotBotPilot))
                    {
                        var ent = AllLogs.FindLast(x => x.ID == aType.PID && x.TypeVeh == TypeAtype12.BotBotPilot);
                        if (ent != null)
                        {
                            aType.ParenEnt.Add(ent);
                            AllLogs.Remove(ent);
                        }
                    }
                    if (aType.TYPE.Contains("Ju 52 3mg4e"))
                    {
                        var planeent = aType.ParenEnt.FirstOrDefault(x => x.TypeVeh == TypeAtype12.AirCraft);
                        if (planeent != null)
                        {
                            var planeName = planeent.NAME;
                            if (!string.IsNullOrEmpty(planeName))
                            {
                                var numfield = int.Parse(planeName.Substring(planeName.Length - 1));
                                var index = planeName.Length - 2;
                                planeName = planeName.Substring(0, index);
                                if (planeName.Equals("Transport"))
                                {
                                    aType.Cargo = 0.35;
                                }
                            }
                        }
                    }
                    pilotsList.Add(aType);
                    if (onlinePlayers.Exists(x => x.PlayerId == aType.LOGIN))
                    {
                        onlinePlayers.First(x => x.PlayerId == aType.LOGIN).IngameStatus = GameStatusPilot.Parking.ToString();
                        onlinePlayers.First(x => x.PlayerId == aType.LOGIN).Coalition = aType.COUNTRY;
                    }
                    RconCommand wrap = new RconCommand(Rcontype.Players, aType);
                    RconCommands.Enqueue(wrap);
                    RconCommand wrap1 = new RconCommand(Rcontype.CheckRegistration, aType);
                    RconCommands.Enqueue(wrap1);
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
                            HandleBotDispose(aType, ent);
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
                    if (pilotsList.Exists(x => x.PLID == aType.TID || x.PID == aType.TID))
                    {
                        var pilot = pilotsList.FirstOrDefault(x => x.PLID == aType.TID || x.PID == aType.TID);
                        if (pilot != null)
                        {
                            pilotsList.FirstOrDefault(x => x.PLID == aType.TID || x.PID == aType.TID).Death = new AType3(aType);
                        }
                    }
                }
                if (str[i].Contains("AType:5 "))
                {
                    var aType = new AType5(str[i]);
                    if (pilotsList.Exists(x => x.PLID == aType.PID))
                    {
                        var pilot = pilotsList.First(x => x.PLID == aType.PID);
                        pilotsList.First(x => x.PLID == aType.PID).DamageList.Clear();
                        pilotsList.First(x => x.PLID == aType.PID).Death = null;
                        if (!pilot.TakeOffAllowed)
                        {
                            RconCommand wrap = new RconCommand(Rcontype.Kick, pilot);
                            RconCommands.Enqueue(wrap);
                        }
                        else
                        {
                            pilotsList.First(x => x.PLID == aType.PID).GameStatus = GameStatusPilot.InFlight.ToString();
                            var playerid = pilotsList.First(x => x.PLID == aType.PID).LOGIN;
                            if (onlinePlayers.Exists(x => x.PlayerId == playerid))
                            {
                                onlinePlayers.First(x => x.PlayerId == playerid).IngameStatus = GameStatusPilot.InFlight.ToString();
                            }
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
                        int numfield = 0;
                        var ent = pilotsList.First(x => x.PLID == aType.PID);
                        var entPlane = ent.ParenEnt.FirstOrDefault(x => x.TypeVeh == TypeAtype12.AirCraft);
                        if (entPlane != null)
                        {
                            var planeName = entPlane.NAME;
                            if (!string.IsNullOrEmpty(planeName))
                            {
                                numfield = int.Parse(planeName.Substring(planeName.Length - 1));
                                var index = planeName.Length - 2;
                                planeName = planeName.Substring(0, index);
                            }
                            if (ent.TYPE.Contains("Ju 52 3mg4e"))
                            {
                                if (!string.IsNullOrEmpty(planeName))
                                {
                                    if (planeName.Equals("Transport"))
                                    {
                                        SupplyCauldron(aType, ent);
                                    }
                                }
                            }
                        }
                        pilotsList.First(x => x.PLID == aType.PID).GameStatus = GameStatusPilot.Parking.ToString();
                        var playerid = ent.LOGIN;
                        if (onlinePlayers.Exists(x => x.PlayerId == playerid))
                        {
                            onlinePlayers.First(x => x.PlayerId == playerid).IngameStatus = GameStatusPilot.InFlight.ToString();
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
                if (str[i].Contains("AType:2 "))
                {
                    AType2 aType = new AType2(str[i]);
                    if (pilotsList.Exists(x => x.PLID == aType.TID) || pilotsList.Exists(x => x.PID == aType.TID))
                    {
                        var ent = pilotsList.FirstOrDefault(x => x.PLID == aType.TID || x.PID == aType.TID);
                        if (ent != null)
                        {
                            pilotsList.FirstOrDefault(x => x.PLID == aType.TID || x.PID == aType.TID).DamageList.Add(aType);
                        }
                    }
                }
            }
            if (updateTarget)
            {
                if (messenger != null)
                {
                    messenger.SpecSend("Targets");
                }
            }
            CheckEveryLog();
            if (Form1.TriggerTime)
            {
                Form1.busy = true;
                SetChangeLog();
            }
        }
        /// <summary>
        /// Обработка утилизации бота. Когда пилот заканчивает сессию.
        /// </summary>
        /// <param name="aType">Событие AType:16</param>
        /// <param name="pilot">Событие AType:10</param>
        private void HandleBotDispose(AType16 aType, AType10 pilot)
        {
            if (pilot.Death == null && pilot.DamageList.Count > 0)
            {
                if (!CheckBotDisposeFieldArea(aType, pilot.COUNTRY) && !CheckBotDisposeForServiceArea(aType, pilot.COUNTRY))
                {
                    ExpertDB db = new ExpertDB();
                    var Planeset = db.PlaneSet.Where(x => x.Coalition == pilot.COUNTRY).ToList();
                    int numfield = 0;
                    var plane = pilot.ParenEnt.FirstOrDefault(x => x.TypeVeh == TypeAtype12.AirCraft);
                    if (plane != null)
                    {
                        if (Planeset.Exists(x => x.LogType == plane.TYPE))
                        {
                            var planeName = plane.NAME;
                            if (!string.IsNullOrEmpty(planeName))
                            {
                                numfield = int.Parse(planeName.Substring(planeName.Length - 1));
                                var index = planeName.Length - 2;
                                planeName = planeName.Substring(0, index);
                                var psent = Planeset.FirstOrDefault(x => x.LogType == plane.TYPE && x.Name == planeName && x.NumField == numfield && x.Coalition == plane.COUNTRY);
                                if (psent != null)
                                {
                                    var ncraft = psent.Number - 1;
                                    if (ncraft < 0)
                                    {
                                        ncraft = 0;
                                    }
                                    db.PlaneSet.First(x => x.LogType == plane.TYPE && x.Name == planeName && x.NumField == numfield && x.Coalition == psent.Coalition).Number = ncraft;
                                    var mess = "-=COMMANDER=-: Destroyed " + psent.LogType + " " + psent.Name + " AirField: " + numfield + " " + psent.Coalition + " NumPlanes: " + ncraft.ToString() + " " + pilot.NAME;
                                    GetLogStr(mess, Color.Red);
                                    var orders = db.PlanesOrders.ToList();
                                    if (orders.Exists(x => x.PlaneSetId == psent.id && x.DateDeath == DateTime.Parse(GameDate)))
                                    {
                                        var orderent = db.PlanesOrders.First(x => x.PlaneSetId == psent.id);
                                        db.PlanesOrders.First(x => x.PlaneSetId == psent.id).Number = orderent.Number + 1;
                                    }
                                    else
                                    {
                                        db.PlanesOrders.Add(new PlanesOrders
                                        {
                                            Coalition = psent.Coalition,
                                            DateDeath = DateTime.Parse(GameDate),
                                            FreqSupply = psent.FreqSupply,
                                            Name = psent.Name,
                                            Number = 1,
                                            PlaneSetId = psent.id
                                        });
                                    }
                                    db.SaveChanges();
                                    if (messenger != null)
                                    {
                                        messenger.SpecSend("FrontLine");
                                    }
                                }
                            }
                        }
                    }
                    db.Dispose();
                }
            }
            if (pilot.Death != null)
            {
                HandleKillPilot(pilot);
            }
        }
        /// <summary>
        /// Проверяет утилизирован ли бот внутри зоны обслуживания аэродрома подскока?
        /// </summary>
        /// <returns>Возвращает true, если бот утилизирован внутри зоны обслуживания и false если вне зоны обслуживания</returns>
        private bool CheckBotDisposeForServiceArea(AType16 aType, int coal)
        {
            ExpertDB db = new ExpertDB();
            var services = db.ServiceArea.Where(x => x.Coalition == coal).ToList();
            db.Dispose();
            foreach (var item in services)
            {
                var dist = SetApp.GetDistance(aType.ZPos, aType.XPos, item.ZPos, item.XPos);
                if (dist <= item.MaintenanceRadius)
                {
                    var diff = aType.YPos - item.YPos;
                    if (diff > 1 || diff < -1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Проверяет утилизирован ли бот в зоне активного аэродрома?
        /// </summary>
        /// <returns>Возвращает true, если утилизация бота произошла в зоне активного аэродрома и false если вне зоны</returns>
        private bool CheckBotDisposeFieldArea(AType16 aType, int coal)
        {
            ExpertDB db = new ExpertDB();
            var rearfields = db.RearFields.Where(x => x.Coalition == coal).ToList();
            var af = db.AirFields.Where(x => x.Coalitions == coal).ToList();
            db.Dispose();
            List<AirFields> localfields = new List<AirFields>();
            foreach (var item in rearfields)
            {
                if (af.Exists(x => x.XPos == item.XPos && x.ZPos == item.ZPos))
                {
                    var ent = af.FirstOrDefault(x => x.XPos == item.XPos && x.ZPos == item.ZPos);
                    if (ent != null)
                    {
                        localfields.Add(ent);
                    }
                }
            }
            foreach (var item in af)
            {
                var dist = SetApp.GetDistance(aType.ZPos, aType.XPos, item.ZPos, item.XPos);
                if (dist <= 2000)
                {
                    var diff = aType.YPos - item.YPos;
                    if (diff > 5 || diff < -5)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        ///  Обработка уничтожения самолета
        /// </summary>
        /// <param name="aType"></param>
        private void HandleKillPilot(AType10 pilot)
        {
            ExpertDB db = new ExpertDB();
            var Planeset = db.PlaneSet.Where(x => x.Coalition == pilot.COUNTRY).ToList();
            int numfield = 0;
            var ent = pilot.ParenEnt.FirstOrDefault(x => x.TypeVeh == TypeAtype12.AirCraft);
            if (ent != null)
            {
                var planeName = ent.NAME;
                if (!string.IsNullOrEmpty(planeName))
                {
                    numfield = int.Parse(planeName.Substring(planeName.Length - 1));
                    var index = planeName.Length - 2;
                    planeName = planeName.Substring(0, index);
                    var psent = Planeset.FirstOrDefault(x => x.LogType == ent.TYPE && x.Name == planeName && x.NumField == numfield && x.Coalition == ent.COUNTRY);
                    if (psent != null)
                    {
                        var ncraft = psent.Number - 1;
                        if (ncraft < 0)
                        {
                            ncraft = 0;
                        }
                        db.PlaneSet.First(x => x.LogType == ent.TYPE && x.Name == planeName && x.NumField == numfield && x.Coalition == psent.Coalition).Number = ncraft;
                        var mess = "-=COMMANDER=-: Destroyed " + psent.LogType + " " + psent.Name + " AirField: " + numfield + " " + psent.Coalition + " NumPlanes: " + ncraft.ToString() + " " + pilot.NAME;
                        GetLogStr(mess, Color.Red);
                        var orders = db.PlanesOrders.ToList();
                        if (orders.Exists(x => x.PlaneSetId == psent.id && x.DateDeath == DateTime.Parse(GameDate)))
                        {
                            var orderent = db.PlanesOrders.First(x => x.PlaneSetId == psent.id);
                            db.PlanesOrders.First(x => x.PlaneSetId == psent.id).Number = orderent.Number + 1;
                        }
                        else
                        {
                            db.PlanesOrders.Add(new PlanesOrders
                            {
                                Coalition = psent.Coalition,
                                DateDeath = DateTime.Parse(GameDate),
                                FreqSupply = psent.FreqSupply,
                                Name = psent.Name,
                                Number = 1,
                                PlaneSetId = psent.id
                            });
                        }
                        db.SaveChanges();
                        if (messenger != null)
                        {
                            messenger.SpecSend("FrontLine");
                        }
                    }
                }
            }
            db.Dispose();
        }
        /// <summary>
        /// Старт колонн
        /// </summary>
        /// <param name="coal">номер коалиции</param>
        private void StartColumn(int coal)
        {
            ExpertDB db = new ExpertDB();
            //var bp = battlePonts.Where(x => x.Coalition == coal).OrderBy(x => x.Point).ToList();
            var allcolumn = db.ColInput.Where(x => x.Coalition == coal && x.Permit).ToList();
            var ActivCol = db.ColInput.Where(x => x.Coalition == coal && x.Active).ToList();
            List<BattlePonts> localBP = new List<BattlePonts>();
            for (int i = 0; i < 5; i++)
            {
                localBP.Add(GetBP(i + 1, coal));
            }
            localBP = localBP.OrderBy(x => x.Point).ToList();
            foreach (var item in ActivCol)
            {
                var entBP = localBP.FirstOrDefault(x => x.WHID == item.NWH && x.Coalition == coal);
                if (entBP != null)
                {
                    localBP.Remove(entBP);
                }
            }
            if (localBP.Exists(x => x.Point < SetApp.Config.BattlePoints))
            {
                if (ActivCol.Count < 2 && allcolumn.Count > 0)
                {
                    int iter = 2 - ActivCol.Count;
                    for (int i = 0; i < iter; i++)
                    {
                        var allwhcol = allcolumn.Where(x => x.NWH == localBP[i].WHID && x.Coalition == localBP[i].Coalition).ToList();
                        if (allwhcol.Count > 0 && localBP[i].Point < SetApp.Config.BattlePoints)
                        {
                            int rindex = random.Next(0, allwhcol.Count);
                            var inputmess = allwhcol[rindex].NameCol;
                            var ent = allwhcol[rindex];
                            ent.Active = true;
                            ActiveColumn.Add(ent);
                            RconCommand command = new RconCommand(Rcontype.Input, ent.NameCol);
                            RconCommands.Enqueue(command);
                            db.ColInput.First(x => x.NameCol == inputmess).Active = true;
                            db.ColInput.First(x => x.NameCol == inputmess).Permit = false;
                        }
                        if (allwhcol.Count == 0 && iter < localBP.Count)
                        {
                            iter++;
                        }
                    }
                    db.SaveChanges();
                }
            }
            db.Dispose();
        }
        /// <summary>
        /// Обработка одного килла
        /// </summary>
        /// <param name="aType">Событие Атайп3</param>
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
            var entNameTID = string.Empty;
            var entAt12 = AllLogs.FindLast(x => x.ID == aType.TID);
            if (entAt12 != null)
            {
                entNameTID = entAt12.NAME;
            }
            bool output = false;
            ExpertDB db = new ExpertDB();
            var targets = db.CompTarget.Where(x => x.Enable).ToList();
            if(targets.Exists(x => x.EntName.Equals(entNameTID) && x.Mandatory))
            {
                var item = targets.First(x => x.EntName.Equals(entNameTID));
                bool enable = false;
                var ent = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-OFF-") && !x.Name.Contains("Icon-"));
                var entON = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-"));
                if (entON.Enable == 1)
                {
                    if (item.InernalWeight > item.Destroed)
                    {
                        enable = true;
                        int destroy = item.Destroed + 1;
                        targets.First(x => x.id == item.id).Destroed = destroy;
                        db.CompTarget.First(x => x.id == item.id).Destroed = destroy;
                        var DestroyedMess = "-=COMMANDER=- " + item.Name + " " + item.Model + " " + entON.Coalition + " destroyed";
                        GetLogStr(DestroyedMess, Color.DarkOrange);
                        if (!DefSpeech.Exists(x => x.IndexAuthor == entON.IndexPoint && x.SubIndexAuthor == entON.SubIndex && x.AuthorMessage == entON.AssociateNameRU))
                        {
                            var coal = 1;
                            if (entON.Coalition == 201)
                            {
                                coal = 2;
                            }
                            var actualphraseRU = Phrases.Where(x => x.Lang == "ru-RU" && x.Group == 1).ToList();
                            var actualphraseEN = Phrases.Where(x => x.Lang == "en-US" && x.Group == 1).ToList();
                            var square = GetQuadForMap(entON.ZPos, entON.XPos);
                            if (actualphraseRU.Count > 0)
                            {
                                int rindex = random.Next(0, actualphraseRU.Count);
                                var MessageRU = actualphraseRU[rindex].Message.Replace("{AuthorMessage}", entON.AssociateNameRU).Replace("{Quad}", square);
                                SaveTargetMessage(MessageRU, entON.AssociateNameRU, "All", entON.VoiceRU, coal, 252, "ru-RU");
                            }
                            if (actualphraseEN.Count > 0)
                            {
                                int rindex = random.Next(0, actualphraseEN.Count);
                                var MessageEN = actualphraseEN[rindex].Message.Replace("{AuthorMessage}", entON.AssociateNameEN).Replace("{Quad}", square);
                                SaveTargetMessage(MessageEN, entON.AssociateNameEN, "All", entON.VoiceEN, coal, 251, "en-US");
                            }
                            DefSpeech.Add(new DeferrdedSpeech(entON.AssociateNameRU, entON.IndexPoint, entON.SubIndex, "All", "11", coal, 180));
                        }
                    }
                    var countMandatory = targets.Where(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Mandatory).ToList().Count - 1;
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
                            enable = true;
                        }
                    }
                    db.SaveChanges();
                    var alltargets = db.ServerInputs.Where(x => x.IndexPoint == ent.IndexPoint && x.Enable == 1 && x.GroupInput != 14 && x.GroupInput != 15).ToList();
                    db.Dispose();
                    if (alltargets.Count == 0)
                    {
                        int invcoal = InvertedCoalition(ent.Coalition);
                        ChangeCoalitionPoint(ent.IndexPoint);
                        SetAttackPoint(invcoal);
                        EnableTargetsToCoalition(invcoal);
                    }
                    if (enable)
                    {
                        output = true;
                    }
                }
            }
            else
            {
                foreach (var item in targets)
                {
                    var Xres = aType.XPos - item.XPos;
                    var Zres = aType.ZPos - item.ZPos;
                    double min = -0.1;
                    double max = 0.1;
                    if ((Xres < max && Zres < max) && (Xres > min && Zres > min))
                    {
                        bool enable = false;
                        var ent = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-OFF-") && !x.Name.Contains("Icon-"));
                        var entON = db.ServerInputs.First(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Name.Contains("-ON-") && !x.Name.Contains("Icon-"));
                        if (entON.Enable == 1)
                        {
                            if (item.InernalWeight > item.Destroed)
                            {
                                enable = true;
                                int destroy = item.Destroed + 1;
                                targets.First(x => x.id == item.id).Destroed = destroy;
                                db.CompTarget.First(x => x.id == item.id).Destroed = destroy;
                                var DestroyedMess = "-=COMMANDER=- " + item.Name + " " + item.Model + " " + entON.Coalition + " destroyed";
                                GetLogStr(DestroyedMess, Color.DarkGoldenrod);
                                if (!DefSpeech.Exists(x => x.IndexAuthor == entON.IndexPoint && x.SubIndexAuthor == entON.SubIndex && x.AuthorMessage == entON.AssociateNameRU))
                                {
                                    var coal = 1;
                                    if (entON.Coalition == 201)
                                    {
                                        coal = 2;
                                    }
                                    var actualphraseRU = Phrases.Where(x => x.Lang == "ru-RU" && x.Group == 1).ToList();
                                    var actualphraseEN = Phrases.Where(x => x.Lang == "en-US" && x.Group == 1).ToList();
                                    var square = GetQuadForMap(entON.ZPos, entON.XPos);
                                    if (actualphraseRU.Count > 0)
                                    {
                                        int rindex = random.Next(0, actualphraseRU.Count);
                                        var MessageRU = actualphraseRU[rindex].Message.Replace("{AuthorMessage}", entON.AssociateNameRU).Replace("{Quad}", square);
                                        SaveTargetMessage(MessageRU, entON.AssociateNameRU, "All", entON.VoiceRU, coal, 252, "ru-RU");
                                    }
                                    if (actualphraseEN.Count > 0)
                                    {
                                        int rindex = random.Next(0, actualphraseEN.Count);
                                        var MessageEN = actualphraseEN[rindex].Message.Replace("{AuthorMessage}", entON.AssociateNameEN).Replace("{Quad}", square);
                                        SaveTargetMessage(MessageEN, entON.AssociateNameEN, "All", entON.VoiceEN, coal, 251, "en-US");
                                    }
                                    DefSpeech.Add(new DeferrdedSpeech(entON.AssociateNameRU, entON.IndexPoint, entON.SubIndex, "All", "11", coal, 180));
                                }
                            }
                            var countMandatory = targets.Where(x => x.IndexPoint == item.IndexPoint && x.SubIndex == item.SubIndex && x.Mandatory).ToList().Count - 1;
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
                                    enable = true;
                                }
                            }
                            db.SaveChanges();
                            var alltargets = db.ServerInputs.Where(x => x.IndexPoint == ent.IndexPoint && x.Enable == 1 && x.GroupInput != 14 && x.GroupInput != 15).ToList();
                            db.Dispose();
                            if (alltargets.Count == 0)
                            {
                                int invcoal = InvertedCoalition(ent.Coalition);
                                ChangeCoalitionPoint(ent.IndexPoint);
                                SetAttackPoint(invcoal);
                                EnableTargetsToCoalition(invcoal);
                            }
                            if (enable)
                            {
                                output = true;
                                break;
                            }
                        }
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
        /// Сохраняет в БД сообщение для последующей его конвертации в голосовое
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="author">Автор сообщения</param>
        /// <param name="recipient">Получатель сообщения</param>
        /// <param name="voice">Имя голоса</param>
        /// <param name="coal">Коалиция</param>
        /// <param name="freq">Частота на которой следует передавать голосовое сообщение</param>
        /// <param name="lang">Язык сообщения</param>
        private void SaveTargetMessage(string message, string author, string recipient, string voice, int coal, double freq, string lang)
        {
            ExpertDB db = new ExpertDB();
            db.Speech.Add(new Speech
            {
                Coalition = coal,
                CreateDate = DateTime.Now,
                Emotion = "evil",
                Frequency = freq,
                Lang = lang,
                Message = message,
                NameSpeaker = author,
                RecipientMessage = recipient,
                Speed = 1.1,
                Voice = voice
            });
            db.SaveChanges();
            db.Dispose();
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
                var column = ActiveColumn.FirstOrDefault(x => x.NameCol == ent.NAME);
                if (column != null)
                {
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
            //RestoreWareHouseInMemory(column.NWH, column.Coalition);
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
                        string namecol = item.NameObjective;
                        var column12 = ColumnAType12.Where(x => x.NAME.Equals(namecol)).ToList();
                        var column12Dead = ColumnAType12.Where(x => x.NAME.Equals(namecol) && x.Destroyed).ToList();
                        var altmess = "-=COMMANDER=-:  Сargo convoy for warehouse: " + ent.NWH + " Coalition: " + ent.Coalition + " was stopped due to the destruction of the bridge. Destroyed unit: " + column12Dead.Count;
                        GetLogStr(altmess, Color.DarkViolet);
                        var allArrivalCol = ent.ArrivalCol + 1;
                        var allArrivalUnits = (int)(ent.Unit / 2) - column12Dead.Count;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalUnit = ent.ArrivalUnit + allArrivalUnits;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalCol = allArrivalCol;
                        db.ColInput.First(x => x.NameCol == namecol).Active = false;
                        db.ColInput.First(x => x.NameCol == namecol).DestroyedUnits = ent.DestroyedUnits + column12Dead.Count + ent.Unit / 2;
                        ActiveColumn.Remove(ent);
                        foreach (var colitem in column12Dead)
                        {
                            ColumnAType12.Remove(colitem);
                        }
                        //RestoreWareHouseInMemory(ent.NWH, ent.Coalition, db);
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
                        string namecol = item.NameObjective;
                        var column12 = ColumnAType12.Where(x => x.NAME.Equals(namecol)).ToList();
                        var column12Dead = ColumnAType12.Where(x => x.NAME.Equals(namecol) && x.Destroyed).ToList();
                        var altmess = "-=COMMANDER=-:  Сargo convoy for warehouse: " + ent.NWH + " Coalition: " + ent.Coalition + " arrived at its destination. Destroyed unit: " + column12Dead.Count;
                        GetLogStr(altmess, Color.DarkViolet);
                        var allArrivalCol = ent.ArrivalCol + 1;
                        var allArrivalUnits = ent.Unit - column12Dead.Count;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalUnit = ent.ArrivalUnit + allArrivalUnits;
                        db.ColInput.First(x => x.NameCol == namecol).ArrivalCol = allArrivalCol;
                        db.ColInput.First(x => x.NameCol == namecol).Active = false;
                        db.ColInput.First(x => x.NameCol == namecol).DestroyedUnits = ent.DestroyedUnits + column12Dead.Count;
                        ActiveColumn.Remove(ent);
                        foreach (var colitem in column12Dead)
                        {
                            ColumnAType12.Remove(colitem);
                        }
                        //RestoreWareHouseInMemory(ent.NWH, ent.Coalition, db);
                    }
                }
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Возвращает текущее количество ресурсов на определенном полевом складе
        /// </summary>
        /// <param name="numtarget">Номер полевого склада</param>
        /// <param name="coal">Номер коалиции полевого склада</param>
        /// <returns>Возвращает текущее количество ресурсов на полевом складе</returns>
        private BattlePonts GetBP(int numtarget, int coal)
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
                    double koef = SetApp.Config.TransportMult;
                    var wcol = koef * 12;
                    if (item.TypeCol == (int)TypeColumn.Armour)
                    {
                        koef = wcol / 8;
                    }
                    if (item.TypeCol == (int)TypeColumn.Mixed)
                    {
                        koef = wcol / 10;
                    }
                    if (item.TypeCol == (int)TypeColumn.Transport)
                    {
                        koef = wcol / 12;
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
                    bp.Point = finalBP;
                }
            }
            db.Dispose();
            return bp;
        }
        /// <summary>
        /// Получает из базы данны направления атак и формирует очередность включения целей на точках, которые входят в направление атаки. Так же инициализирует список инпутов мостов.
        /// </summary>
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
        /// Устанавливает текущую точку атаки в зависимости от коалиции
        /// </summary>
        /// <param name="coal">Принимает номер коалиции для которой требуется установить текущую точку атаки</param>
        private void SetAttackPoint(int coal)
        {
            if (coal == 201)
            {
                if (blueQ.Count > 0)
                {
                    currentBluePoint = blueQ.Dequeue();
                }
                else
                {
                    currentBluePoint = null;
                }
            }
            if (coal == 101)
            {
                if (redQ.Count > 0)
                {
                    currentRedPoint = redQ.Dequeue();
                }
                else
                {
                    currentRedPoint = null;
                }
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
        /// <summary>
        /// Обработка события "смерти". Вернет true если найдено что-то уничтоженное в составе полевых складов и false если полевые склады не повреждены. Если уничтождение полевого склада обнаружено, обработает
        /// это уничтожение, запишет в БД всю актуальную информацию.
        /// </summary>
        /// <param name="aType"></param>
        /// <returns>Вернет true если найдено что-то уничтоженное в составе полевых складов и false если полевые склады не повреждены</returns>
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
                            if (!DefSpeech.Exists(x => x.IndexAuthor == entbp.WHID && x.SubIndexAuthor == entbp.Coalition && x.AuthorMessage == entbp.AssociateNameRU))
                            {
                                var actualphraseRU = Phrases.Where(x => x.Lang == "ru-RU" && x.Group == 1).ToList();
                                var actualphraseEN = Phrases.Where(x => x.Lang == "en-US" && x.Group == 1).ToList();
                                var square = GetQuadForMap(aType.ZPos, aType.XPos);
                                if (actualphraseRU.Count > 0)
                                {
                                    int rindex = random.Next(0, actualphraseRU.Count);
                                    var MessageRU = actualphraseRU[rindex].Message.Replace("{AuthorMessage}", entbp.AssociateNameRU).Replace("{Quad}", square);
                                    SaveTargetMessage(MessageRU, entbp.AssociateNameRU, "All", entbp.VoiceRU, messcoal, 252, "ru-RU");
                                }
                                if (actualphraseEN.Count > 0)
                                {
                                    int rindex = random.Next(0, actualphraseEN.Count);
                                    var MessageEN = actualphraseEN[rindex].Message.Replace("{AuthorMessage}", entbp.AssociateNameEN).Replace("{Quad}", square);
                                    SaveTargetMessage(MessageEN, entbp.AssociateNameEN, "All", entbp.VoiceEN, messcoal, 251, "en-US");
                                }
                                DefSpeech.Add(new DeferrdedSpeech(entbp.AssociateNameRU, entbp.WHID, entbp.Coalition, "All", "11", messcoal, 180));
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
            HandleUnlockKotel(ent, db);
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
        /// Сохранаяет список населенных пунктов, которые в конце миссии нужно удалить из котла. Происходит из-за разблокировки котла.
        /// </summary>
        /// <param name="ent">Населенный пункт, который только что был захвачен</param>
        /// <param name="db">База Данных</param>
        private void HandleUnlockKotel(GraphCity ent, ExpertDB db)
        {
            var allP = db.GraphCity.ToList();
            var array = ent.Targets.Split(',');
            if (array.Length > 0)
            {
                foreach (var item in array)
                {
                    int index = 0;
                    if (int.TryParse(item, out index))
                    {
                        var locent = allP.FirstOrDefault(x => x.IndexCity == index);
                        if (locent != null)
                        {
                            if (locent.Kotel)
                            {
                                var currentKotel = allP.Where(x => x.CompLinks == locent.CompLinks && x.Kotel).ToList();
                                for (int i = 0; i < currentKotel.Count; i++)
                                {
                                    if (!UnlockCauldron.Exists(x => x.IndexCity == currentKotel[i].IndexCity))
                                    {
                                        UnlockCauldron.Add(currentKotel[i]);
                                        GetLogStr("Освобожден из котла: " + currentKotel[i].Name_ru, Color.DarkMagenta);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Разблокирование котла
        /// </summary>
        private void SetUnlockKotel()
        {
            ExpertDB db = new ExpertDB();
            foreach (var item in UnlockCauldron)
            {
                db.GraphCity.First(x => x.IndexCity == item.IndexCity).Kotel = false;
                db.GraphCity.First(x => x.IndexCity == item.IndexCity).PointsKotel = 0;
                db.GraphCity.First(x => x.IndexCity == item.IndexCity).CompLinks = 0;
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
        /// Общий метод снабжения котлов
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
        /// <summary>
        /// Снабжение пилотом точки котла
        /// </summary>
        /// <param name="type6">Событие посадки</param>
        /// <param name="pilot">Пилот</param>
        /// <returns>Возвращает true если снабжение населенного пункта состоялось, false если не состоялось</returns>
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
                    var ent = onlinePlayers.FirstOrDefault(x => x.PlayerId == pilot.LOGIN);
                    if (pilot.Cargo > 0)
                    {
                        if (ent != null)
                        {
                            var messMoment = "-=COMMANDER=- " + pilot.NAME + " Successful landing. Wait for the plane to unload (~1 min). Takeoff is prohibited.";
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, messMoment, ent.Cid);
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
                            RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, messMoment, ent.Cid);
                            RconCommands.Enqueue(sendall);
                        }
                    }
                }
            }
            db.Dispose();
            return false;
        }
        /// <summary>
        /// Загрузка грузов в самолет пилота если тот сел на союзный аэродром (исключение аэродромы в котлах)
        /// </summary>
        /// <param name="type6">Событие посадки</param>
        /// <param name="pilot">Пилот</param>
        /// <returns>Возвращает true если загрузка самолета игрока грузом состоялась, false если не состоялась</returns>
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
                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, messMoment, ent.Cid);
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
                        RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.ClientId, mess, ent.Cid);
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
            SetPhase(2);
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
            SetPhase(1);
            ClearUserDirect();
            SetEndMission(1);
        }
        /// <summary>
        /// Очистка таблицы с пользовательскими направлениями атаки.
        /// </summary>
        private void ClearUserDirect()
        {
            ExpertDB db = new ExpertDB();
            var ud = db.PilotDirect.ToList();
            var votes = db.VoteDirect.ToList();
            foreach (var item in ud)
            {
                db.PilotDirect.Remove(item);
            }
            foreach (var item in votes)
            {
                db.VoteDirect.Remove(item);
            }
            db.SaveChanges();
            db.Dispose();
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
        /// Добавляет в список захваченных населенный пункт который перешел под контроль в связи с положением в котле.
        /// </summary>
        private void CollapsedCauldron()
        {
            ExpertDB db = new ExpertDB();
            var kotels = db.GraphCity.Where(x => x.Kotel && x.PointsKotel <= 1).ToList();
            foreach (var item in kotels)
            {
                var victoryCoal = InvertedCoalition(item.Coalitions);
                victories.Add(new Victory(item, victoryCoal));
                MinusResourceWH(item.Coalitions);
            }
            db.Dispose();
        }
        /// <summary>
        /// Списывает ресурсы со складов определенной коалиции
        /// </summary>
        /// <param name="coal"></param>
        private void MinusResourceWH(int coal)
        {
            ExpertDB db = new ExpertDB();
            var lbp = db.BattlePonts.Where(x => x.Coalition == coal && x.Point >= 36).ToList();
            if (lbp.Count > 0)
            {
                int index = random.Next(0, lbp.Count);
                var ent = lbp[index];
                var whid = ent.WHID;
                var curpoints = ent.Point - 36;
                db.BattlePonts.First(x => x.Coalition == coal && x.WHID == whid).Point = curpoints;
                db.SaveChanges();
            }
            db.Dispose();
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
                SetPhase(3);
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
            else
            {
                StartRConService();
                rcon.OpenSDS(nameFSDS);
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
            if (HandlerLogs.qLog.Count > 0)
            {
                HandlerLogs.qLog.Clear();
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
            if (UnlockCauldron.Count > 0)
            {
                UnlockCauldron.Clear();
            }
            if (Planeset.Count > 0)
            {
                Planeset.Clear();
            }
            if (DefSpeech.Count > 0)
            {
                DefSpeech.Clear();
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
        /// <summary>
        /// Обновление списка пользователей онлайн
        /// </summary>
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
        /// <summary>
        /// Обновления таблицы БД со списком пользователей онлайн
        /// </summary>
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
            var profiles = db.ProfileUser.ToList();
            if (!profiles.Exists(x => x.GameId == player.PlayerId))
            {
                var linkes = db.LinkedAccount.ToList();
                if (!linkes.Exists(x => x.GameID == player.PlayerId))
                {
                    var code = GenerationCode(db);
                    db.LinkedAccount.Add(new LinkedAccount
                    {
                        CheckCode = code,
                        GameID = player.PlayerId,
                        PilotName = player.Name,
                        CreateDate = DateTime.Now
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
            //if (watcher != null)
            //{
            //    watcher.LogEvents -= Watcher_Events;
            //}
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
            return firstquad + "-" + secondquad;
        }
        /// <summary>
        /// Обработка пользовательских направлений атаки. И следом сразу запуск основной генерации миссии.
        /// </summary>
        public void HandleUserDirect()
        {
            SelectUserDirect(101);
            SelectUserDirect(201);
            RemoveVotes();
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
        /// <summary>
        /// Очистка всех голосов за направление атаки перед фазой планирования
        /// </summary>
        private void RemoveVotes()
        {
            ExpertDB db = new ExpertDB();
            var votes = db.VoteDirect.ToList();
            foreach (var item in votes)
            {
                db.VoteDirect.Remove(item);
            }
            db.SaveChanges();
            db.Dispose();
        }
        /// <summary>
        /// Возвращает список войсов в зависимости от языка 101 - русский язык, 201 - английский язык
        /// </summary>
        /// <returns>Возвращает список войсов для русского языка</returns>
        private List<string> GetNameDispatchers(int coal)
        {
            var lname = new List<string>();
            if (coal == 101)
            {
                lname.Add("oksana");
                lname.Add("jane");
                lname.Add("omazh");
                lname.Add("zahar");
                lname.Add("ermil");
                lname.Add("oksana");
                lname.Add("jane");
                lname.Add("omazh");
                lname.Add("zahar");
                lname.Add("ermil");
                lname.Add("oksana");
                lname.Add("jane");
                lname.Add("omazh");
                lname.Add("zahar");
                lname.Add("ermil");
                lname.Add("oksana");
                lname.Add("jane");
                lname.Add("omazh");
                lname.Add("zahar");
                lname.Add("ermil");
            }
            if (coal == 201)
            {
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
                lname.Add("alyss");
                lname.Add("nick");
            }
            return lname;
        }
    }
    /// <summary>
    /// Типы колонн техники
    /// </summary>
    enum TypeColumn
    {
        /// <summary>
        /// Танковая колонна
        /// </summary>
        Armour = 1,
        /// <summary>
        /// Колонна бронированной и не бронированной техники
        /// </summary>
        Mixed = 2,
        /// <summary>
        /// Колонна не бронированной техники
        /// </summary>
        Transport = 3
    }
    /// <summary>
    /// Эмоциональные окраски голосовых сообщений https://cloud.yandex.ru/docs/speechkit/tts/request
    /// </summary>
    enum EmotionSRS
    {
        neutral = 0,
        good = 1,
        evil = 2
    }
}
