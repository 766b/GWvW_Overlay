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
using System.Windows.Interop;
using System.Threading;
using System.Windows.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;

using System.Runtime.InteropServices;
using System.Windows.Resources;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Keyboard.KeyboardListener KListener = new Keyboard.KeyboardListener();
        
        int GWL_ExStyle = -20;
        int WS_EX_Transparent = 0x20;
        int WS_EX_Layered = 0x80000;
        bool ResetMatch = false; 
        bool inGame = false;

        private bool? _adjustingHeight = null;
        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }

        System.Timers.Timer t1 = new System.Timers.Timer();
        System.Timers.Timer t2 = new System.Timers.Timer();
        System.Timers.Timer t3 = new System.Timers.Timer();

        //JSON Data
        Match_Details_ Match_Details = new Match_Details_();
        ObjectiveNames_ ObjectiveNames = new ObjectiveNames_();
        WvwMatch_ WvwMatch = new WvwMatch_();
        Matches_ jsonMatches = new Matches_();


        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNew);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmd);
        
        public MainWindow()
        {
            InitializeComponent();
            this.SourceInitialized += Window_SourceInitialized;

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

            DateTime cur = DateTime.Now;

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
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.maps[map].objectives[obj].time_left = left.ToString(@"mm\:ss");
                        }));
                    } 
                    else 
                    {
                         Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            WvwMatch.Details.maps[map].objectives[obj].time_left = " ";
                        }));
                    }
                }
            }
        }  

        public void rtvWorldNames()
        {
            WvwMatch.World = JsonConvert.DeserializeObject<List<World_Names_>>(getJSON(@"https://api.guildwars2.com/v1/world_names.json"));
        }

        public void rtvObjectiveNames()
        {
            ObjectiveNames = JsonConvert.DeserializeObject<ObjectiveNames_>(getJSON(@"Resources/objectives.json"));
            WvwMatch.ObjectiveNames = ObjectiveNames.wvw_objectives;
            ObjectiveNames.wvw_objectives = null;
        }

        public void rtvMatches()
        {
            jsonMatches = JsonConvert.DeserializeObject<Matches_>(getJSON("https://api.guildwars2.com/v1/wvw/matches.json"));
            WvwMatch.Match = jsonMatches.wvw_matches;
            jsonMatches = null;
        }
        
        public void rtvMatchDetails(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Options.active_match == null)
                return;

            Match_Details = JsonConvert.DeserializeObject<Match_Details_>(getJSON("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=" + WvwMatch.Options.active_match));

            if (WvwMatch.Details == null || ResetMatch)
            {
                WvwMatch.Details = Match_Details;
                ResetMatch = false;
                //Console.WriteLine(WvwMatch.Details.maps[3].objectives[0].ObjData.type + " : " + WvwMatch.Details.maps[3].objectives[0].ObjData.type);
                WvwMatch.GetBLID();
            }
            else
            { 
                WvwMatch.Details.match_id = Match_Details.match_id;
                WvwMatch.Details.scores = Match_Details.scores;
                for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
                {
                    WvwMatch.Details.maps[i].scores = Match_Details.maps[i].scores;

                    for (int m = 0; m < Match_Details.maps[i].objectives.Count; m++)
                    {
                        if (WvwMatch.Details.maps[i].objectives[m].owner != Match_Details.maps[i].objectives[m].owner)
                        {
                            WvwMatch.Details.maps[i].objectives[m].owner = Match_Details.maps[i].objectives[m].owner;
                            WvwMatch.Details.maps[i].objectives[m].owner_guild = Match_Details.maps[i].objectives[m].owner_guild;
                            WvwMatch.Details.maps[i].objectives[m].last_change = DateTime.Now;
                        }
                    }
                }

            }

            // Fill objective names and icons positions
            if(WvwMatch.Details.maps[3].objectives[0].ObjData.name == null)
            {
                for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
                {
                    Console.WriteLine(WvwMatch.Details.maps[3].objectives[0].owner + " : " + WvwMatch.Details.maps[3].objectives[0].ObjData.type);
                    var ObjData = JsonConvert.DeserializeObject<List<WvwObjective>>(getJSON(string.Format("Resources/obj_{0}.json", WvwMatch.Details.maps[i].type)));
                    foreach (var obj in ObjData)
                    {
                        for (int y = 0; y < WvwMatch.Details.maps[i].objectives.Count; y++)
                        {
                            if (obj.id == WvwMatch.Details.maps[i].objectives[y].id)
                                WvwMatch.Details.maps[i].objectives[y].ObjData = obj;
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

        public string getJSON(string file)
        {
            string s;
            if (file.StartsWith("http"))
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        s = client.DownloadString(@file);
                    }
                    catch (WebException e)
                    {
                        throw e;
                    }
                }
            }
            else
            {
                Uri uri = new Uri(file, UriKind.Relative);
                StreamResourceInfo contentStream = Application.GetContentStream(uri);
                s = contentStream.ToString();
                StreamReader sr = new StreamReader(contentStream.Stream);
                s = sr.ReadToEnd();
            }
            
            return s;
        }

        void KListener_KeyDown(object sender, Keyboard.RawKeyEventArgs args)
        {
            if (args.Key.ToString() == Properties.Settings.Default["hotkey"].ToString() && !(bool)Properties.Settings.Default["alwaysTop"])
            {
                StringBuilder wTitle = new StringBuilder(13);
                if (GetWindowText(GetForegroundWindow(), wTitle, 13) > 0)
                {
                    if (wTitle.ToString() == "Guild Wars 2")
                    {
                        if (!inGame)
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
                    }
                    else
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            IntPtr handle = new WindowInteropHelper(this).Handle;
                            SetWindowLong(handle, GWL_ExStyle, WS_EX_Layered);
                            inGame = false;
                        }));
                    }
                } 

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    ShowWindow(handle, 4);
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
                    ShowWindow(handle, 6);
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
            this.InvalidateVisual();
        }

        public void exitApp(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
        }

        private void showOptionsWindow(object sender, EventArgs e)
        {
            SetOptions optWindow = new SetOptions();
            optWindow.Show();

            WvwMatch.Details.maps[3].objectives[7].owner_guild = "3213123";
        }

        private void opcSlider_change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider x = (Slider)sender;
            MainWindow1.Opacity = x.Value;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
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
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
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
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
