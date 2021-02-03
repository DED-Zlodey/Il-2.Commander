using System.Text.RegularExpressions;

namespace Il_2.Commander.Parser
{
    class AType0
    {
        public string MFile { get; set; }
        #region Regulars
        private static Regex reg_mfile = new Regex(@"(?<=MFile:).*?(?= MID:)");
        #endregion

        public AType0(string str)
        {
            MFile = reg_mfile.Match(str).Value;
        }
    }
}
