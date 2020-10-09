using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType3
    {
        public int TICK { get; private set; }
        public int AID { get; private set; }
        public int TID { get; private set; }
        public double XPos { get; private set; }
        public double YPos { get; private set; }
        public double ZPos { get; private set; }

        #region Regulars
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_aid = new Regex(@"(?<=AID:).*?(?= TID)");
        private static Regex reg_tid = new Regex(@"(?<=TID:).*?(?= POS)");
        private static Regex reg_coord = new Regex(@"(?<={).*?(?=})");
        #endregion

        /// <summary>
        /// Обрабатывает AType:3. Хранит всю информацию о событии AType 3. Событие уничтожения какого-либо объекта в миссии, каким-либо объектом в миссии.
        /// </summary>
        /// <param name="str">Принимает строку, в которой содержится информация о событии AType:3</param>
        public AType3(string str)
        {
            str = str.Replace('(', '{');
            str = str.Replace(')', '}');
            TICK = int.Parse(reg_tick.Match(str).Value);
            AID = int.Parse(reg_aid.Match(str).Value);
            TID = int.Parse(reg_tid.Match(str).Value);
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
