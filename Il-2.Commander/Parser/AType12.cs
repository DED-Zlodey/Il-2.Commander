using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType12
    {
        public int TICK { get; private set; }
        public int ID { get; private set; }
        public string TYPE { get; private set; }
        public int COUNTRY { get; private set; }
        public string NAME { get; private set; }
        public int PID { get; private set; }
        public double XPos { get; private set; }
        public double YPos { get; private set; }
        public double ZPos { get; private set; }
        public bool Destroyed { get; set; }
        public int Unit { get; set; }

        #region Regulars
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_id = new Regex(@"(?<=ID:).*?(?= TYPE)");
        private static Regex reg_type = new Regex(@"(?<=TYPE:).*?(?= COUNTRY)");
        private static Regex reg_country = new Regex(@"(?<=COUNTRY:).*?(?= NAME:)");
        private static Regex reg_name = new Regex(@"(?<=NAME:).*?(?= PID)");
        private static Regex reg_pid = new Regex(@"(?<=PID:).*?(?= POS)");
        private static Regex reg_coord = new Regex(@"(?<={).*?(?=})");
        #endregion

        /// <summary>
        /// Обрабатывает AType:12. Хранит всю информацию о событии AType 12
        /// </summary>
        /// <param name="str">Принимает строку, в которой содержится информация о событии AType:12</param>
        public AType12(string str)
        {
            str = str.Replace('(', '{');
            str = str.Replace(')', '}');
            TICK = int.Parse(reg_tick.Match(str).Value);
            ID = int.Parse(reg_id.Match(str).Value);
            TYPE = reg_type.Match(str).Value;
            COUNTRY = int.Parse(reg_country.Match(str).Value);
            NAME = reg_name.Match(str).Value;
            PID = int.Parse(reg_pid.Match(str).Value);
            var strcoord = reg_coord.Match(str).Value.Split(new char[] { ',' });
            if (strcoord.Length > 1)
            {
                XPos = double.Parse(SetApp.ReplaceSeparator(strcoord[0]));
                YPos = double.Parse(SetApp.ReplaceSeparator(strcoord[1]));
                ZPos = double.Parse(SetApp.ReplaceSeparator(strcoord[2]));
            }
        }
    }
}
