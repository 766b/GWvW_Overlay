using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ArenaNET;
using ArenaNET.DataStructures;
using GWvW_Overlay.DataModel;
using GWvW_Overlay.Keyboard;
using GWvW_Overlay.Properties;
using GWvW_Overlay.Resources.Lang;
using Logitech_LCD;
using Logitech_LCD.Applets;
using Logitech_LED;
using MumbleLink_CSharp_GW2;
using Newtonsoft.Json;
using Utils.Text;
using Objective = GWvW_Overlay.DataModel.Objective;

namespace GWvW_Overlay
{
    public partial class MainWindow
    {
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        static readonly WinEventDelegate ProcDelegate = WinEventProc;
        public static readonly GW2Link DataLink = new GW2Link();
        readonly KeyboardListener _kListener = new KeyboardListener();
        readonly IntPtr _hhook;

        static MainWindow _handleThis;

        bool _resetMatch;
        static bool _inGame;
        private bool? _adjustingHeight;

        readonly Timer _t1 = new Timer();
        readonly Timer _mapDetectTimer = new Timer(100);
        private int _currentMapId;
        readonly Timer _t3 = new Timer();



        //JSON Data
        private readonly WvwMatchup _wvwMatch = new WvwMatchup();

        public WvwMatchup WvwMatch { get { return _wvwMatch; } }

        List<WvWMatch> _jsonMatches = new List<WvWMatch>();

        public static Guild GuildData = new Guild();

        readonly CampLogger LogWindow = new CampLogger();

        public BaseApplet applet;

        //About & Settings Windows
        private SetOptions optionWindow;
        private About aboutWindow;

        #region Win32
        public void ClickTroughActivate()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Transparent);

