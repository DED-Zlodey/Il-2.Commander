namespace Il_2.Commander.Data
{
    class CompTarget
    {
        public int id { get; set; }
        /// <summary>
        /// Название цели
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Строковый индекс конкретного объекта
        /// </summary>
        public string EntName { get; set; }
        /// <summary>
        /// Внутриигровой скрипт модели объекта
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// Идентификатор цели
        /// </summary>
        public int GroupInput { get; set; }
        /// <summary>
        /// Общее кол-во уничтоженных объектов для выключения цели
        /// </summary>
        public int TotalWeigth { get; set; }
        /// <summary>
        /// Кол-во "жизней" в объекте.
        /// </summary>
        public int InernalWeight { get; set; }
        /// <summary>
        /// Номер точки к которой приписана цель и соответственно объект
        /// </summary>
        public int IndexPoint { get; set; }
        /// <summary>
        /// Субиндекс цели
        /// </summary>
        public int SubIndex { get; set; }
        /// <summary>
        /// Координата X
        /// </summary>
        public double XPos { get; set; }
        /// <summary>
        /// Координата Y (высота над уровнем моря)
        /// </summary>
        public double YPos { get; set; }
        /// <summary>
        /// Координата Z
        /// </summary>
        public double ZPos { get; set; }
        /// <summary>
        /// Вкл/выкл цели
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// Кол-во уничтоженных объектов внутри объекта
        /// </summary>
        public int Destroed { get; set; }
        /// <summary>
        /// Обязательно или не обязательно уничтожать объект для выключения цели
        /// </summary>
        public bool Mandatory { get; set; }
    }
}
