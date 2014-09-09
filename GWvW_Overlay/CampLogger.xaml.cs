using GWvW_Overlay.Resources.Lang;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace GWvW_Overlay
{
    public partial class CampLogger
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
                    var owner = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format(" {0}\n", data["owner"]) };
                    owner.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(data["owner_color"]));
                    owner.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }

                var obj = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format(data["owner"] == "released" ? "{0} {1}: " + Strings.claimReleased + ".\n" : "{0} {1}: " + Strings.claimedBy + " ", data["time"], data["objective"]) };

                obj.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(null));
                obj.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

            }
            else
            {
                var fromToTo = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format("{0}\n", data["to"]) };
                fromToTo.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(data["to_color"]));
                fromToTo.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                var fromTo = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format(Strings.to) };
                fromTo.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(null));
                fromTo.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);

                var fromToFrom = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format("{0}", data["from"]) };
                fromToFrom.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(data["from_color"]));
                fromToFrom.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                var obj = new TextRange(eventLog.Document.ContentStart, eventLog.Document.ContentStart) { Text = string.Format("{0} {1}: ", data["time"], data["objective"]) };
                obj.ApplyPropertyValue(TextElement.ForegroundProperty, GetColor(null));
                obj.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            }
        }

        public void ResetText()
        {
            eventLog.Document.Blocks.Clear();
        }

        public SolidColorBrush GetColor(string color)
        {
            if (color == null) color = "white";

            switch (color.ToLower())
            {
                case "red":
                    return Brushes.Firebrick;
                case "green":
                    return Brushes.Green; //.Green;
                case "blue":
                    return Brushes.SteelBlue;
                case "black":
                    return Brushes.Black;
                case "neutral":
                    return (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFEEEE"));
                case "white":
                    return (SolidColorBrush)(new BrushConverter().ConvertFromString("#FFEEEE"));
                default:
                    return Brushes.Black;
            }
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
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

            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Layered);

            // Weird fix for disappearing window when switching between dual screen.
            Topmost = false;
            Topmost = true;
        }

        public void ClickTroughActivate()
        {
            if ((bool)Properties.Settings.Default["tracker_saved"] == false)
                return;

            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Transparent);

            // Weird fix for disappearing window when switching between dual screen.
            Topmost = false;
            Topmost = true;
        }

        private void btnSetHide_Click(object sender, RoutedEventArgs e)
        {
            Tracker.Visibility = Visibility.Hidden;
            Properties.Settings.Default["show_tracker"] = false;
            Properties.Settings.Default.Save();
        }

        private void SaveSettings(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
