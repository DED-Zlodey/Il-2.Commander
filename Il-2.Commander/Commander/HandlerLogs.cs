using Il_2.Commander.Data;
using Il_2.Commander.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Il_2.Commander.Commander
{
    public delegate void EventFirstLog(int e);
    class HandlerLogs
    {
        public event EventLog GetLogStr;
        public event EventFirstLog EvLog;
        private Watcher watcher;
        /// <summary>
        /// Очередь из лог-файлов
        /// </summary>
        public static Queue<List<string>> qLog = new Queue<List<string>>();
        /// <summary>
        /// крайний лог-файл
        /// </summary>
        private string LastFile { get; set; }

        #region Регулярки
        private static Regex reg_brackets = new Regex(@"(?<={).*?(?=})");
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        #endregion

        public void Start()
        {
            if (watcher == null)
            {
                Action startwatcher = () =>
                {
                    StartWatcher();
                };
                Task taskstartgen = Task.Factory.StartNew(startwatcher);
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

        private void Watcher_Events(string pathLog)
        {
            if (pathLog != LastFile)
            {
                LastFile = pathLog;
                if (pathLog.Contains("[0].txt"))
                {
                    HandlingFirstLog(pathLog);
                    EvLog(1);
                }
            }
        }
        private void HandlingFirstLog(string path)
        {
            var str = SetApp.GetFile(path);
            for (int i = 0; i < str.Count; i++)
            {
                if (str[i].Contains("AType:9 "))
                {
                    ReWriteAType9(str, i, str[i], path);
                    break;
                }
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        private void ReWriteAType9(List<string> str, int i, string strA, string path)
        {
            List<string> fakefields = new List<string>();
            ExpertDB db = new ExpertDB();
            var tick = reg_tick.Match(strA).Value;
            var rearFields = db.RearFields.ToList();
            var allfields = db.AirFields.Where(x => x.idMap == 2).ToList();
            List<AirFields> fields = new List<AirFields>();
            foreach (var item in allfields)
            {
                if (!rearFields.Exists(x => x.IndexFiled == item.IndexCity))
                {
                    fields.Add(item);
                }
            }
            int counter = 1;
            foreach (var item in fields)
            {
                fakefields.Add("T:" + tick + " AType:9 AID:" + counter + " COUNTRY:" + item.Coalitions + " POS(" + item.XPos.ToString().Replace(",", ".") + ", " +
                    item.YPos.ToString().Replace(",", ".") + ", " + item.ZPos.ToString().Replace(",", ".") + ") IDS()");
                counter++;
            }
            str.InsertRange(i, fakefields);
            db.Dispose();
            FileInfo fi = new FileInfo(path);
            var npath = SetApp.Config.DirStatLogs + fi.Name;
            File.WriteAllLines(npath, str);
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
                if (Form1.TriggerTime)
                {
                    qLog.Enqueue(str);
                }
                else
                {
                    SetVictoryLog(str, pathLog);
                }
            }
            //var currentdt = DateTime.Now;
            //var curmissend = currentdt - messDurTime;
            //var ts = currentdt - dt;
            //if (curmissend.TotalMinutes >= durmess)
            //{
            //    messDurTime = DateTime.Now;
            //    var ostatok = Math.Round(DurationMission - ts.TotalMinutes, 0);
            //    if (ostatok < 0)
            //    {
            //        ostatok = 0;
            //    }
            //    var mess = "-=COMMANDER=- END of the mission: " + ostatok + " min.";
            //    RconCommand sendall = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 0);
            //    RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
            //    RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
            //    RconCommands.Enqueue(sendall);
            //    RconCommands.Enqueue(sendred);
            //    RconCommands.Enqueue(sendblue);
            //    GetLogStr(mess, Color.Black);
            //}
            //if (ts.TotalMinutes >= DurationMission)
            //{
            //    if (TriggerTime)
            //    {
            //        TriggerTime = false;
            //        var mess = "-=COMMANDER=- MISSION END.";
            //        RconCommand sendred = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 1);
            //        RconCommand sendblue = new RconCommand(Rcontype.ChatMsg, RoomType.Coalition, mess, 2);
            //        GetLogStr(mess, Color.Red);
            //        RconCommands.Enqueue(sendred);
            //        RconCommands.Enqueue(sendblue);
            //        CreateVictoryCoalition();
            //        StartGeneration("pregen");
            //    }
            //}
            FileInfo fi = new FileInfo(pathLog);
            File.Move(pathLog, SetApp.Config.DirStatLogs + fi.Name);
            //UpdateCurrentPlayers();
            //if (TriggerTime)
            //{
            //    StartColumn(101);
            //    StartColumn(201);
            //}
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
    }
}
