using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Windows.Threading;
//using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace GWvW_Overlay
{

    /// <summary>
    /// Interaction logic for CampLogger.xaml
    /// </summary>
    public partial class CampLogger : Window
    {
        public CampLogger()
        {
            InitializeComponent();

            if ((bool)Properties.Settings.Default["tracker_saved"])
            {
                cnvsPromt.Visibility = Visibility.Hidden;
            }
        }

        public void AddEventLog(Dictionary<string, string> data, bool claim)
        {
            if (claim)
            {
                if (data["owner"] != "released")
                {
                    TextRange owner = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);
                    owner.Text = string.Format(" {0}\n", data["owner"]);
                    owner.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(data["owner_color"]));
                    owner.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }

                TextRange obj = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);

                if(data["owner"] == "released")
                    obj.Text = string.Format("{0} {1}: claim released.\n", data["time"], data["objective"]);
                else
                    obj.Text = string.Format("{0} {1}: claimed by ", data["time"], data["objective"]);

                obj.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(null));
                obj.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

            }
            else
            {
                TextRange from_to_to = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);
                from_to_to.Text = string.Format("{0}\n", data["to"]);
                from_to_to.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(data["to_color"]));
                from_to_to.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange from_to = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);
                from_to.Text = string.Format(" to ");
                from_to.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(null));
                from_to.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                TextRange from_to_from = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);
                from_to_from.Text = string.Format("{0}", data["from"]);
                from_to_from.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(data["from_color"]));
                from_to_from.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange obj = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart);
                obj.Text = string.Format("{0} {1}: ", data["time"], data["objective"]);
                obj.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(null));
                obj.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
        }

        public void AddCampLog(Dictionary<string, string> data)
        {/*
            TextRange owner = new TextRange(eventCamp.Document.ContentEnd, eventCamp.Document.ContentEnd);
            owner.Text = string.Format("{0}\t", data["time_left"]);
            owner.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(null));
            owner.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            
            TextRange obj = new TextRange(eventCamp.Document.ContentEnd, eventCamp.Document.ContentEnd);
            obj.Text = string.Format("{0}\n", data["objective"]);
            obj.ApplyPropertyValue(TextElement.ForegroundProperty, getColor(null));
            obj.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);*/
        }

        public void ResetText()
        {
            eventLog.Document.Blocks.Clear();
            //eventCamp.Document.Blocks.Clear();
        }
        public void ResetText(string box)
        {
            if (box == "camp")
            {
                //eventCamp.Document.Blocks.Clear();
            }
        }
        public SolidColorBrush getColor(string color)
        {
            if (color == null) color = "black";

            switch (color.ToLower())
            { 
                case "red":
                    return Brushes.DarkRed;
                case "green":
                    return Brushes.DarkGreen; //.Green;
                case "blue":
                    return Brushes.DarkBlue;
                case "black":
                    return Brushes.Black;
                case "white":
                    return Brushes.White;
                default:
                    return Brushes.Black;
            }
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void btnSetClicktrough_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["tracker_saved"] = true;
            Properties.Settings.Default.Save();
            cnvsPromt.Visibility = Visibility.Hidden;
            ClickTroughActivate();
        }

        public void ClickTroughVoid()
        {
            if ((bool)Properties.Settings.Default["tracker_saved"] == false)
                return;
            
            eventLog.IsEnabled = false;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Layered);

            // Weird fix for disappearing window when switching between dual screen.
            this.Topmost = false; 
            this.Topmost = true;
        }

        public void ClickTroughActivate()
        {
            if ((bool)Properties.Settings.Default["tracker_saved"] == false)
                return;

            eventLog.IsEnabled = true;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Transparent);

            // Weird fix for disappearing window when switching between dual screen.
            this.Topmost = false;
            this.Topmost = true;


        }

        private void btnSetHide_Click(object sender, RoutedEventArgs e)
        {
            Tracker.Visibility = Visibility.Hidden;
            Properties.Settings.Default["show_tracker"] = false;
            Properties.Settings.Default.Save();
        }

        private void saveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
