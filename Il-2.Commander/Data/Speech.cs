using System;
using System.ComponentModel.DataAnnotations;

namespace Il_2.Commander.Data
{
    class Speech
    {
        [Key]
        public int id { get; set; }
        /// <summary>
        /// Реципиент голосовго сообщения
        /// </summary>
        public string RecipientMessage { get; set; }
        /// <summary>
        /// Имя говорящего отображающееся в списке SRS
        /// </summary>
        public string NameSpeaker { get; set; }
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Язык
        /// </summary>
        public string Lang { get; set; }
        /// <summary>
        /// Коалиция которой будет отправлено голосове сообщение
        /// </summary>
        public int Coalition { get; set; }
        /// <summary>
        /// Частота MHz на которой будет передано сообщение
        /// </summary>
        public double Frequency { get; set; }
        /// <summary>
        /// Голос синтеза речи https://cloud.yandex.ru/docs/speechkit/tts/voices
        /// </summary>
        public string Voice { get; set; }
        /// <summary>
        /// Скорость речи задается дробным числом в диапазоне от 0.1 (min.) до 3.0 (max)
        /// </summary>
        public double Speed { get; set; }
        /// <summary>
        /// Эмоциональная окраска голоса. good, evil, neutral
        /// </summary>
        public string Emotion { get; set; }
        /// <summary>
        /// Дата создания записи в таблице
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
