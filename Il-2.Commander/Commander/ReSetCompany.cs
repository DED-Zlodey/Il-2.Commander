using Il_2.Commander.Data;
using System.Collections.Generic;
using System.Linq;

namespace Il_2.Commander.Commander
{
    class ReSetCompany
    {
        public void Start()
        {
            List<GraphCity> graphs = new List<GraphCity>();
            ExpertDB db = new ExpertDB();
            var curgraph = db.GraphCity.ToList();
            var clear = db.TempGraphCity.ToList();
            var setup = db.PreSetupMap.ToList();
            var profileUser = db.ProfileUser.Where(x => x.Coalition > 0).ToList();
            for (int i = 0; i < clear.Count; i++)
            {
                var indexcity = clear[i].IndexCity;
                var coal = clear[i].Coalitions;
                var fildlist = db.AirFields.Where(x => x.IndexCity == indexcity).ToList();
                foreach (var item in fildlist)
                {
                    db.AirFields.First(x => x.id == item.id).Coalitions = coal;
                }
                db.GraphCity.First(x => x.IndexCity == indexcity).Coalitions = coal;
                db.GraphCity.First(x => x.IndexCity == indexcity).Kotel = false;
                db.GraphCity.First(x => x.IndexCity == indexcity).PointsKotel = 0;
            }
            foreach (var item in setup)
            {
                db.PreSetupMap.First(x => x.id == item.id).Played = false;
            }
            foreach(var item in profileUser)
            {
                db.ProfileUser.First(x => x.GameId == item.GameId).Coalition = 0;
            }
            db.SaveChanges();
            db.Dispose();
        }
    }
}
