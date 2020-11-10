using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class VoteDirect
    {
        [Key]
        public string userid { get; set; }
        public string UName { get; set; }
        public string VoteName { get; set; }
    }
}
