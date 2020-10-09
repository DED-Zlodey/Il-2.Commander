using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType16
    {
        public int TICK { get; private set; }
        public int BOTID { get; private set; }
        public double XPos { get; private set; }
        public double YPos { get; private set; }
        public double ZPos { get; private set; }

        #region Regulars
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_botid = new Regex(@"(?<=BOTID:).*?(?= POS)");
        private static Regex reg_coord = new Regex(@"(?<={).*?(?=})");
        #endregion

        /// <summary>
        /// Обрабатывает AType:16. Хранит всю информацию о событии AType 16. Событие утилизации бота.
        /// </summary>
        /// <param name="str">Принимает строку, в которой содержится информация о событии AType:16</param>
        public AType16(string str)
        {
            str = str.Replace('(', '{');
            str = str.Replace(')', '}');
            TICK = int.Parse(reg_tick.Match(str).Value);
            BOTID = int.Parse(reg_botid.Match(str).Value);
            var strcoord = reg_coord.Match(str).Value.Split(new char[] { ',' });
            if (strcoord.Length > 1)
            {
                XPos = double.Parse(SetApp.ReplaceSeparator(strcoord[0]));
                YPos = double.Parse(SetApp.ReplaceSeparator(strcoord[1]));
                ZPos = double.Parse(SetApp.ReplaceSeparator(strcoord[2]));
            }
            else
            {
                XPos = 0;
                YPos = 0;
                ZPos = 0;
            }
        }
    }
}
