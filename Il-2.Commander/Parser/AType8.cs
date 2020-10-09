using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType8
    {
        public int TICK { get; private set; }
        public int OBJID { get; private set; }
        public double XPos { get; private set; }
        public double YPos { get; private set; }
        public double ZPos { get; private set; }
        public int COAL { get; private set; }
        public int TYPE { get; private set; }
        public int RES { get; private set; }
        public int ICTYPE { get; private set; }

        #region Regulars
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_objid = new Regex(@"(?<=OBJID:).*?(?= POS)");
        private static Regex reg_coal = new Regex(@"(?<=COAL:).*?(?= TYPE)");
        private static Regex reg_type = new Regex(@"(?<=TYPE:).*?(?= RES)");
        private static Regex reg_res = new Regex(@"(?<=RES:).*?(?= ICTYPE)");
        private static Regex reg_ictype = new Regex(@"(?<=ICTYPE:).*?(?=$)");
        private static Regex reg_coord = new Regex(@"(?<={).*?(?=})");
        #endregion

        /// <summary>
        /// Обрабатывает AType:8. Хранит всю информацию о событии AType8
        /// </summary>
        /// <param name="str">Принимает строку, в которой содержится информация о событии AType:8</param>
        public AType8(string str)
        {
            str = str.Replace('(', '{');
            str = str.Replace(')', '}');
            TICK = int.Parse(reg_tick.Match(str).Value);
            COAL = int.Parse(reg_coal.Match(str).Value);
            OBJID = int.Parse(reg_objid.Match(str).Value);
            TYPE = int.Parse(reg_type.Match(str).Value);
            RES = int.Parse(reg_res.Match(str).Value);
            ICTYPE = int.Parse(reg_ictype.Match(str).Value);
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
