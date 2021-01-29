using Il_2.Commander.Parser;

namespace Il_2.Commander.Commander
{
    class RconCommand
    {
        public Rcontype Type { get; private set; }
        public string Command { get; private set; }
        public RoomType TypeRoom { get; private set; }
        public int RecipientId { get; private set; }
        public AType10 aType { get; private set; }
        public AType20 Bans { get; private set; }

        public RconCommand(Rcontype rtype, RoomType roomType, string mess, int recip)
        {
            Type = rtype;
            Command = mess;
            TypeRoom = roomType;
            RecipientId = recip;
        }
        public RconCommand(Rcontype rtype, string mess)
        {
            Type = rtype;
            Command = mess;
            TypeRoom = RoomType.All;
            RecipientId = 0;
        }
        public RconCommand(Rcontype rtype, AType10 atype)
        {
            Type = rtype;
            aType = atype;
        }
        public RconCommand(Rcontype rtype, AType20 aType)
        {
            Type = rtype;
            Bans = aType;
        }
        public RconCommand(Rcontype rtype)
        {
            Type = rtype;
        }
    }
    enum Rcontype
    {
        Input = 1,
        ChatMsg = 2,
        Players = 3,
        ReSetSPS = 4,
        CheckBans = 5
    }
}
