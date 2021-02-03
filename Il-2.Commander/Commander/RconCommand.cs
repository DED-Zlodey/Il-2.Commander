using Il_2.Commander.Parser;

namespace Il_2.Commander.Commander
{
    /// <summary>
    /// Класс ркон команд
    /// </summary>
    class RconCommand
    {
        /// <summary>
        /// Тип ркон команды. Используется для определения какой метод выполнять.
        /// </summary>
        public Rcontype Type { get; private set; }
        /// <summary>
        /// РКон команда в текстовом формате
        /// </summary>
        public string Command { get; private set; }
        /// <summary>
        /// Тип комнаты в которую отсылается сообщение, если таковое необходимо отправить с помощью ркон
        /// </summary>
        public RoomType TypeRoom { get; private set; }
        /// <summary>
        /// cid пилота которому необлходимо отправить приватное сообщение.
        /// </summary>
        public int RecipientId { get; private set; }
        /// <summary>
        /// Объект AType10 с данными пилота
        /// </summary>
        public AType10 aType { get; private set; }
        /// <summary>
        /// Данные игрока, которого необходмо выкинуть с сервера
        /// </summary>
        public AType20 Bans { get; private set; }
        /// <summary>
        /// Данные пилота совершившего взлет.
        /// </summary>
        public AType5 TakeOffForbiden { get; private set; }
        /// <summary>
        /// Конструктор для отправки приватного сообщения пилоту.
        /// </summary>
        /// <param name="rtype"></param>
        /// <param name="roomType"></param>
        /// <param name="mess"></param>
        /// <param name="recip"></param>
        public RconCommand(Rcontype rtype, RoomType roomType, string mess, int recip)
        {
            Type = rtype;
            Command = mess;
            TypeRoom = roomType;
            RecipientId = recip;
        }
        /// <summary>
        /// Конструктор для отправки сообщения всем пилотам на сервере
        /// </summary>
        /// <param name="rtype"></param>
        /// <param name="mess"></param>
        public RconCommand(Rcontype rtype, string mess)
        {
            Type = rtype;
            Command = mess;
            TypeRoom = RoomType.All;
            RecipientId = 0;
        }
        /// <summary>
        /// Конструктор для приветствия пилота с данными из AType10 (Rcontype.Players). Для выкидывания пилота (Rcontype.Kick). Проверки регистрации (Rcontype.CheckRegistration).
        /// </summary>
        /// <param name="rtype"></param>
        /// <param name="atype"></param>
        public RconCommand(Rcontype rtype, AType10 atype)
        {
            Type = rtype;
            aType = atype;
        }
        /// <summary>
        /// конструктор для проверки пилота не находится ли тот в бан-листе. Создается при каждом входе пользователя на сервер.
        /// </summary>
        /// <param name="rtype"></param>
        /// <param name="aType"></param>
        public RconCommand(Rcontype rtype, AType20 aType)
        {
            Type = rtype;
            Bans = aType;
        }
    }
    /// <summary>
    /// Типы ркон команд
    /// </summary>
    enum Rcontype
    {
        Input = 1,
        ChatMsg = 2,
        Players = 3,
        ReSetSPS = 4,
        CheckBans = 5,
        CheckRegistration = 6,
        Kick = 7
    }
}
