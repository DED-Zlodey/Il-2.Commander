namespace Il_2.Commander.Data
{
    class ColInput
    {
        public int id { get; set; }
        public string NameCol { get; set; }
        public int IndexPoint { get; set; }
        public int SubIndex { get; set; }
        public int Coalition { get; set; }
        public int TypeCol { get; set; }
        public double XPos { get; set; }
        public double YPos { get; set; }
        public double ZPos { get; set; }
        public int Unit { get; set; }
        public double Speed { get; set; }
        public double DistWay { get; set; }
        public int Bridges { get; set; }
        public double TravelTime { get; set; }
        public int ArrivalCol { get; set; }
        public int ArrivalUnit { get; set; }
        public int DestroyedUnits { get; set; }
        public bool Active { get; set; }
        public bool Permit { get; set; }
        public int NWH { get; set; }
    }
}
