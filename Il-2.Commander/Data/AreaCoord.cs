using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class AreaCoord
    {
        [Key]
        public int id { get; set; }
        public int IndexArea { get; set; }
        public int NumPoint { get; set; }
        public double XPos { get; set; }
        public double ZPos { get; set; }
    }
}
