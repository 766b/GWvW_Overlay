using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Logitech_LCD;
using Logitech_LCD.Applets;

namespace GWvW_Overlay
{
    public partial class ColorDisplayApplet : BaseApplet
    {
        int i = 0;
        public ColorDisplayApplet()
        {
            InitializeComponent();
        }

        public ColorDisplayApplet(LcdType lcdType)
            : base(lcdType)
        {
            InitializeComponent();
        }

        protected override void OnDataUpdate(object sender, EventArgs e)
        {
            i++;
            this.label1.Text = i.ToString();
        }
    }
}
