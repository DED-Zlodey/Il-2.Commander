using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class PhaseGen
    {
        [Key]
        public int id { get; set; }
        public int NPhase { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
