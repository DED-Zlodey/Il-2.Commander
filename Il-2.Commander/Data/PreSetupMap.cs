namespace Il_2.Commander.Data
{
    class PreSetupMap
    {
        public int id { get; set; }
        public int idServ { get; set; }
        public int idTour { get; set; }
        public int idMap { get; set; }
        public string GameDate { get; set; }
        public double TempMin { get; set; }
        public double TempMax { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public int CloudMin { get; set; }
        public int CloudMax { get; set; }
        public int PrecType { get; set; }
        public int PrecLevel { get; set; }
        public double PrecMM { get; set; }
        public string SeasonPrefix { get; set; }
        public string GuiMap { get; set; }
        public int PressureMin { get; set; }
        public int PressureMax { get; set; }
        public string NameMiss { get; set; }
        public bool Played { get; set; }
    }
}
