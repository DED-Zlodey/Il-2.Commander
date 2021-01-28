using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class PlaneSet
    {
        [Key]
        public int id { get; set; }
        public int Map { get; set; }
        public int NumField { get; set; }
        public int FreqSupply { get; set; }
        public string Name { get; set; }
        public string LogType { get; set; }
        public DateTime MinGameDate { get; set; }
        public DateTime MaxGameDate { get; set; }
        public int Coalition { get; set; }
        public string Model { get; set; }
        public string Script { get; set; }
        public int Number { get; set; }
        public string AvPayloads { get; set; }
        public string AvMods { get; set; }
        public string AvSkins { get; set; }
        public bool Enable { get; set; }
    }
}
