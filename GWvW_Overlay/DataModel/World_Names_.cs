namespace GWvW_Overlay.DataModel
{
    public class World_Names_
    {
        private string _name;

        public int id { get; set; }

        public string name
        {
            get { return _name; }
            set { _name = value ?? "Server ID " + id; }
        }
    }
}
