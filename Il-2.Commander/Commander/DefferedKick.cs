using Il_2.Commander.Parser;
using System;

namespace Il_2.Commander.Commander
{
    class DefferedKick
    {
        public string GameId { get; set; }
        public string  PilotName { get; set; }
        public DateTime CreateDate { get; set; }
        public TimeSpan TimeOut { get; set; }

        public DefferedKick(AType10 pilot, TimeSpan timeout)
        {
            GameId = pilot.LOGIN;
            PilotName = pilot.NAME;
            CreateDate = DateTime.Now;
            TimeOut = timeout;
        }
    }
}
