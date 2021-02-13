using System;

namespace Il_2.Commander.Commander
{
    class DeferrdedSpeech
    {
        public string Message { get; private set; }
        public string Recipient { get; private set; }
        public string AuthorMessage { get; private set; }
        public int IndexAuthor { get; private set; }
        public int SubIndexAuthor { get; private set; }
        /// <summary>
        /// Дата и время создания спича
        /// </summary>
        public DateTime CreateTime { get; private set; }
        public int Coalition { get; private set; }
        public int DurationInSec { get; private set; }

        public DeferrdedSpeech(string authormessage, int indexauthor, int subindexauthor, string recipient, string message, int coal, int dursec)
        {
            Message = message;
            AuthorMessage = authormessage;
            IndexAuthor = indexauthor;
            SubIndexAuthor = subindexauthor;
            Recipient = recipient;
            Coalition = coal;
            DurationInSec = dursec;
            CreateTime = DateTime.Now;
        }
    }
}
