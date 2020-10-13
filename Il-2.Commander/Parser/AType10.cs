using Il_2.Commander.Commander;
using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType10
    {
        public int TICK { get; private set; }
        public int PLID { get; private set; }
        public int PID { get; private set; }
        public int BUL { get; private set; }
        public int SH { get; private set; }
        public int BOMB { get; private set; }
        public int RCT { get; private set; }
        public double XPos { get; private set; }
        public double YPos { get; private set; }
        public double ZPos { get; private set; }
        public string LOGIN { get; private set; }
        public string IDS { get; private set; }
        public int ICPL { get; private set; }
        public int ISTSTART { get; private set; }
        public string NAME { get; private set; }
        public string TYPE { get; private set; }
        public int COUNTRY { get; private set; }
        public int FORM { get; private set; }
        public int FIELD { get; private set; }
        public int INAIR { get; private set; }
        public int PARENT { get; private set; }
        public int PAYLOAD { get; private set; }
        public double FUEL { get; private set; }
        public string SKIN { get; private set; }
        public int WM { get; private set; }
        public string GameStatus { get; set; }
        public Player Player { get; set; }

        #region Регулярки
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_plid = new Regex(@"(?<=PLID:).*?(?= PID)");
        private static Regex reg_pid = new Regex(@"(?<=PID:).*?(?= BUL)");
        private static Regex reg_bul = new Regex(@"(?<=BUL:).*?(?= SH)");
        private static Regex reg_sh = new Regex(@"(?<=SH:).*?(?= BOMB)");
        private static Regex reg_bomb = new Regex(@"(?<=BOMB:).*?(?=RCT)");
        private static Regex reg_rct = new Regex(@"(?<=RCT:).*?(?= )");
        private static Regex reg_coord = new Regex(@"(?<={).*?(?=})");
        private static Regex reg_login = new Regex(@"(?<=LOGIN:).*?(?= NAME)");
        private static Regex reg_name = new Regex(@"(?<=NAME:).*?(?= TYPE)");
        private static Regex reg_type = new Regex(@"(?<=TYPE:).*?(?= COUNTRY)");
        private static Regex reg_country = new Regex(@"(?<=COUNTRY:).*?(?= FORM)");
        private static Regex reg_form = new Regex(@"(?<=FORM:).*?(?= FIELD)");
        private static Regex reg_field = new Regex(@"(?<=FIELD:).*?(?= INAIR)");
        private static Regex reg_inair = new Regex(@"(?<=INAIR:).*?(?= PARENT)");
        private static Regex reg_parent = new Regex(@"(?<=PARENT:).*?(?= ISPL)");
        private static Regex reg_payload = new Regex(@"(?<=PAYLOAD:).*?(?= FUEL)");
        private static Regex reg_fuel = new Regex(@"(?<=FUEL:).*?(?= SKIN)");
        private static Regex reg_wm = new Regex(@"(?<=WM:).*?(?=$)");
        private static Regex reg_ids = new Regex(@"(?<=IDS:).*?(?= LOGIN)");
        private static Regex reg_icpl = new Regex(@"(?<=ISPL:).*?(?= ISTSTART)");
        private static Regex reg_iststart = new Regex(@"(?<=ISTSTART:).*?(?= PAYLOAD)");
        private static Regex reg_skin = new Regex(@"(?<=SKIN:).*?(?= WM)");
        #endregion

        /// <summary>
        /// Обрабатывает AType:10. Хранит всю информацию о событии AType 10
        /// </summary>
        /// <param name="str">Принимает строку, в которой содержится информация о событии AType:10</param>
        public AType10(string str)
        {
            str = str.Replace('(', '{');
            str = str.Replace(')', '}');
            TICK = int.Parse(reg_tick.Match(str).Value);
            PLID = int.Parse(reg_plid.Match(str).Value);
            PID = int.Parse(reg_pid.Match(str).Value);
            BUL = int.Parse(reg_bul.Match(str).Value);
            SH = int.Parse(reg_sh.Match(str).Value);
            BOMB = int.Parse(reg_bomb.Match(str).Value);
            RCT = int.Parse(reg_rct.Match(str).Value);
            var strcoord = reg_coord.Match(str).Value.Split(new char[] { ',' });
            XPos = double.Parse(SetApp.ReplaceSeparator(strcoord[0]));
            YPos = double.Parse(SetApp.ReplaceSeparator(strcoord[1]));
            ZPos = double.Parse(SetApp.ReplaceSeparator(strcoord[2]));
            IDS = reg_ids.Match(str).Value;
            LOGIN = reg_login.Match(str).Value;
            NAME = reg_name.Match(str).Value;
            TYPE = reg_type.Match(str).Value;
            COUNTRY = int.Parse(reg_country.Match(str).Value);
            FORM = int.Parse(reg_form.Match(str).Value);
            FIELD = int.Parse(reg_form.Match(str).Value);
            INAIR = int.Parse(reg_inair.Match(str).Value);
            PARENT = int.Parse(reg_parent.Match(str).Value);
            ICPL = int.Parse(reg_icpl.Match(str).Value);
            ISTSTART = int.Parse(reg_iststart.Match(str).Value);
            PAYLOAD = int.Parse(reg_payload.Match(str).Value);
            FUEL = double.Parse(SetApp.ReplaceSeparator(reg_fuel.Match(str).Value));
            SKIN = reg_skin.Match(str).Value;
            WM = int.Parse(reg_wm.Match(str).Value);
            GameStatus = GameStatusPilot.Parking.ToString();
        }
    }
    enum GameStatusPilot
    {
        Parking = 1,
        InFlight = 2,
        Spectator = 3
    }
}
