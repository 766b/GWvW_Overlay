using System.Collections.Generic;
using System.Linq;

namespace GWvW_Overlay.DataModel
{
    public class Match_Details_
    {
        public string match_id { get; set; }
        public List<int> Scores { get; set; }
        public List<Map> Maps { get; set; }

        public double ScoresSum
        {
            get { return Scores.Sum(); }
        }
    }
}
