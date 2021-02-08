using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class ATCDispatcher
    {
        [Key]
        public int id { get; set; }
        public string Lang { get; set; }
        public string Phrase { get; set; }
        public int TypePhrase { get; set; }
    }
}
