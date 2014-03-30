using GWvW_Overlay.Resources.Lang;
using Logitech_LCD;
using Logitech_LCD.Applets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GWvW_Overlay
{
    public partial class ColorDisplayApplet : BaseApplet
    {
        private String _bl;
        private Label[] lines;
        private int currentLine;
        public WvwMatch_ match { get; set; }

        public ColorDisplayApplet(ref WvwMatch_ match) :
            this(LcdType.Color, ref match)
        {

        }


        public ColorDisplayApplet(LcdType lcdType, ref WvwMatch_ match)
            : base(lcdType)
        {
            InitializeComponent();

            lines = new Label[6] {line1,
                                  line2,
                                  line3,
                                  line4,
                                  line5,
                                  line6};

            this.match = match;
            _bl = match.Options.active_bl;
            LcdColorLeftButtonPressed += ColorDisplayApplet_OnLcdColorLeftButtonPressed;
            LcdColorRightButtonPressed += ColorDisplayApplet_OnLcdColorRightButtonPressed;
            buildTabControlTabs();
        }

        void ColorDisplayApplet_OnLcdColorRightButtonPressed(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex < tabs.TabCount - 1)
            {
                tabs.SelectedIndex++;
            }
            else
            {
                tabs.SelectedIndex = 0;
            }
        }

        void ColorDisplayApplet_OnLcdColorLeftButtonPressed(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex > 0)
            {
                tabs.SelectedIndex--;
            }
            else
            {
                tabs.SelectedIndex = tabs.TabCount - 1;
            }
        }



        protected override void OnDataUpdate(object sender, EventArgs e)
        {
            if (_bl != match.Options.active_bl)
            {
                _bl = match.Options.active_bl;
                buildTabControlTabs();
            }
            if (match.Details != null)
            {
                this.BackgroundImage = null;
                tabs.Visible = true;
                currentLine = 0;
                List<Objective> result = new List<Objective>();
                if (this.tabs.SelectedTab.Text == Strings.camps)
                {
                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "camp")
                                                               .Take(6)
                                                               .ToList();
                }
                else if ((this.tabs.SelectedTab.Text == Strings.towers) ||
                         (this.tabs.SelectedTab.Text == (Strings.towers + " 1")))
                {
                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "tower")
                                                               .Take(6)
                                                               .ToList();
                }
                else if (this.tabs.SelectedTab.Text == Strings.towers + " 2")
                {
                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "tower")
                                                               .Take(6)
                                                               .ToList();

                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "tower")
                                                               .Except(result)
                                                               .Take(6)
                                                               .ToList();
                }
                else if (this.tabs.SelectedTab.Text == Strings.keeps)
                {
                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "keep")
                                                               .Take(6)
                                                               .ToList();
                }
                else if (this.tabs.SelectedTab.Text == Strings.castles)
                {
                    result = match.Details.Maps.FirstOrDefault(map => map.Type == _bl).Objectives
                                                               .Where(obj => obj.ObjData.type == "castle")
                                                               .Take(6)
                                                               .ToList();
                }
                result.ForEach(new Action<Objective>(format));
                for (; currentLine < lines.Length; currentLine++)
                {
                    lines[currentLine].Visible = false;
                }
            }
            else
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].Visible = false;
                }
                tabs.Visible = false;

                this.BackgroundImage = Image.FromFile("Resources/mapeb_normal.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }

        }

        private void format(Objective obj)
        {
            TimeSpan diff = DateTime.Now.Subtract(obj.last_change);
            TimeSpan left = TimeSpan.FromMinutes(5) - diff;
            String time = diff < TimeSpan.FromMinutes(5) ? left.ToString(@"mm\:ss") : "N/A";
            lines[currentLine].Text = String.Format("{0} {1}",
                                                     time,
                                                     obj.ObjData.name);
            lines[currentLine].ForeColor = ownerColor(obj.owner);
            lines[currentLine].Visible = true;
            currentLine++;
        }

        private Color ownerColor(String owner)
        {
            switch (owner)
            {
                case "Green":
                    return Color.Green;
                case "Blue":
                    return Color.Blue;
                case "Red":
                    return Color.Red;
                default:
                    return Color.White;
            }
        }

        private void buildTabControlTabs()
        {
            this.tabs.TabPages.Clear();
            this.tabs.TabPages.Add(new TabPage(Strings.camps));
            if (_bl == "Center")
            {
                this.tabs.TabPages.Add(new TabPage(Strings.towers + " 1"));
                this.tabs.TabPages.Add(new TabPage(Strings.towers + " 2"));
            }
            else
            {
                this.tabs.TabPages.Add(new TabPage(Strings.towers));
            }
            this.tabs.TabPages.Add(new TabPage(Strings.keeps));
            if (_bl == "Center")
                this.tabs.TabPages.Add(new TabPage(Strings.castles));
        }

    }
}
