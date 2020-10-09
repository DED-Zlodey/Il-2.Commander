using System;

namespace Il_2.Commander.Commander
{
    class Player
    {
        public int Cid { get; private set; }
        public string Name { get; private set; }
        public string ProfileId { get; private set; }
        public string PlayerId { get; private set; }
        public string IngameStatus { get; set; }
        public int Ping { get; private set; }
        public int Coalition { get; set; }

        public Player(int cid, string ingameStatus, int ping, string unescapedName, string unescapedPlayerId, string unescapedProfileId)
        {
            Cid = cid;
            ProfileId = Uri.UnescapeDataString(unescapedProfileId);
            PlayerId = Uri.UnescapeDataString(unescapedPlayerId);
            IngameStatus = ingameStatus;
            Ping = ping;
            Name = Uri.UnescapeDataString(unescapedName);
        }
    }
}
