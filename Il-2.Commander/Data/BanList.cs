using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class BanList
    {
        [Key]
        public int id { get; set; }
        public string PlayerId { get; set; }
        public string ProfileId { get; set; }
        public string PilotName { get; set; }
        public DateTime CreateDate { get; set; }
        public int HoursBan { get; set; }
        public int MinuteBan { get; set; }
        public string ReasonBan { get; set; }
    }
}
