using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Il_2.Commander
{
    public static class SetApp
    {
        public static ConfigJson Config { get; private set; }

        /// <summary>
        /// Считывает из файла настроек начальные параметры, необходимые для работы программы
        /// </summary>
        public static void SetUp()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            Config = JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        /// <summary>
        /// Заменяет разделители в строке, в зависимости от настроек окружения
        /// </summary>
        /// <param name="value">Принимает строку</param>
        /// <returns>Возвращает строку с замененными делителями</returns>
        public static string ReplaceSeparator(string value)
        {
            string dec_sep = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            return value.Replace(",", dec_sep).Replace(".", dec_sep);
        }

        /// <summary>
        /// Читает файл и возвращает списком
        /// </summary>
        /// <param name="path">Принимает путь до файла</param>
        /// <returns>Возвращает коллекцию строк из файла, если файл существует. Если файла нет, вернет пустую коллекцию строк</returns>
        public static List<string> GetFile(string path)
        {
            List<string> str = new List<string>();
            if (File.Exists(path))
            {
                StreamReader read = new StreamReader(path, Encoding.Default);
                while (true)
                {
                    string s = read.ReadLine();
                    if (s != null)
                    {
                        str.Add(s);
                    }
                    else
                        break;
                }
                read.Close();
            }
            return str;
        }
    }
}
