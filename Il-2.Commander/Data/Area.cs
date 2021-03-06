using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class Area
    {
        [Key]
        public int id { get; set; }
        public int IndexArea { get; set; }
        public int Coalition { get; set; }
    }
}
