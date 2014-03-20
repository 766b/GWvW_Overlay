/*
 * Objective names https://gist.github.com/codemasher/bac2b4f87e7af128087e (smiley.1438)
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Windows.Interop;
using System.Runtime.InteropServices;

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

        readonly Keyboard.KeyboardListener _kListener = new Keyboard.KeyboardListener();
        readonly IntPtr _hhook;

        static MainWindow _handleThis;

        bool _resetMatch;
        static bool _inGame;
        private bool? _adjustingHeight;

        readonly System.Timers.Timer _t1 = new System.Timers.Timer();
        readonly System.Timers.Timer _t2 = new System.Timers.Timer();
        readonly System.Timers.Timer _t3 = new System.Timers.Timer();

        //JSON Data
        Match_Details_ _matchDetails = new Match_Details_();
        readonly WvwMatch_ WvwMatch = new WvwMatch_();
        Matches_ _jsonMatches = new Matches_();

        public Utils Utils = new Utils();
        public Guild GuildData = new Guild();

        readonly CampLogger LogWindow = new CampLogger();

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
        public MainWindow()
        {
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

            _t2.Interval = 1000;
            _t2.Elapsed += UpdatePosition;
            _t2.Start();
            

            _t3.Interval = 1000;
            _t3.Elapsed += UpdateTimers;

            _handleThis = this;

            RtvWorldNames();
            RtvMatches();

            Console.WriteLine(CmbbxHomeServerSelection.Items.Count);
            foreach (World_Names_ item in CmbbxHomeServerSelection.Items)
            {
                if (item.id == (int)Properties.Settings.Default["home_server"])
                    CmbbxHomeServerSelection.SelectedItem = item;
            }

            BuildMenu();

        }

        public void UpdatePosition(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Details == null)
                return;

            foreach (Map map in WvwMatch.Details.Maps)
            {
                foreach (Objective obj in map.Objectives)
                {
                    DateTime cur = DateTime.Now;

                    if (obj.ObjData.top != 0.0)
                    {
                        Objective obj1 = obj;
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            obj1.ObjData.left = Width * (obj1.ObjData.left_base / obj1.ObjData.res_width);
                            obj1.ObjData.top = Height * (obj1.ObjData.top_base / obj1.ObjData.res_height);
                        }));
                    }
                }
            }
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            DataContext = WvwMatch;

            LogWindow.Show();
            if (!(bool)Properties.Settings.Default["show_tracker"])
                LogWindow.Hide();


            //Test
            /*
            Dictionary<string, string> dict1 = new Dictionary<string, string>() { { "time", DateTime.Now.ToString("t") }, { "objective", "Objective Name" }, { "owner_color", "green" }, { "owner", "released" } };
            Dictionary<string, string> dict2 = new Dictionary<string, string>() { { "time", DateTime.Now.ToString("t") }, { "objective", "Objective Name" }, { "owner_color", "red" }, { "owner", "[Tag] Name" } };

            Dictionary<string, string> dict3 = new Dictionary<string, string>() 
                                    { 
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective",  "Objective Name"},
                                        {"from", "Server Name 1"},
                                        {"from_color", "blue"},
                                        {"to", "Server Name 2"},
                                        {"to_color", "green"},
                                    };

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                LogWindow.AddEventLog(dict1, true);
                LogWindow.AddEventLog(dict2, true);
                LogWindow.AddEventLog(dict3, false);
            }));*/

            if (!(bool) Properties.Settings.Default["auto_matchup"])
            {
                cnvsMatchSelection.Visibility = Visibility.Visible;

            }
            else
            {
                cnvsMatchSelection.Visibility = Visibility.Hidden;
                AutoMatchSetActiveMatch();
                GetBorderlandSelection();
            }

            //rtvMatchDetails(null, null);
        }

        public void GetBorderlandSelection()
        {
            CnvsBlSelection.Visibility = Visibility.Visible;
            LblBlueBl.Content = WvwMatch.getServerName("blue");
            LblGreenBl.Content = WvwMatch.getServerName("green");
            LblRedBl.Content = WvwMatch.getServerName("red");
        }

        public void AutoMatchSetActiveMatch()
        {
            foreach (var match in WvwMatch.Match)
            {
                if (match.blue_world_id == (int)Properties.Settings.Default["home_server"]
                    || match.green_world_id == (int)Properties.Settings.Default["home_server"]
                    || match.red_world_id == (int)Properties.Settings.Default["home_server"])
                {
                    WvwMatch.Options.active_match = match.wvw_match_id;
                    RtvMatchDetails(null, null);
                    break;
                }
            }
        }

        public void BuildMenu()
        {
            var mainMenu = new ContextMenu();

            var matches = new MenuItem {Header = "Matches"};
            var y = WvwMatch.GetMatchesList();
            foreach (var x in y)
            {
                var i = new MenuItem {Header = x.Value, Tag = x.Key};
                i.Click += MatchSelected;
                matches.Items.Add(i);
            }
            mainMenu.Items.Add(matches);

            var menuOptions = new MenuItem {Header = "Options"};
            menuOptions.Click += ShowOptionsWindow;
            mainMenu.Items.Add(menuOptions);

            if (WvwMatch.Options.active_match != null)
            {
                var blBlue = new MenuItem {Header = string.Format("Blue Borderland ({0})", WvwMatch.getServerName("blue")),Tag = "BlueHome"};
                blBlue.Click += BorderlandSelected;

                var blRed = new MenuItem {Header = string.Format("Red Borderland ({0})", WvwMatch.getServerName("red")),Tag = "RedHome"};
                blRed.Click += BorderlandSelected;

                var blGreen = new MenuItem {Header = string.Format("Green Borderland ({0})", WvwMatch.getServerName("green")), Tag = "GreenHome"};
                blGreen.Click += BorderlandSelected;

                var blEb = new MenuItem {Header = "Eternal Battleground", Tag = "Center"};
                blEb.Click += BorderlandSelected;

                mainMenu.Items.Add(new Separator());
                
                mainMenu.Items.Add(blBlue);
                mainMenu.Items.Add(blGreen);
                mainMenu.Items.Add(blRed);
                mainMenu.Items.Add(blEb);
                
            }
            var aboutWin = new MenuItem { Header = "About" };
            aboutWin.Click += ShowAboutWin;
            mainMenu.Items.Add(aboutWin);

            var exitApp = new MenuItem {Header = "Exit"};
            exitApp.Click += ExitApp;

            mainMenu.Items.Add(exitApp);
            ContextMenu = mainMenu;
        }

        public void UpdateTimers(Object source, System.Timers.ElapsedEventArgs e)
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
                    if (WvwMatch.Details.Maps[map].Objectives[obj].id >= 62) // Skip Ruins of Power. No the best way to go about it...
                        continue; 

                    TimeSpan diff = cur.Subtract(WvwMatch.Details.Maps[map].Objectives[obj].last_change);
                    TimeSpan left = TimeSpan.FromMinutes(5) - diff;
                    if (diff < TimeSpan.FromMinutes(5)) 
                    {
                        if (WvwMatch.Details.Maps[map].Objectives[obj].ObjData.type == "camp" && WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                        {
                           /* Dictionary<string, string> dict = new Dictionary<string, string>()
                            {
                                {"objective", WvwMatch.Details.maps[map].objectives[obj].ObjData.name},
                                {"time_left", left.ToString(@"mm\:ss")}
                            };

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                LogWindow.AddCampLog(dict);
                            }));*/
                            eventCamps += string.Format("{0}\t{1}\n", left.ToString(@"mm\:ss"), WvwMatch.Details.Maps[map].Objectives[obj].ObjData.name);
                        }

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.Maps[map].Objectives[obj].time_left = left.ToString(@"mm\:ss");
                        }));
                    } 
                    else 
                    {

                        if (WvwMatch.Details.Maps[map].Objectives[obj].ObjData.type == "camp" && WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                        {
                            /*Dictionary<string, string> dict = new Dictionary<string, string>()
                            {
                                {"objective", WvwMatch.Details.maps[map].objectives[obj].ObjData.name},
                                {"time_left", "N/A"}
                            };

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                LogWindow.AddCampLog(dict);
                            }));*/
                            eventCamps += string.Format("{0}\t{1}\n", "N/A", WvwMatch.Details.Maps[map].Objectives[obj].ObjData.name);
                        }
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.Maps[map].Objectives[obj].time_left = " ";
                        }));
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
                    if(LogWindow != null)
                    {
                        LogWindow.RedScoreLocal.Width = 120 * (WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Scores[0] / WvwMatch.Details.Maps[WvwMatch.Options.active_blid].ScoresSum);
                        LogWindow.BlueScoreLocal.Width = 120 * (WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Scores[1] / WvwMatch.Details.Maps[WvwMatch.Options.active_blid].ScoresSum);
                        LogWindow.GreenScoreLocal.Width = 120 * (WvwMatch.Details.Maps[WvwMatch.Options.active_blid].Scores[2] / WvwMatch.Details.Maps[WvwMatch.Options.active_blid].ScoresSum);

                        LogWindow.LblCampCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].CountObjType("camp", WvwMatch.HomeServerColor);
                        LogWindow.LblTowerCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].CountObjType("tower", WvwMatch.HomeServerColor);
                        LogWindow.LblCastleCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].CountObjType("castle", WvwMatch.HomeServerColor);
                        LogWindow.LblKeepCount.Content = WvwMatch.Details.Maps[WvwMatch.Options.active_blid].CountObjType("keep", WvwMatch.HomeServerColor);
                    }
            }));
            

            //Main map
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                RedBarGlobal.Width = ScoreBoard.Width * (WvwMatch.Details.Scores[0] / WvwMatch.Details.ScoresSum);
                BlueBarGlobal.Width = (ScoreBoard.Width * (WvwMatch.Details.Scores[1] / WvwMatch.Details.ScoresSum)) + 1;

                //not impl
                RedBarBL.Width = 40;
                BlueBarBL.Width = 10;
            }));
        }

        public void RtvWorldNames()
        {
            WvwMatch.World = JsonConvert.DeserializeObject<List<World_Names_>>(Utils.GetJson(@"https://api.guildwars2.com/v1/world_names.json"));

            Console.WriteLine(WvwMatch.World.Count);
            WvwMatch.World.Sort((x, y) => y.name != null ? (x.name != null ? String.Compare(x.name, y.name, StringComparison.Ordinal) : 0) : 0);
        }

        public void RtvMatches()
        {
            _jsonMatches = JsonConvert.DeserializeObject<Matches_>(Utils.GetJson("https://api.guildwars2.com/v1/wvw/matches.json"));
            _jsonMatches.wvw_matches.Sort((x, y) => y.wvw_match_id != null ? (x.wvw_match_id != null ? String.Compare(x.wvw_match_id, y.wvw_match_id, StringComparison.Ordinal) : 0) : 0);
            WvwMatch.Match = _jsonMatches.wvw_matches;
            _jsonMatches = null;
        }
        
        public void RtvMatchDetails(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Options.active_match == null)
                return;

            _matchDetails = JsonConvert.DeserializeObject<Match_Details_>(Utils.GetJson("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=" + WvwMatch.Options.active_match));

            if (WvwMatch.Details == null || _resetMatch)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    LogWindow.ResetText();
                }));
                    WvwMatch.Details = _matchDetails;
                    _resetMatch = false;
                    WvwMatch.GetBLID();
                
            }
            else
            { 
                WvwMatch.Details.match_id = _matchDetails.match_id;
                WvwMatch.Details.Scores = _matchDetails.Scores;
                for (int i = 0; i < WvwMatch.Details.Maps.Count; i++)
                {
                    int map = i;
                    WvwMatch.Details.Maps[map].Scores = _matchDetails.Maps[map].Scores;

                    for (int m = 0; m < _matchDetails.Maps[map].Objectives.Count; m++)
                    {
                        int obj = m;

                        //Caching Guild info
                        if(_matchDetails.Maps[map].Objectives[obj].owner_guild != null)
                        {
                            GuildData.GetGuildById(_matchDetails.Maps[map].Objectives[obj].owner_guild);
                        }

                        if (WvwMatch.Details.Maps[map].Objectives[obj].owner != _matchDetails.Maps[map].Objectives[obj].owner)
                        {
                            if (WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                            {
                                var dict = new Dictionary<string, string>
                                    { 
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective", WvwMatch.Details.Maps[map].Objectives[obj].ObjData.name},
                                        {"from", WvwMatch.getServerName(WvwMatch.Details.Maps[map].Objectives[obj].owner)},
                                        {"from_color", WvwMatch.Details.Maps[map].Objectives[obj].owner},
                                        {"to", WvwMatch.getServerName(_matchDetails.Maps[map].Objectives[obj].owner)},
                                        {"to_color", _matchDetails.Maps[map].Objectives[obj].owner},
                                    };
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LogWindow.AddEventLog(dict, false)));
                            }

                            WvwMatch.Details.Maps[map].Objectives[obj].owner = _matchDetails.Maps[map].Objectives[obj].owner;
                            WvwMatch.Details.Maps[map].Objectives[obj].owner_guild = _matchDetails.Maps[map].Objectives[obj].owner_guild;
                            WvwMatch.Details.Maps[map].Objectives[obj].last_change = DateTime.Now;
                        }
                        if (WvwMatch.Details.Maps[map].Objectives[obj].owner_guild != _matchDetails.Maps[map].Objectives[obj].owner_guild &&
                            WvwMatch.Details.Maps[map].Objectives[obj].owner == _matchDetails.Maps[map].Objectives[obj].owner)
                        {
                            WvwMatch.Details.Maps[map].Objectives[obj].owner_guild = _matchDetails.Maps[map].Objectives[obj].owner_guild;

                            if (WvwMatch.Options.active_bl == WvwMatch.Details.Maps[map].Type)
                            {
                                var guildInfo = GuildData.GetGuildById(WvwMatch.Details.Maps[map].Objectives[obj].owner_guild);
                                var dict = new Dictionary<string, string>
                                    {
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective", WvwMatch.Details.Maps[map].Objectives[obj].ObjData.name},
                                        {"owner_color", WvwMatch.Details.Maps[map].Objectives[obj].owner}, {"owner", guildInfo == null ? "released" : string.Format("[{1}] {0}", guildInfo[0], guildInfo[1])}
                                    };

                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => LogWindow.AddEventLog(dict, true)));
                            }
                            
                        }

                    }
                }

            }

            // Fill objective names and icons positions
            if(WvwMatch.Details.Maps[3].Objectives[0].ObjData.name == null)
            {
                for (int i = 0; i < WvwMatch.Details.Maps.Count; i++)
                {
                    int map = i;
                    var objData = JsonConvert.DeserializeObject<List<WvwObjective>>(Utils.GetJson(string.Format("Resources/obj_{0}.json", WvwMatch.Details.Maps[map].Type)));
                    foreach (var obj in objData)
                    {
                        for (int y = 0; y < WvwMatch.Details.Maps[map].Objectives.Count; y++)
                        {
                            var objct = y;
                            if (obj.id == WvwMatch.Details.Maps[map].Objectives[objct].id)
                                WvwMatch.Details.Maps[map].Objectives[objct].ObjData = obj;
                        }
                    }
                }
            }

            _t3.Start();
        }

        public ImageSource GetPng(string type, string color)
        {
            var y = color == "none" ? string.Format("Resources/{0}.png", type) : string.Format("Resources/{0}_{1}.png", type, color.ToLower());
            ImageSource x = new BitmapImage(new Uri(y, UriKind.Relative));
            return x;
        }

        void KListener_KeyDown(object sender, Keyboard.RawKeyEventArgs args)
        {
            if (args.Key.ToString() == Properties.Settings.Default["hotkey"].ToString() && !(bool)Properties.Settings.Default["alwaysTop"])
            {
                /*StringBuilder wTitle = new StringBuilder(13);
                if (Natives.GetWindowText(Natives.GetForegroundWindow(), wTitle, 13) > 0)
                {
                    if (wTitle.ToString() == "Guild Wars 2")
                    {
                        if (!inGame)
                        {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                IntPtr handle = new WindowInteropHelper(this).Handle;
                                Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Transparent);
                            }));

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                IntPtr gwHandle = Natives.FindWindow(null, "Guild Wars 2");
                                Natives.SetForegroundWindow(gwHandle);
                                inGame = true;
                            }));
                        }
                        /*else if ((bool)Properties.Settings.Default["alwaysTop"])
                        {
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                IntPtr handle = new WindowInteropHelper(this).Handle;
                                SetWindowLong(handle, GWL_ExStyle, WS_EX_Transparent);
                            }));

                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                            {
                                IntPtr gwHandle = FindWindow(null, "Guild Wars 2");
                                SetForegroundWindow(gwHandle);
                                inGame = true;
                            }));
                        }*/
                    /*}
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            IntPtr handle = new WindowInteropHelper(this).Handle;
                            Natives.SetWindowLong(handle, Natives.GWL_ExStyle, Natives.WS_EX_Layered);
                            inGame = false;
                        }));

                        LogWindow.ClickTroughVoid();
                    }
                } */

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    Natives.ShowWindow(handle, 4);
                }));
            }
        }
        
        void KListener_KeyUp(object sender, Keyboard.RawKeyEventArgs args)
        {
            if (args.Key.ToString() == Properties.Settings.Default["hotkey"].ToString() && !(bool)Properties.Settings.Default["alwaysTop"])
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => 
                {
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    Natives.ShowWindow(handle, 6);
                }));
            }
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
                selectedBl = (string) label.Tag;

            var item = sender as MenuItem;
            if (item != null)
                selectedBl = (string) item.Tag;
            
            if(selectedBl == null)
                selectedBl = "Center";

            WvwMatch.Options.active_bl = selectedBl;
            Icons.ItemsSource = WvwMatch.Details.Maps[WvwMatch.Options.blid[selectedBl]].Objectives;

            if (LogWindow != null)
                LogWindow.lblBLTitle.Content = WvwMatch.Options.active_bl_title;

            MainWindow1.InvalidateVisual();
            
            CnvsBlSelection.Visibility = Visibility.Hidden;
        }

        public void ExitApp(object sender, EventArgs e)
        {
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
            var optWindow = new SetOptions(LogWindow, WvwMatch, MainWindow1);
            optWindow.Show();
        }

        private void ShowAboutWin(object sender, EventArgs e)
        {
            var aboutWindow = new About();
            aboutWindow.Show();
        }

        public static Point GetMousePosition() // mouse position relative to screen
        {
            var w32Mouse = new Natives.Win32Point();
            Natives.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            var hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            if (hwndSource != null) hwndSource.AddHook(DragHook);

            Console.WriteLine(CmbbxHomeServerSelection.Items.Count);
            foreach (World_Names_ item in CmbbxHomeServerSelection.Items)
            {
                if (item.id == (int)Properties.Settings.Default["home_server"])
                    CmbbxHomeServerSelection.SelectedItem = item;
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

        private void MainClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWinEvent(_hhook);
            LogWindow.Close();
            Properties.Settings.Default.Save();
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
            if (ChkbxAutoMatchSelect.IsChecked != null && (bool) ChkbxAutoMatchSelect.IsChecked &&
                CmbbxHomeServerSelection.SelectedValue != null)
            {
                Properties.Settings.Default["auto_matchup"] = (bool)ChkbxAutoMatchSelect.IsChecked;
                Properties.Settings.Default["home_server"] = CmbbxHomeServerSelection.SelectedValue;
                Properties.Settings.Default.Save();
                AutoMatchSetActiveMatch();
            }
            // If home-server is set go to BL selection
            else if (CmbbxHomeServerSelection.SelectedValue != null)
            {
                Properties.Settings.Default["home_server"] = CmbbxHomeServerSelection.SelectedValue;
                Properties.Settings.Default.Save();
                AutoMatchSetActiveMatch();
            }
            else if (LstbxMatchSelection.SelectedItem != null)
            {
                WvwMatch.Options.active_match = (string) LstbxMatchSelection.SelectedValue;
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

            var selection = (World_Names_) CmbbxHomeServerSelection.SelectedItem;

            Console.WriteLine(selection.name);

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
