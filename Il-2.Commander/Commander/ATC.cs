using Il_2.Commander.Data;
using System.Linq;

namespace Il_2.Commander.Commander
{
    class ATC
    {
        /// <summary>
        /// Имя говорящего отображаемое в оверлее SRS
        /// </summary>
        public string WhosTalking { get; private set; }
        /// <summary>
        /// Голос синтеза речи https://cloud.yandex.ru/docs/speechkit/tts/voices
        /// </summary>
        public string VoiceName { get; private set; }
        /// <summary>
        /// Язык
        /// </summary>
        public string Lang { get; private set; }
        /// <summary>
        /// Координата X
        /// </summary>
        public double XPos { get; private set; }
        /// <summary>
        /// Координата Z
        /// </summary>
        public double ZPos { get; private set; }

        public ATC()
        {

        }
        public ATC(RearFields rear, string voice, string lang)
        {
            ExpertDB db = new ExpertDB();
            var allfields = db.AirFields.ToList();
            VoiceName = voice;
            Lang = lang;
            var ent = allfields.FirstOrDefault(x => x.IndexCity == rear.IndexFiled && x.XPos == rear.XPos && x.ZPos == rear.ZPos);
            if (lang.Equals("ru-RU"))
            {
                if(ent != null)
                {
                    WhosTalking = ent.NameRu.Replace(" ", "-");
                }
                else
                {
                    WhosTalking = "Dispatcher";
                }
            }
            if (lang.Equals("en-US"))
            {
                if (ent != null)
                {
                    WhosTalking = ent.NameEn.Replace(" ", "-");
                }
                else
                {
                    WhosTalking = "Dispatcher";
                }
            }
            if(string.IsNullOrEmpty(WhosTalking))
            {
                WhosTalking = "Dispatcher";
            }
            XPos = rear.XPos;
            ZPos = rear.ZPos;
            db.Dispose();
        }

    }
}
