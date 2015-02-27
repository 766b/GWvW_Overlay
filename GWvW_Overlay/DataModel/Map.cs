using System.Collections.Generic;
using System.Linq;

namespace GWvW_Overlay.DataModel
{
    public class Map
    {
        public string Type { get; set; }
        public List<int> Scores { get; set; }
        public List<Objective> Objectives { get; set; }

        public double ScoresSum
        {
            get { return Scores.Sum(); }
        }

        public int CountObjType(string type, string color)
        {
            return
                Objectives.Count(
                    obj => obj.ObjData.type.ToLower() == type.ToLower() && obj.owner.ToLower() == color.ToLower());
        }
    }
}
