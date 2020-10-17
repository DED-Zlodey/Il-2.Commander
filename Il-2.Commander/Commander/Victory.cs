using Il_2.Commander.Data;

namespace Il_2.Commander.Commander
{
    class Victory
    {
        /// <summary>
        /// Коалиция захватившая населенный пункт
        /// </summary>
        public int Coalition { get; private set; }
        /// <summary>
        /// Название захваченного населенного пункта
        /// </summary>
        public string NameCity { get; private set; }
        /// <summary>
        /// Создает объект с информацией о захвате населенного пункта. 
        /// </summary>
        /// <param name="ent">Объект населенного пункта, который захвачен</param>
        /// <param name="coal">Номер захватывающей коалиция</param>
        public Victory(GraphCity ent, int coal)
        {
            Coalition = coal;
            NameCity = ent.Name_en;
        }
    }
}
