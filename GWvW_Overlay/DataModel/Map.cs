
using System.Collections.Generic;
using System.Linq;

namespace GWvW_Overlay.DataModel
{
    public class Map
    {
        public static Dictionary<int, string> ColorId = new Dictionary<int, string>
        {
            {96, "BlueHome"},
            {94, "RedHome"},
            {95, "GreenHome"},
            {38, "Center"},
        };


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

        public static bool KnownMap(int id)
        {
            return ColorId.Keys.Contains(id);
        }

    }
}
