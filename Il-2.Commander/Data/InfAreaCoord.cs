using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class InfAreaCoord : IComparable<InfAreaCoord>
    {
        [Key]
        public int id { get; set; }
        public int IndexArea { get; set; }
        public int NumPoint { get; set; }
        public double XPos { get; set; }
        public double ZPos { get; set; }
        public int CompareTo(InfAreaCoord other) //сортировка по по полю NumPoint
        {
            if (other == null)
                return 1;

            else
                return this.NumPoint.CompareTo(other.NumPoint);
        }
    }
}
