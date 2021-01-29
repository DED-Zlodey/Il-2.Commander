using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType20
    {
        public int TICK { get; private set; }
        public string USERID { get; set; }
        public string USERNICKID { get; set; }

        #region Регулярки
        private static Regex reg_tick = new Regex(@"(?<=T:).*?(?= AType:)");
        private static Regex reg_userid = new Regex(@"(?<=USERID:).*?(?= USERNICKID:)");
        private static Regex reg_usernickid = new Regex(@"(?<=USERNICKID:).*?(?=$)");
        #endregion

        public AType20(string str)
        {
            TICK = int.Parse(reg_tick.Match(str).Value);
            USERID = reg_userid.Match(str).Value;
            USERNICKID = reg_usernickid.Match(str).Value;
        }
    }
}
