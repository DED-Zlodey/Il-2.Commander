using System;
using System.IO;
using System.Threading;

namespace Il_2.Commander.Commander
{
    public delegate void delShowLog(string pathLog);
    class Watcher
    {
        /// <summary>
        /// Событие об изменении в файловой системе. Возникает, когда в папку логов записывается новый лог-файл
        /// </summary>
        public event delShowLog LogEvents;
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private string FName = string.Empty;

        /// <summary>
        /// Отслеживает появление нового лог-файла
        /// </summary>
        /// <param name="path">Принимает путь до папки в которую DSerever складывает лог-файлы</param>
        public Watcher(string path)
        {
            watcher.Path = path;
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.Filter = @"*.txt";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.EnableRaisingEvents = true;
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != FName)
            {
                FName = e.FullPath;
                Thread.Sleep(1500);
                MakeEvents(e.FullPath);
            }
        }
        private void watcher_Changed(Object sender, FileSystemEventArgs e)
        {
            if(e.FullPath != FName)
            {
                FName = e.FullPath;
                Thread.Sleep(1500);
                MakeEvents(e.FullPath);
            }
        }
        private void MakeEvents(string FileName)
        {
            FileInfo fi = new FileInfo(FileName);
            LogEvents?.Invoke(string.Format(fi.FullName));
        }
    }
}