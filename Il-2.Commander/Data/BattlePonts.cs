using System;

namespace Il_2.Commander.Data
{
    class BattlePonts : IComparable<BattlePonts>
    {
        public int id { get; set; }
        public int WHID { get; set; }
        public int Point { get; set; }
        public int Coalition { get; set; }
        public string AssociateNameRU { get; set; }
        public string AssociateNameEN { get; set; }
        public string VoiceRU { get; set; }
        public string VoiceEN { get; set; }
        public int CompareTo(BattlePonts other) //сортировка по по полю Point
        {
            if (other == null)
                return 1;

            else
                return this.Point.CompareTo(other.Point);
        }
    }
}
