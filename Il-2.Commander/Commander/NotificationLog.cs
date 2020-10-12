using System.Drawing;

namespace Il_2.Commander.Commander
{
    class NotificationLog
    {
        public Color Color { get; set; }
        public string Text { get; set; }

        public NotificationLog(Color color, string text)
        {
            Text = text;
            Color = color;
        }
    }
}
