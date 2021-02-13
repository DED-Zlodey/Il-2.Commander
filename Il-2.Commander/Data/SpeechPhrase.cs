using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class SpeechPhrase
    {
        [Key]
        public int id { get; set; }
        public string Message { get; set; }
        public string Lang { get; set; }
        public int Group { get; set; }
    }
}
