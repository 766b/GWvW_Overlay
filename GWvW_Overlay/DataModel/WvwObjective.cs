using System.ComponentModel;
using System.Runtime.CompilerServices;
using GWvW_Overlay.Properties;

namespace GWvW_Overlay.DataModel
{
    //Match Details
    public class WvwObjective : INotifyPropertyChanged
    {
        private double _left;
        private double _left_base;
        private double _top;
        private double _top_base;

        public int id { get; set; }

        public string map { get; set; }
        public int points { get; set; }
        public string type { get; set; }
        public double res_width { get; set; }
        public double res_height { get; set; }

        public string name_de { get; set; }
        public string name_en { get; set; }
        public string name_es { get; set; }
        public string name_fr { get; set; }

        public string name
        {
            get
            {
                switch (Settings.Default["show_names_lang"].ToString())
                {
                    case "English":
                        return name_en;
                    case "German":
                        return name_de;
                    case "Spanish":
                        return name_es;
                    case "French":
                        return name_fr;
                    default:
                        return name_en;
                }
            }
        }

        public double top_base
        {
            set { _top_base = value; }
            get { return _top_base; }
        }

        public double left_base
        {
            set { _left_base = value; }
            get { return _left_base; }
        }

        public double top
        {
            get { return _top; }
            set
            {
                if (_top_base == 0.0)
                    _top_base = value;

                if (value != _top)
                {
                    _top = value;
                    OnPropertyChanged();
                }
            }
        }

        public double left
        {
            get { return _left; }
            set
            {
                if (value != _left)
                {
                    if (_left_base == 0.0)
                        _left_base = value;

                    _left = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "none passed")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Matches

    // World Names
}
