using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class PlanesOrders
    {
        [Key]
        public int id { get; set; }
        public int PlaneSetId { get; set; }
        public int FreqSupply { get; set; }
        public string Name { get; set; }
        public DateTime DateDeath { get; set; }
        public int Coalition { get; set; }
        public int Number { get; set; }
    }
}
