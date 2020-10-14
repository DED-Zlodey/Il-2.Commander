using System;

namespace Il_2.Commander.Commander
{
    class DeferredCommand
    {
        /// <summary>
        /// РКон команда
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// Дата и время создания команды
        /// </summary>
        public DateTime CreateTime { get; private set; }
        /// <summary>
        /// Задержка в минутах.
        /// </summary>
        public int Duration { get; private set; }
        /// <summary>
        /// Номер комнаты коалиции для отправки сообщения
        /// </summary>
        public int Coalition { get; private set; }

        public DeferredCommand(string command, int duration, int coalition)
        {
            Command = command;
            Duration = duration;
            CreateTime = DateTime.Now;
            Coalition = coalition;
        }
    }
}
