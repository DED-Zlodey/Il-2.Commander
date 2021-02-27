using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Il_2.Commander.Data
{
    class DurationBFL
    {
        [Key]
        public int id { get; set; }
        public int HoursBan { get; set; }
        public int MinutesBan { get; set; }
    }
}
