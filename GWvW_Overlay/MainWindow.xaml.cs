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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using System.Threading;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        static WinEventDelegate procDelegate = new WinEventDelegate(WinEventProc);

        Keyboard.KeyboardListener KListener = new Keyboard.KeyboardListener();
        IntPtr hhook;

        static MainWindow handle_this;

        bool ResetMatch = false;
        static bool inGame = false;
        private bool? _adjustingHeight = null;

        System.Timers.Timer t1 = new System.Timers.Timer();
        System.Timers.Timer t2 = new System.Timers.Timer();
        System.Timers.Timer t3 = new System.Timers.Timer();

        //JSON Data
        Match_Details_ Match_Details = new Match_Details_();
        ObjectiveNames_ ObjectiveNames = new ObjectiveNames_();
        WvwMatch_ WvwMatch = new WvwMatch_();
        Matches_ jsonMatches = new Matches_();

        Utils Utils = new Utils();
        Guild GuildData = new Guild();

        CampLogger LogWindow = new CampLogger();

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
            StringBuilder wTitle = new StringBuilder(13);
            Natives.GetWindowText(hwnd, wTitle, 13);

            if (wTitle.ToString() == "Guild Wars 2" && inGame != true)
            {
                inGame = true;
                handle_this.ClickTroughActivate();
                //Console.WriteLine("Setting click-trough");              
            }
            else if (wTitle.ToString() != "Guild Wars 2" && inGame == true)
            {
                inGame = false;
                handle_this.ClickTroughVoid();
                //Console.WriteLine("Voiding click-trough");                
            }

            
            
        }
        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += Window_SourceInitialized;

            hhook = SetWinEventHook(Natives.EVENT_SYSTEM_FOREGROUND, Natives.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                procDelegate, 0, 0, Natives.WINEVENT_SKIPOWNPROCESS); // | Natives.WINEVENT_SKIPOWNPROCESS

            MainWindow1.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            
            
            KListener.KeyDown += new Keyboard.RawKeyEventHandler(KListener_KeyDown);
            KListener.KeyUp += new Keyboard.RawKeyEventHandler(KListener_KeyUp);

            t1.Interval = 4000;
            t1.Elapsed += new System.Timers.ElapsedEventHandler(rtvMatchDetails);
            t1.Start();

            t2.Interval = 1000;
            t2.Elapsed += new System.Timers.ElapsedEventHandler(updatePosition);
            t2.Start();
            

            t3.Interval = 1000;
            t3.Elapsed += new System.Timers.ElapsedEventHandler(updateTimers);

            handle_this = this;

            rtvMatchDetails(null, null);
            rtvWorldNames();
            rtvMatches();
            rtvObjectiveNames();

            buildMenu();
        }

        public void updatePosition(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Details == null)
                return;

            foreach (Map map in WvwMatch.Details.maps)
            {
                foreach (Objective obj in map.objectives)
                {
                    DateTime cur = DateTime.Now;

                    if (obj.ObjData.top != 0.0)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            obj.ObjData.left = this.Width * (obj.ObjData.left_base / obj.ObjData.res_width);
                            obj.ObjData.top = this.Height * (obj.ObjData.top_base / obj.ObjData.res_height);
                        }));
                    }
                }
            }
        }

        private void onLoad(object sender, RoutedEventArgs e)
        {
            DataContext = WvwMatch;

            LogWindow.Show();
            if (!(bool)Properties.Settings.Default["show_tracker"])
                LogWindow.Hide();
        }

        public void buildMenu()
        {
            ContextMenu mainMenu = new ContextMenu();

            MenuItem matches = new MenuItem();
            matches.Header = "Matches";
            var y = WvwMatch.getMatchesList();
            foreach (var x in y)
            {
                MenuItem i = new MenuItem();
                i.Header = x.Value;
                i.Tag = x.Key;
                i.Click += new RoutedEventHandler(matchSelected);
                matches.Items.Add(i);
            }
            mainMenu.Items.Add(matches);

            MenuItem menu_options = new MenuItem();
            menu_options.Header = "Options";
            menu_options.Click += new RoutedEventHandler(showOptionsWindow);
            mainMenu.Items.Add(menu_options);

            if (WvwMatch.Options.active_match != null)
            {
                MenuItem bl_blue = new MenuItem();
                bl_blue.Header = string.Format("Blue Borderland ({0})", WvwMatch.getServerName("blue"));
                bl_blue.Tag = "BlueHome";
                bl_blue.Click += new RoutedEventHandler(borderlandSelected);

                MenuItem bl_red = new MenuItem();
                bl_red.Header = string.Format("Red Borderland ({0})", WvwMatch.getServerName("red"));
                bl_red.Tag = "RedHome";
                bl_red.Click += new RoutedEventHandler(borderlandSelected);

                MenuItem bl_green = new MenuItem();
                bl_green.Header = string.Format("Green Borderland ({0})", WvwMatch.getServerName("green"));
                bl_green.Tag = "GreenHome";
                bl_green.Click += new RoutedEventHandler(borderlandSelected);

                MenuItem bl_eb = new MenuItem();
                bl_eb.Header = "Eternal Battleground";
                bl_eb.Tag = "Center";
                bl_eb.Click += new RoutedEventHandler(borderlandSelected);

                mainMenu.Items.Add(new Separator());
                
                mainMenu.Items.Add(bl_blue);
                mainMenu.Items.Add(bl_green);
                mainMenu.Items.Add(bl_red);
                mainMenu.Items.Add(bl_eb);
                
            }
            MenuItem exit_app = new MenuItem();
            exit_app.Header = "Exit";
            exit_app.Click += new RoutedEventHandler(exitApp);
            mainMenu.Items.Add(exit_app);
            this.ContextMenu = mainMenu;
        }

        public void updateTimers(Object source, System.Timers.ElapsedEventArgs e)
        {

            if (WvwMatch.Details == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { LogWindow.ResetText("camp"); }));
            DateTime cur = DateTime.Now;
            string eventCamps = "";

            for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
            {
                int map = i;

                for (int m = 0; m < WvwMatch.Details.maps[map].objectives.Count; m++)
                {
                    int obj = m;

                    TimeSpan diff = cur.Subtract(WvwMatch.Details.maps[map].objectives[obj].last_change);
                    TimeSpan left = TimeSpan.FromMinutes(5) - diff;
                    if (diff < TimeSpan.FromMinutes(5)) 
                    {
                        if (WvwMatch.Details.maps[map].objectives[obj].ObjData.type == "camp" && WvwMatch.Options.active_bl == WvwMatch.Details.maps[map].type)
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
                            eventCamps += string.Format("{0}\t{1}\n", left.ToString(@"mm\:ss"), WvwMatch.Details.maps[map].objectives[obj].ObjData.name);
                        }

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.maps[map].objectives[obj].time_left = left.ToString(@"mm\:ss");
                        }));
                    } 
                    else 
                    {

                        if (WvwMatch.Details.maps[map].objectives[obj].ObjData.type == "camp" && WvwMatch.Options.active_bl == WvwMatch.Details.maps[map].type)
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
                            eventCamps += string.Format("{0}\t{1}\n", "N/A", WvwMatch.Details.maps[map].objectives[obj].ObjData.name);
                        }
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.maps[map].objectives[obj].time_left = " ";
                        }));
                    }
                }
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                LogWindow.txtblk_eventCamp.Text = eventCamps;
            }));
        }  

        public void rtvWorldNames()
        {
            WvwMatch.World = JsonConvert.DeserializeObject<List<World_Names_>>(Utils.getJSON(@"https://api.guildwars2.com/v1/world_names.json"));
        }

        public void rtvObjectiveNames()
        {
            ObjectiveNames = JsonConvert.DeserializeObject<ObjectiveNames_>(Utils.getJSON(@"Resources/objectives.json"));
            WvwMatch.ObjectiveNames = ObjectiveNames.wvw_objectives;
            ObjectiveNames.wvw_objectives = null;
        }

        public void rtvMatches()
        {
            jsonMatches = JsonConvert.DeserializeObject<Matches_>(Utils.getJSON("https://api.guildwars2.com/v1/wvw/matches.json"));
            WvwMatch.Match = jsonMatches.wvw_matches;
            jsonMatches = null;
        }
        
        public void rtvMatchDetails(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Options.active_match == null)
                return;

            Match_Details = JsonConvert.DeserializeObject<Match_Details_>(Utils.getJSON("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=" + WvwMatch.Options.active_match));

            if (WvwMatch.Details == null || ResetMatch)
            {
                LogWindow.ResetText();
                WvwMatch.Details = Match_Details;
                ResetMatch = false;
                WvwMatch.GetBLID();
            }
            else
            { 
                WvwMatch.Details.match_id = Match_Details.match_id;
                WvwMatch.Details.scores = Match_Details.scores;
                for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
                {
                    int map = i;
                    WvwMatch.Details.maps[map].scores = Match_Details.maps[map].scores;

                    for (int m = 0; m < Match_Details.maps[map].objectives.Count; m++)
                    {
                        int obj = m;

                        //Caching Guild info
                        if(Match_Details.maps[map].objectives[obj].owner_guild != null)
                        {
                            GuildData.getGuildByID(Match_Details.maps[map].objectives[obj].owner_guild);
                        }

                        if (WvwMatch.Details.maps[map].objectives[obj].owner != Match_Details.maps[map].objectives[obj].owner)
                        {
                            if (WvwMatch.Options.active_bl == WvwMatch.Details.maps[map].type)
                            {
                                Dictionary<string, string> dict = new Dictionary<string, string>() 
                                    { 
                                        {"time", DateTime.Now.ToString("t")},
                                        {"objective", WvwMatch.Details.maps[map].objectives[obj].ObjData.name},
                                        {"from", WvwMatch.getServerName(WvwMatch.Details.maps[map].objectives[obj].owner)},
                                        {"from_color", WvwMatch.Details.maps[map].objectives[obj].owner},
                                        {"to", WvwMatch.getServerName(Match_Details.maps[map].objectives[obj].owner)},
                                        {"to_color", Match_Details.maps[map].objectives[obj].owner},
                                    };
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    LogWindow.AddEventLog(dict, false);
                                }));
                            }

                            WvwMatch.Details.maps[map].objectives[obj].owner = Match_Details.maps[map].objectives[obj].owner;
                            WvwMatch.Details.maps[map].objectives[obj].owner_guild = Match_Details.maps[map].objectives[obj].owner_guild;
                            WvwMatch.Details.maps[map].objectives[obj].last_change = DateTime.Now;
                        }
                        if (WvwMatch.Details.maps[map].objectives[obj].owner_guild != Match_Details.maps[map].objectives[obj].owner_guild &&
                            WvwMatch.Details.maps[map].objectives[obj].owner == Match_Details.maps[map].objectives[obj].owner)
                        {
                            WvwMatch.Details.maps[map].objectives[obj].owner_guild = Match_Details.maps[map].objectives[obj].owner_guild;

                            if (WvwMatch.Options.active_bl == WvwMatch.Details.maps[map].type)
                            {
                                var GuildInfo = GuildData.getGuildByID(WvwMatch.Details.maps[map].objectives[obj].owner_guild);
                                Dictionary<string, string> dict = new Dictionary<string, string>() 
                                { 
                                    {"time", DateTime.Now.ToString("t")},
                                    {"objective", WvwMatch.Details.maps[map].objectives[obj].ObjData.name},
                                    {"owner_color", WvwMatch.Details.maps[map].objectives[obj].owner}
                                };
                                if (GuildInfo == null)
                                    dict.Add("owner", "released"); 
                                else
                                    dict.Add("owner", string.Format("[{1}] {0}", GuildInfo[0], GuildInfo[1]));

                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    LogWindow.AddEventLog(dict, true);
                                }));
                            }
                            
                        }

                    }
                }

            }

            // Fill objective names and icons positions
            if(WvwMatch.Details.maps[3].objectives[0].ObjData.name == null)
            {
                for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
                {
                    int map = i;
                    var ObjData = JsonConvert.DeserializeObject<List<WvwObjective>>(Utils.getJSON(string.Format("Resources/obj_{0}.json", WvwMatch.Details.maps[map].type)));
                    foreach (var obj in ObjData)
                    {
                        for (int y = 0; y < WvwMatch.Details.maps[map].objectives.Count; y++)
                        {
                            int objct = y;
                            if (obj.id == WvwMatch.Details.maps[map].objectives[objct].id)
                                WvwMatch.Details.maps[map].objectives[objct].ObjData = obj;
                        }
                    }
                }
            }

            t3.Start();
        }

        public ImageSource getPNG(string type, string color)
        {
            string y;
            if (color == "none")
            {
                y = string.Format("Resources/{0}.png", type);
            }
            else
            {
                y = string.Format("Resources/{0}_{1}.png", type, color.ToLower());
                
            }
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

        public void matchSelected(object sender, EventArgs e)
        {
            if (WvwMatch.Options.active_match != (string)((MenuItem)sender).Tag)
            {
                ResetMatch = true;
                Icons.ItemsSource = null;
            }

            WvwMatch.Options.active_match = (string)((MenuItem)sender).Tag;
            rtvMatchDetails(null, null);
            buildMenu();
            ContextMenu.IsOpen = true;

        }

        public void borderlandSelected(object sender, EventArgs e)
        {
            string selectedBL = (string)((MenuItem)sender).Tag;
            WvwMatch.Options.active_bl = selectedBL;
            Icons.ItemsSource = WvwMatch.Details.maps[WvwMatch.Options.blid[selectedBL]].objectives;

            if (LogWindow != null)
                LogWindow.lblBLTitle.Content = WvwMatch.Options.active_bl_title;

            this.InvalidateVisual();
        }

        public void exitApp(object sender, EventArgs e)
        {
            UnhookWinEvent(hhook);
            LogWindow.Close();
            Application.Current.Shutdown();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
        }

        private void showOptionsWindow(object sender, EventArgs e)
        {
            SetOptions optWindow = new SetOptions(LogWindow);
            optWindow.Show();
        }

        private void opcSlider_change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider x = (Slider)sender;
            MainWindow1.Opacity = x.Value;
        }

        

        public static Point GetMousePosition() // mouse position relative to screen
        {
            Natives.Win32Point w32Mouse = new Natives.Win32Point();
            Natives.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            double _aspectRatio = WvwMatch.Options.width / WvwMatch.Options.height;
            switch ((Natives.WM)msg)
            {
                case Natives.WM.WINDOWPOSCHANGING:
                    {
                        Natives.WINDOWPOS pos = (Natives.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(Natives.WINDOWPOS));

                        if ((pos.flags & (int)Natives.SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

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
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

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

        private void mainClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookWinEvent(hhook);
            LogWindow.Close();
            Properties.Settings.Default.Save();
        }
    }
}
