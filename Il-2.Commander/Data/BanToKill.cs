using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class BanToKill
    {
        [Key]
        public int id { get; set; }
        public string GameId { get; set; }
        /// <summary>
        /// Никнейм пилота желающего получать бан за смерть пилота
        /// </summary>
        public string Pilot { get; set; }
        /// <summary>
        /// На сколько часов пилот желает получать бан после своей смерти
        /// </summary>
        public int BanHours { get; set; }
    }
}
