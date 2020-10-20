using Il_2.Commander.Parser;
using System;

namespace Il_2.Commander.Commander
{
    class HandlingAirSupply
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
        /// Тип операции. Загрузка самолета или его разгрузка.
        /// </summary>
        public TypeSupply TypeSupply { get; private set; }
        /// <summary>
        /// Задержка в секундах.
        /// </summary>
        public int Duration { get; private set; }
        /// <summary>
        /// Номер населенного пункта, которому нужно пополнить очки живучести
        /// </summary>
        public int IndexCity { get; private set; }
        /// <summary>
        /// Пилот, который выполняет миссию снабжения
        /// </summary>
        public AType10 Pilot { get; private set; }
        public HandlingAirSupply(string command, DateTime dateTime, TypeSupply typeSupply, int duration, int indexCity, AType10 pilot)
        {
            Command = command;
            CreateTime = dateTime;
            TypeSupply = typeSupply;
            Duration = duration;
            IndexCity = indexCity;
            Pilot = pilot;
        }
    }
    enum TypeSupply
    {
        ForPilot = 1,
        ForCity = 2
    }
}
