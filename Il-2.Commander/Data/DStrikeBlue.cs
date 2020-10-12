using System;

namespace Il_2.Commander.Data
{
    class DStrikeBlue : IComparable<DStrikeBlue>
    {
        public int id { get; set; }
        public int SerialNumber { get; set; }
        public int IndexPoint { get; set; }
        public int NextPoint { get; set; }
        public bool FrontLinePoint { get; set; }
        public int Enable { get; set; }
        public int CompareTo(DStrikeBlue other) //сортировка по по полю SerialNumber
        {
            if (other == null)
                return 1;

            else
                return this.SerialNumber.CompareTo(other.SerialNumber);
        }
    }
}
