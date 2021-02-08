using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class ServiceArea
    {
        [Key]
        public int id { get; set; }
        public string NameFieldEn { get; set; }
        public string NameFieldRu { get; set; }
        public int IndexField { get; set; }
        public double XPos { get; set; }
        public double YPos { get; set; }
        public double ZPos { get; set; }
        public double MaintenanceRadius { get; set; }
        public int Coalition { get; set; }
    }
}
