using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class PilotDirect
    {
        [Key]
        public string UserId { get; set; }
        public string PilotName { get; set; }
        public string Direct { get; set; }
        public int Coalition { get; set; }
        public int NVote { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}
