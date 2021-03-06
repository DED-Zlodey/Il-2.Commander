﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Il_2.Commander
{
    public static class SetApp
    {
        /// <summary>
        /// Конфиг коммандера
        /// </summary>
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
        /// <summary>
        /// Возвращает длину отрезка между двумя точками
        /// </summary>
        /// <param name="x1">Координата Х первой точки</param>
        /// <param name="y1">Координата Y первой точки</param>
        /// <param name="x2">Координата Х второй точки</param>
        /// <param name="y2">Координата Y второй точки</param>
        /// <returns></returns>
        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
    }
}