            LogWindow.ClickTroughActivate();
        }

        public void ClickTroughVoid()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Layered);

            LogWindow.ClickTroughVoid();
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            var wTitle = new StringBuilder(13);
            Natives.GetWindowText(hwnd, wTitle, 13);

            if (wTitle.ToString() == "Guild Wars 2" && _inGame != true)
            {
                _inGame = true;
                _handleThis.ClickTroughActivate();
            }
            else if (wTitle.ToString() != "Guild Wars 2" && _inGame)
            {
                _inGame = false;
                _handleThis.ClickTroughVoid();
            }
        }

        public static Point GetMousePosition() // mouse position relative to screen
        {
            var w32Mouse = new Natives.Win32Point();
            Natives.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if (args.Key.ToString() == Settings.Default["hotkey"].ToString() && !(bool)Settings.Default["alwaysTop"])
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    Natives.ShowWindow(handle, 4);
                }));
            }
        }

        void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            if (args.Key.ToString() == Settings.Default["hotkey"].ToString() && !(bool)Settings.Default["alwaysTop"])
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    Natives.ShowWindow(handle, 6);
                }));
            }
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            double aspectRatio = WvwMatch.Options.width / WvwMatch.Options.height;
            switch ((Natives.WM)msg)
            {
                case Natives.WM.WINDOWPOSCHANGING:
                    {
                        var pos = (Natives.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(Natives.WINDOWPOS));

                        if ((pos.flags & (int)Natives.SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        var hwndSource = HwndSource.FromHwnd(hwnd);
                        if (hwndSource != null)
                        {
                            var wnd = (Window)hwndSource.RootVisual;
                            if (wnd == null)
                                return IntPtr.Zero;
                        }

                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));
                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * aspectRatio); // adjusting width to heigth change

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case Natives.WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion


        public MainWindow()
        {
            RtvMatches();
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 50.0;
            SourceInitialized += Window_SourceInitialized;

            _hhook = SetWinEventHook(Natives.EVENT_SYSTEM_FOREGROUND, Natives.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                ProcDelegate, 0, 0, Natives.WINEVENT_SKIPOWNPROCESS); // | Natives.WINEVENT_SKIPOWNPROCESS

            MainWindow1.WindowStartupLocation = WindowStartupLocation.CenterScreen;


            _kListener.KeyDown += KListener_KeyDown;
            _kListener.KeyUp += KListener_KeyUp;

            _t1.Interval = 4000;
            _t1.Elapsed += RtvMatchDetails;
            _t1.Start();


            _mapDetectTimer.Elapsed += (sender, args) =>
            {
                if (WvwMatch.Details == null) return;
                var map = DataLink.GetCoordinates().MapId;
                if (map != _currentMapId && WvwMatch.Details.Maps.Any(m => m.Id == map))
                {

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        BorderlandSelected(WvwMatch.Details.Maps.First(m => m.Id == map).Type, EventArgs.Empty);
                    }));

                }
                _currentMapId = map;
            };

            _mapDetectTimer.Enabled = Settings.Default.auto_switch_map;

            _t3.Interval = 1000;
            _t3.Elapsed += UpdateTimers;

            _handleThis = this;

            Console.WriteLine(CmbbxHomeServerSelection.Items.Count);
            foreach (World item in CmbbxHomeServerSelection.Items)
            {
                if (item.Id == Settings.Default.home_server)
                    CmbbxHomeServerSelection.SelectedItem = item;
            }

            BuildMenu();

            if (LogitechLcd.Instance.IsConnected(LcdType.Color))
            {
                applet = new ColorDisplayApplet(this, WvwMatch);
            }
            else if (LogitechLcd.Instance.IsConnected(LcdType.Mono))
            {
                //applet = new MonoDisplayApplet();
            }


        }

        public void UpdatePosition(Object source, EventArgs e)
        {
            if (WvwMatch.Matches == null || WvwMatch.Details == null)
                return;

            foreach (ArenaNET.Map map in WvwMatch.Details.Maps)
            {
                foreach (ArenaNET.Objective obj in map.Objectives)
                {
                    DateTime cur = DateTime.Now;

                    if (obj.Coordinates != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            if (obj.DisplayCoordinates == null)
                            {
                                obj.DisplayCoordinates = new Coordinate();
                            }

                            var mapSize = new Coordinate()
                            {
                                X = Math.Abs(map.MapRect[0].X) + Math.Abs(map.MapRect[1].X),
                                Y = Math.Abs(map.MapRect[0].Y) + Math.Abs(map.MapRect[1].Y)
                            };

                            ArenaNET.Objective.DisplayHeight = Height;
                            ArenaNET.Objective.DisplayWidth = Width - 80;
                            WvwMatch.PlayerPositions.CanvasHeight = Height;
                            WvwMatch.PlayerPositions.CanvasWidth = Width;
                        }));
                    }
                }
            }

        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {

            LogWindow.Show();
            if (!Settings.Default.show_tracker)
                LogWindow.Hide();

            if (!Settings.Default.auto_matchup)
            {
                cnvsMatchSelection.Visibility = Visibility.Visible;

            }
            else
            {
                cnvsMatchSelection.Visibility = Visibility.Hidden;
                AutoMatchSetActiveMatch();
                GetBorderlandSelection();
            }
        }

        public void GetBorderlandSelection()
        {
            if (Settings.Default.server_color_lightning && LogitechLed.Instance.IsInit)
            {
                switch (WvwMatch.HomeServerColor)
                {
                    case "red":
                        LogitechLed.Instance.SetLighting(100, 0, 0);
                        break;
                    case "green":
                        LogitechLed.Instance.SetLighting(0, 100, 0);
                        break;
                    case "blue":
                        LogitechLed.Instance.SetLighting(0, 0, 100);
                        break;
                    default:
                        LogitechLed.Instance.SetLighting(100, 100, 100);
                        break;

                }
            }
            CnvsBlSelection.Visibility = Visibility.Visible;
            LblBlueBl.Content = WvwMatch.Details.Worlds.Blue.Name;
            LblGreenBl.Content = WvwMatch.Details.Worlds.Green.Name;
            LblRedBl.Content = WvwMatch.Details.Worlds.Red.Name;
        }

        public void AutoMatchSetActiveMatch()
        {
            foreach (var match in WvwMatch.Matches)
            {
                if (match.Worlds.Blue.Id == Settings.Default.home_server
                    || match.Worlds.Green.Id == Settings.Default.home_server
                    || match.Worlds.Red.Id == Settings.Default.home_server)
                {
                    WvwMatch.Options.active_match = match.Id;
                    RtvMatchDetails(null, null);
                    BuildMenu();
                    break;
                }
            }
        }

        public void BuildMenu()
        {
            var mainMenu = new ContextMenu();

            var matches = new MenuItem { Header = "Matches" };
            var y = WvwMatch.Matches;
            foreach (var x in y)
            {
                var i = new MenuItem { Header = x, Tag = x.Id };
                i.Click += MatchSelected;
                matches.Items.Add(i);
            }
            mainMenu.Items.Add(matches);

            var menuOptions = new MenuItem { Header = "Options" };
            menuOptions.Click += ShowOptionsWindow;
            mainMenu.Items.Add(menuOptions);

            if (WvwMatch.Options.active_match != null)
            {
                var blBlue = new MenuItem { Header = string.Format(Strings.blueBorderland + " ({0})", WvwMatch.Details.Worlds.Blue.Name), Tag = "BlueHome" };
                blBlue.Click += BorderlandSelected;

                var blRed = new MenuItem { Header = string.Format(Strings.redBorderland + " ({0})", WvwMatch.Details.Worlds.Red.Name), Tag = "RedHome" };
                blRed.Click += BorderlandSelected;

                var blGreen = new MenuItem { Header = string.Format(Strings.greenBorderland + " ({0})", WvwMatch.Details.Worlds.Green.Name), Tag = "GreenHome" };
                blGreen.Click += BorderlandSelected;

                var blEb = new MenuItem { Header = Strings.eternalBattlegrounds, Tag = "Center" };
                blEb.Click += BorderlandSelected;

                mainMenu.Items.Add(new Separator());

                mainMenu.Items.Add(blGreen);
                mainMenu.Items.Add(blBlue);
                mainMenu.Items.Add(blRed);
                mainMenu.Items.Add(blEb);

            }
            var aboutWin = new MenuItem { Header = Strings.about };
            aboutWin.Click += ShowAboutWin;
            mainMenu.Items.Add(aboutWin);

            var exitApp = new MenuItem { Header = Strings.exit };
            exitApp.Click += ExitApp;

            mainMenu.Items.Add(exitApp);
            ContextMenu = mainMenu;
        }

        public void UpdateTimers(Object source, ElapsedEventArgs e)
        {
            if (WvwMatch.Details == null)
                return;

            AdjustScore();

            var cur = DateTime.Now;
            string eventCamps = "";

            for (int i = 0; i < WvwMatch.Details.Maps.Count; i++)
            {
                int map = i;

                for (int m = 0; m < WvwMatch.Details.Maps[map].Objectives.Count; m++)
                {
                    int obj = m;

                    WvwMatch.Details.Maps[map].Objectives[obj].TimeLeft = new TimeSpan();

                    if (!WvwMatch.Details.Maps[map].Objectives[obj].LastFlipped.HasValue) continue;

                    TimeSpan diff = cur.Subtract(WvwMatch.Details.Maps[map].Objectives[obj].LastFlipped.Value);
                    TimeSpan left = TimeSpan.FromMinutes(5) - diff;
                    if (diff < TimeSpan.FromMinutes(5))
                    {
                        if (WvwMatch.Details.Maps[map].Objectives[obj].Type == "Camp" && WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                        {
                            eventCamps += string.Format("{0}\t{1}\n", left.ToString(@"mm\:ss"), WvwMatch.Details.Maps[map].Objectives[obj].Name);
                        }
                    }
                    else
                    {

                        if (WvwMatch.Details.Maps[map].Objectives[obj].Type == "Camp" && WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                        {
                            eventCamps += string.Format("{0}\t{1}\n", "N/A", WvwMatch.Details.Maps[map].Objectives[obj].Name);
                        }
                    }
                }
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                LogWindow.txtblk_eventCamp.Text = eventCamps;
            }));
        }

        public void AdjustScore()
        {
            //Tracker

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    if (LogWindow != null)
                    {
                        var scores = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Scores;
                        var scoreSum = scores.Green + scores.Blue + scores.Red;
                        LogWindow.RedScoreLocal.Width = 120 * (scores.Red / scoreSum);
                        LogWindow.BlueScoreLocal.Width = 120 * (scores.Blue / scoreSum);
                        LogWindow.GreenScoreLocal.Width = 120 * (scores.Green / scoreSum);

                        LogWindow.LblCampCount.Content =
                            WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Objectives.Count(
                                o =>
                                    o.Type == "Camp" &&
                                    Comparison.CaseInsensitiveComparison(o.Owner, WvwMatch.HomeServerColor));
                        LogWindow.LblTowerCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Objectives.Count(
                                o =>
                                    o.Type == "Tower" &&
                                    Comparison.CaseInsensitiveComparison(o.Owner, WvwMatch.HomeServerColor));
                        LogWindow.LblCastleCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Objectives.Count(
                                o =>
                                    o.Type == "Castle" &&
                                    Comparison.CaseInsensitiveComparison(o.Owner, WvwMatch.HomeServerColor));
                        LogWindow.LblKeepCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Objectives.Count(
                                o =>
                                    o.Type == "Keep" &&
                                    Comparison.CaseInsensitiveComparison(o.Owner, WvwMatch.HomeServerColor));
                    }
                }));


            ////Main map
            //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            //{
            //    RedBarGlobal.Width = ScoreBoard.Width * (WvwMatch.Details.Scores[0] / WvwMatch.Details.ScoresSum);
            //    BlueBarGlobal.Width = (ScoreBoard.Width * (WvwMatch.Details.Scores[1] / WvwMatch.Details.ScoresSum)) + 1;

            //    //not impl
            //    RedBarBL.Width = 40;
            //    BlueBarBL.Width = 10;
            //}));
        }

        public void RtvMatches()
        {
            _jsonMatches = Request.GetResourceBulk<WvWMatch>("all");
            _jsonMatches.Sort((x, y) => y.Id != null ? (x.Id != null ? String.Compare(x.Id, y.Id, StringComparison.Ordinal) : 0) : 0);
            WvwMatch.Matches = _jsonMatches;
            WvwMatch.InitBlid();
            _jsonMatches = null;
        }

        public void RtvMatchDetails(Object source, ElapsedEventArgs e)
        {
            if (WvwMatch.Options.active_match == null)
                return;

            if (WvwMatch.Details == null || _resetMatch)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    LogWindow.ResetText();
                }));
                WvwMatch.Details = WvwMatch.Matches.Find(m => m.Id == WvwMatch.Options.active_match);
                _resetMatch = false;

            }
            else
            {
                var matchDetails = Request.GetResource<WvWMatch>(WvwMatch.Details.Id);

                WvwMatch.Details.Id = matchDetails.Id;
                WvwMatch.Details.Scores = matchDetails.Scores;


                WvwMatch.Details.Maps.ForEach(map =>
                {
                    var remoteMap = matchDetails.Maps.Find(m => m.Type == map.Type);

                    map.Scores = remoteMap.Scores;


                    remoteMap.Objectives.ForEach(obj =>
                    {
                        var localObj = map.Objectives.Find(o => o.Id == obj.Id);

                        if (!obj.ClaimedBy.IsEmpty())
                        {
                            GuildData.GetGuildById(obj.ClaimedBy);
                        }

                        if (localObj.Owner != obj.Owner)
                        {
                            if (WvwMatch.Options.active_bl == map.Type)
                            {
                                var dict = new Dictionary<string, string>
                                    { 
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective", localObj.Name},
                                        {"from", WvwMatch.GetServerName(localObj.Owner)},
                                        {"from_color", localObj.Owner},
                                        {"to", WvwMatch.GetServerName(obj.Owner)},
                                        {"to_color", obj.Owner}
                                    };
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LogWindow.AddEventLog(dict, false)));
                            }

                            localObj.Owner = obj.Owner;
                            localObj.ClaimedBy = obj.ClaimedBy;
                            localObj.LastFlipped = obj.LastFlipped;
                        }
                        if (localObj.ClaimedBy != obj.ClaimedBy &&
                            localObj.Owner == obj.Owner)
                        {
                            localObj.ClaimedBy = obj.ClaimedBy;

                            if (WvwMatch.Options.active_bl == map.Type)
                            {
                                var guildInfo = GuildData.GetGuildById(localObj.ClaimedBy);
                                var dict = new Dictionary<string, string>
                                    {
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective", localObj.Name},
                                        {"owner_color", localObj.Owner}, {"owner", guildInfo == null ? "released" : string.Format("[{1}] {0}", guildInfo[0], guildInfo[1])}
                                    };

                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LogWindow.AddEventLog(dict, true)));
                            }

                        }
                    });



                });

            }

            _t3.Start();
        }

        public ImageSource GetPng(string type, string color)
        {
            var y = color == "none" ? string.Format("Resources/{0}.png", type) : string.Format("Resources/{0}_{1}.png", type, color.ToLower());
            ImageSource x = new BitmapImage(new Uri(y, UriKind.Relative));
            return x;
        }



        public void MatchSelected(object sender, EventArgs e)
        {
            if (WvwMatch.Options.active_match != (string)((MenuItem)sender).Tag)
            {
                _resetMatch = true;
                Icons.ItemsSource = null;
            }

            WvwMatch.Options.active_match = (string)((MenuItem)sender).Tag;
            RtvMatchDetails(null, null);
            BuildMenu();
            //ContextMenu.IsOpen = true;

            GetBorderlandSelection();
        }

        public void BorderlandSelected(object sender, EventArgs e)
        {
            string selectedBl = null;

            var label = sender as Label;
            if (label != null)
                selectedBl = (string)label.Tag;

            var item = sender as MenuItem;
            if (item != null)
                selectedBl = (string)item.Tag;

            var str = sender as String;
            if (str != null)
                selectedBl = str;

            if (selectedBl == null)
                selectedBl = "Center";

            WvwMatch.Options.active_bl = selectedBl;
            Icons.ItemsSource = WvwMatch.Details.Maps[WvwMatch.Options.blid[selectedBl]].Objectives;
            WvwMatch.MarkersVisibility = Visibility.Visible;

            if (LogWindow != null)
                LogWindow.lblBLTitle.Content = WvwMatch.Options.active_bl_title;

            MainWindow1.InvalidateVisual();

            CnvsBlSelection.Visibility = Visibility.Hidden;

            ArenaNET.Objective.DisplayWidth = Width - 80;
            ArenaNET.Objective.DisplayHeight = Height;

        }

        public void ExitApp(object sender, EventArgs e)
        {
            _t1.Stop();
            _t3.Stop();
            _mapDetectTimer.Stop();
            UnhookWinEvent(_hhook);
            LogWindow.Close();
            Application.Current.Shutdown();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                try
                {
                    DragMove();
                }
                catch
                {

                }

        }

        private void ShowOptionsWindow(object sender, EventArgs e)
        {
            if ((optionWindow == null) || (!optionWindow.IsVisible))
            {
                optionWindow = new SetOptions(LogWindow, WvwMatch, MainWindow1);
            }
            optionWindow.Show();
            optionWindow.Focus();
        }

        private void ShowAboutWin(object sender, EventArgs e)
        {
            if ((aboutWindow == null) || (!aboutWindow.IsVisible))
            {
                aboutWindow = new About();
            }
            aboutWindow.Show();
            aboutWindow.Focus();
        }




        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual((Window)sender);
            if (hwndSource != null) hwndSource.AddHook(DragHook);

            Console.WriteLine(CmbbxHomeServerSelection.Items.Count);
            foreach (World item in CmbbxHomeServerSelection.Items)
            {
                if (item.Id == Settings.Default.home_server)
                    CmbbxHomeServerSelection.SelectedItem = item;
            }
        }



        private void MainClosing(object sender, CancelEventArgs e)
        {
            UnhookWinEvent(_hhook);
            LogWindow.Close();
            Settings.Default.Save();
        }

        private void lblSelectionOK_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (CmbbxHomeServerSelection.SelectedValue == null && LstbxMatchSelection.SelectedValue == null)
            {
                MessageBox.Show("Please select the Home Server or one of the avaliable matches.");
                return;
            }
            //If auto-match-up is TRUE and home-server is NOT selected
            if (ChkbxAutoMatchSelect.IsChecked != null && (bool)ChkbxAutoMatchSelect.IsChecked &&
                CmbbxHomeServerSelection.SelectedValue == null)
            {
                MessageBox.Show("Automatic Match Selection requires \"Home Server\" to be set.");
                return;
            }

            //Check if auto-match-up is TRUE and home-server is selected
            if (ChkbxAutoMatchSelect.IsChecked != null && (bool)ChkbxAutoMatchSelect.IsChecked &&
                CmbbxHomeServerSelection.SelectedValue != null)
            {
                Settings.Default.auto_matchup = ChkbxAutoMatchSelect.IsChecked.Value;
                Settings.Default["home_server"] = CmbbxHomeServerSelection.SelectedValue;
                Settings.Default.Save();
                AutoMatchSetActiveMatch();
            }
            // If home-server is set go to BL selection
            else if (CmbbxHomeServerSelection.SelectedValue != null)
            {
                Settings.Default["home_server"] = CmbbxHomeServerSelection.SelectedValue;
                Settings.Default.Save();
                AutoMatchSetActiveMatch();
            }
            else if (LstbxMatchSelection.SelectedItem != null)
            {
                WvwMatch.Options.active_match = (string)LstbxMatchSelection.SelectedValue;
                RtvMatchDetails(null, null);
            }
            BuildMenu();
            cnvsMatchSelection.Visibility = Visibility.Hidden;
            GetBorderlandSelection();
        }

        private void CmbbxMatchSelection_Change(object sender, SelectionChangedEventArgs e)
        {
            if (CmbbxHomeServerSelection.SelectedItem == null)
                return;

            var selection = (World)CmbbxHomeServerSelection.SelectedItem;

            Console.WriteLine(selection.Name);

            CnvsMatchUp.Visibility = Visibility.Hidden;
            cnvsMatchSelection.Height = 100;
        }

        private void lblSelectionReset_Click(object sender, MouseButtonEventArgs e)
        {
            cnvsMatchSelection.Height = 400;
            CnvsMatchUp.Visibility = Visibility.Visible;
            CmbbxHomeServerSelection.SelectedItem = null;
            LstbxMatchSelection.SelectedItem = null;
        }
    }
}
