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
        
        //Options      
        string selectedMatch;
        string selectedBorderland;

        int GWL_ExStyle = -20;
        int WS_EX_Transparent = 0x20;
        int WS_EX_Layered = 0x80000;
        bool ResetMatch = false; 
        bool inGame = false;
        bool AlwaysOnTop = false;

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

            rtvWorldNames();
            
            //rtvMatchDetails(null, null);
            rtvMatches();
            rtvObjectiveNames();

            buildMenu();
        }

        public void updatePosition(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (WvwMatch.Details == null)
                return;

            foreach(WvwObjective obj in WvwMatch.Details.maps[3].objectives)
            {
                if (obj.top != 0.0)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        obj.left = this.Width * (obj.left_base / obj.res_width);
                        obj.top = this.Height * (obj.top_base / obj.res_height);
                    }));
                }
            }
        }

        private void onLoad(object sender, RoutedEventArgs e)
        {
            //this.DataContext = WvwMatch;

            
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

            MenuItem options = new MenuItem();
            options.Header = "Options";

            MenuItem opcLbl = new MenuItem();
            opcLbl.Header = "Opacity:";
            options.Items.Add(opcLbl);

            Slider opcSlider = new Slider();
            opcSlider.Maximum = 1.0;
            opcSlider.Value = 0.65;
            opcSlider.Width = 150;
            opcSlider.Minimum = 0.3;
            opcSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(opcSlider_change);
            options.Items.Add(opcSlider);
            
            CheckBox alwsTop = new CheckBox();
            alwsTop.Content = "Always on top";
            alwsTop.IsChecked = (bool?)AlwaysOnTop;
            alwsTop.Click += new RoutedEventHandler(setAlwsTop);
            options.Items.Add(alwsTop);

            mainMenu.Items.Add(options);
            if (selectedMatch != null)
            {
                MenuItem bl_blue = new MenuItem();
                bl_blue.Header = string.Format("Blue Borderland ({0})", WvwMatch.getServerName(selectedMatch, "blue"));
                bl_blue.Tag = "BlueHome";
                bl_blue.Click += new RoutedEventHandler(borderlandSelected);

                MenuItem bl_red = new MenuItem();
                bl_red.Header = string.Format("Red Borderland ({0})", WvwMatch.getServerName(selectedMatch, "red"));
                bl_red.Tag = "RedHome";
                bl_red.Click += new RoutedEventHandler(borderlandSelected);

                MenuItem bl_green = new MenuItem();
                bl_green.Header = string.Format("Green Borderland ({0})", WvwMatch.getServerName(selectedMatch, "green"));
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

        public void setAlwsTop(object sender, EventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
                AlwaysOnTop = true;
            else
                AlwaysOnTop = false;
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

            /*for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
            {
                WvwMatch.Details.maps[i].objectives = JsonConvert.DeserializeObject<List<WvwObjective>>(getJSON(string.Format("Resources/obj_{0}.json", WvwMatch.Details.maps[i].type)));
            }*/
        }

        public void rtvMatches()
        {
            jsonMatches = JsonConvert.DeserializeObject<Matches_>(getJSON("https://api.guildwars2.com/v1/wvw/matches.json"));
            WvwMatch.Match = jsonMatches.wvw_matches;
            jsonMatches = null;
        }
        
        public void rtvMatchDetails(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (selectedMatch == null) return;
            Match_Details = JsonConvert.DeserializeObject<Match_Details_>(getJSON("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=" + selectedMatch));


            if (WvwMatch.Details == null || ResetMatch)
            {
                WvwMatch.Details = Match_Details;
                //FillMap();
                //t2.Start();
                ResetMatch = false;
            }

            // Fill objective names and icons positions
            if(WvwMatch.Details.maps[3].objectives[0].name == null)
            {
                for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
                {
                    WvwMatch.Details.maps[i].objectives = JsonConvert.DeserializeObject<List<WvwObjective>>(getJSON(string.Format("Resources/obj_{0}.json", WvwMatch.Details.maps[i].type)));
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { DataContext = WvwMatch; })); 
            }

            matchCompare();
        }
        
        public void FillMap()
        {
            if (Match_Details.maps == null) return;
            for(int i = 0; i < Match_Details.maps.Count; i++)
            {
                if (Match_Details.maps[i].type == selectedBorderland)
                {
                    for (int m = 0; m < Match_Details.maps[i].objectives.Count; m++)
                    {
                        //changeIcon(Match_Details.maps[i].objectives[m].id, Match_Details.maps[i].objectives[m].owner);
                    }
                }
            }
        }
        
        public void matchCompare()
        {
            for (int i = 0; i < Match_Details.maps.Count; i++)
            {
                for (int m = 0; m < Match_Details.maps[i].objectives.Count; m++)
                {
                    if (WvwMatch.Details.maps[i].objectives[m].owner != Match_Details.maps[i].objectives[m].owner)
                    {
                        WvwMatch.Details.maps[i].objectives[m].owner = Match_Details.maps[i].objectives[m].owner;
                        WvwMatch.Details.maps[i].objectives[m].owner_guild = Match_Details.maps[i].objectives[m].owner_guild;
                        WvwMatch.Details.maps[i].objectives[m].last_change = DateTime.Now;
                        //if (Match_Details.maps[i].type == selectedBorderland)
                            //changeIcon(Match_Details.maps[i].objectives[m].id, Match_Details.maps[i].objectives[m].owner);
                    }
                }
            }
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
        /*
        public void updateTimers(int Objective, string time_left)
        {
            switch (Objective)
            {
                case 1: //overlook
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { overlook_timer.Content = time_left; }));
                    break;
                case 2://Valley
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { valley_timer.Content = time_left; }));
                    break;
                case 3://Lowlands
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { lowlands_timer.Content = time_left; }));
                    break;
                case 4://Golanta
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { golanta_timer.Content = time_left; }));
                    break;
                case 5://Pangloss
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { pangloss_timer.Content = time_left; }));
                    break;
                case 6://Speldan
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { splendan_timer.Content = time_left; }));
                    break;
                case 7://Danelon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { danelon_timer.Content = time_left; }));
                    break;
                case 8://Umberglade
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { umber_timer.Content = time_left; }));
                    break;
                case 9://Stonemist
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { stonemist_timer.Content = time_left; }));
                    break;
                case 10://Rogue
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { rogue_timer.Content = time_left; }));
                    break;
                case 11://Aldon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { aldon_timer.Content = time_left; }));
                    break;
                case 12://Wildcreek
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { wildcreek_timer.Content = time_left; }));
                    break;
                case 13://Jerrifer
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { jerrifer_timer.Content = time_left; }));
                    break;
                case 14://Klovan
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { klovan_timer.Content = time_left; }));
                    break;
                case 15://Langor
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { langor_timer.Content = time_left; }));
                    break;
                case 16://Quentin
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { quentin_timer.Content = time_left; }));
                    break;
                case 17://Mendon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { mendon_timer.Content = time_left; }));
                    break;
                case 18://Anzalias
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { anzalias_timer.Content = time_left; }));
                    break;
                case 19://Ogrewatch
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ogrewatch_timer.Content = time_left; }));
                    break;
                case 20://Veloka
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { veloka_timer.Content = time_left; }));
                    break;
                case 21://Durios
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { durios_timer.Content = time_left; }));
                    break;
                case 22://Bravost
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { bravost_timer.Content = time_left; }));
                    break;
                
                case 23:
                case 37:
                case 46:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { garrison_timer.Content = time_left; }));
                    break;
                case 36:
                case 42:
                case 26:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { se_tower_timer.Content = time_left; }));
                    break;
                case 45:
                case 25:
                case 35:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { sw_tower_timer.Content = time_left; }));
                    break;
                case 47:
                case 38:
                case 30:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { nw_tower_timer.Content = time_left; }));
                    break;
                case 49:
                case 59:
                case 53:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { sw_camp_timer.Content = time_left; }));
                    break;//49: "Bluevale Refuge", sw_camp  59: "Redvale Refuge", sw_camp53: "Greenvale Refuge", sw_camp
                case 52:
                case 48:
                case 58:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { nw_camp_timer.Content = time_left; }));
                    break;//52: "Arah’s Hope", nw_camp      48: "Faithleap",  nw_camp58: "Godslore", nw_camp
                case 57:
                case 28:
                case 40:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ne_tower_timer.Content = time_left; }));
                    break;//57: "Cragtop",  e_tower         28: "Dawn’s Eyrie",  e_tower40: "Cliffside",  e_tower
                case 50:
                case 55:
                case 61:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { se_camp_timer.Content = time_left; }));
                    break;//50: "Bluewater Lowlands", se_camp55: "Redwater Lowlands", se_camp61: "Greenwater Lowlands", se_camp
                case 54:
                case 51:
                case 60:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ne_camp_timer.Content = time_left; }));
                    break;//54: "Foghaven", ne_camp         51: "Astralholme", ne_camp60: "Stargrove", ne_camp
                case 33:
                case 44:
                case 27:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { w_keep_timer.Content = time_left; }));
                    break;//27: "Ascension Bay", w_castle   33: "Dreaming Bay",w_castle44: "Dreadfall Bay",w_castle
                case 56:
                case 39:
                case 29:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { n_camp_timer.Content = time_left; }));
                    break;//29: "The Spiritholme", n_camp   39: "The Godsword",  n_camp56: "The Titanpaw", n_camp
                case 24:
                case 34:
                case 43:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { s_camp_timer.Content = time_left; }));
                    break;//24: "Champion’s demense", 	s_camp  34: "Victors’s Lodge",  	s_camp43: "Hero’s Lodge", 		s_camp
                case 31:
                case 32:
                case 41://Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {               }));
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { e_keep_timer.Content = time_left; }));
                    break;//31: "Askalion Hills", 		e_castle    32: "Etheron Hills", 		e_castle41: "Shadaran Hills",  		e_castle
            }
        }

        public void updateTimers(Object source, System.Timers.ElapsedEventArgs e)
        {

            DateTime cur = DateTime.Now;

            for (int i = 0; i < WvwMatch.Details.maps.Count; i++)
            {
                for (int m = 0; m < WvwMatch.Details.maps[i].objectives.Count; m++)
                {
                    if (WvwMatch.Details.maps[i].type == selectedBorderland)
                    {
                        TimeSpan diff = cur.Subtract(WvwMatch.Details.maps[i].objectives[m].last_change);
                        TimeSpan left = TimeSpan.FromMinutes(5) - diff;
                        if (diff < TimeSpan.FromMinutes(5)) { updateTimers(WvwMatch.Details.maps[i].objectives[m].id, left.ToString(@"mm\:ss")); } else { updateTimers(WvwMatch.Details.maps[i].objectives[m].id, " "); }
                    }
                }
            }
            
        }

        public void changeIcon(int Objective, string color)
        { 
            switch (Objective)
            {
                case 1: //overlook
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { overlook_icon.Source = getPNG("keep", color); }));
                    break;
                case 2://Valley
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { valley_icon.Source = getPNG("keep", color); }));
                    break;
                case 3://Lowlands
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { lowlands_icon.Source = getPNG("keep", color); }));
                    break;
                case 4://Golanta
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { golanta_icon.Source = getPNG("camp", color); }));
                    break;
                case 5://Pangloss
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { pangloss_icon.Source = getPNG("camp", color); }));
                    break;
                case 6://Speldan
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { splendan_icon.Source = getPNG("camp", color); }));
                    break;
                case 7://Danelon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { danelon_icon.Source = getPNG("camp", color); }));
                    break;
                case 8://Umberglade
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { umber_icon.Source = getPNG("camp", color); }));
                    break;
                case 9://Stonemist
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { stonemist_icon.Source = getPNG("castle", color); }));
                    break;
                case 10://Rogue
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { rogue_icon.Source = getPNG("camp", color); }));
                    break;
                case 11://Aldon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { aldon_icon.Source = getPNG("tower", color); }));
                    break;
                case 12://Wildcreek
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { wildcreek_icon.Source = getPNG("tower", color); }));
                    break;
                case 13://Jerrifer
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { jerrifer_icon.Source = getPNG("tower", color); }));
                    break;
                case 14://Klovan
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { klovan_icon.Source = getPNG("tower", color); }));
                    break;
                case 15://Langor
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { langor_icon.Source = getPNG("tower", color); }));
                    break;
                case 16://Quentin
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { quentin_icon.Source = getPNG("tower", color); }));
                    break;
                case 17://Mendon
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { mendon_icon.Source = getPNG("tower", color); }));
                    break;
                case 18://Anzalias
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { anzalias_icon.Source = getPNG("tower", color); }));
                    break;
                case 19://Ogrewatch
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { ogrewatch_icon.Source = getPNG("tower", color); }));
                    break;
                case 20://Veloka
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { veloka_icon.Source = getPNG("tower", color); }));
                    break;
                case 21://Durios
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { durios_icon.Source = getPNG("tower", color); }));
                    break;
                case 22://Bravost
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { bravost_icon.Source = getPNG("tower", color); }));
                    break;
                case 23:
                case 37:
                case 46:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => { garrison_icon.Source = getPNG("keep", color); }));
                    break;
                case 36:
                case 42: 
                case 26:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {se_tower_icon.Source = getPNG("tower", color);  }));
                    break;
                case 45:
                case 25:
                case 35:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {sw_tower_icon.Source = getPNG("tower", color);  }));
                    break;
                case 47:
                case 38:
                case 30:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {nw_tower_icon.Source = getPNG("tower", color);  }));
                    break;
                case 49:
                case 59:
                case 53:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {sw_camp_icon.Source = getPNG("camp", color);  }));
                    break;//49: "Bluevale Refuge", sw_camp  59: "Redvale Refuge", sw_camp53: "Greenvale Refuge", sw_camp
                case 52:
                case 48:
                case 58:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {nw_camp_icon.Source = getPNG("camp", color);  }));
                    break;//52: "Arah’s Hope", nw_camp      48: "Faithleap",  nw_camp58: "Godslore", nw_camp
                case 57:
                case 28:
                case 40:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {ne_tower_icon.Source = getPNG("tower", color);  }));
                    break;//57: "Cragtop",  e_tower         28: "Dawn’s Eyrie",  e_tower40: "Cliffside",  e_tower
                case 50:
                case 55:
                case 61:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {se_camp_icon.Source = getPNG("camp", color);  }));
                    break;//50: "Bluewater Lowlands", se_camp55: "Redwater Lowlands", se_camp61: "Greenwater Lowlands", se_camp
                case 54:
                case 51:
                case 60:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {ne_camp_icon.Source = getPNG("camp", color);  }));
                    break;//54: "Foghaven", ne_camp         51: "Astralholme", ne_camp60: "Stargrove", ne_camp
                case 33:
                case 44:
                case 27:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {w_keep_icon.Source = getPNG("keep", color);  }));
                    break;//27: "Ascension Bay", w_castle   33: "Dreaming Bay",w_castle44: "Dreadfall Bay",w_castle
                case 56:
                case 39:
                case 29:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {n_camp_icon.Source = getPNG("camp", color);  }));
                    break;//29: "The Spiritholme", n_camp   39: "The Godsword",  n_camp56: "The Titanpaw", n_camp
                case 24:
                case 34:
                case 43:
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {s_camp_icon.Source = getPNG("camp", color);  }));
                    break;//24: "Champion’s demense", 	s_camp  34: "Victors’s Lodge",  	s_camp43: "Hero’s Lodge", 		s_camp
                case 31:
                case 32:
                case 41://Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {               }));
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {e_keep_icon.Source = getPNG("keep", color);  }));
                    break;//31: "Askalion Hills", 		e_castle    32: "Etheron Hills", 		e_castle41: "Shadaran Hills",  		e_castle
            }


        
        }
        */
        public string getJSON(string file)
        {
            string s;
            if (file.StartsWith("http"))
            {
                using (WebClient client = new WebClient())
                {
                    s = client.DownloadString(@file);
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
            if (args.Key.ToString() == "Home")
            {
                StringBuilder wTitle = new StringBuilder(13);
                if (GetWindowText(GetForegroundWindow(), wTitle, 13) > 0 && !AlwaysOnTop)
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
            if (args.Key.ToString() == "Home" && !AlwaysOnTop)
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
            if (selectedMatch != (string)((MenuItem)sender).Tag)
                ResetMatch = true;

            selectedMatch = (string)((MenuItem)sender).Tag;
            buildMenu();
            ContextMenu.IsOpen = true;
        }

        public void borderlandSelected(object sender, EventArgs e)
        {
            string selectedBL = (string)((MenuItem)sender).Tag;
            WvwMatch.Options.active_bl = selectedBL;
            
            if (selectedBL != "Center")
            {
                /*if (selectedBL == "RedHome")
                    lbl_borderlands.Content = "Red Borderlands";
                else if (selectedBL == "GreenHome")
                    lbl_borderlands.Content = "Green Borderlands";
                else
                    lbl_borderlands.Content = "Blue Borderlands";

                */
                //MainWindow1.Height = 771.637;
                //MainWindow1.Width = 580;
                //map_canvas_eb.Visibility = Visibility.Hidden;
                //map_canvas_bl.Visibility = Visibility.Visible;
            }
            else
            {
               // MainWindow1.Height = 650;
                //MainWindow1.Width = 650;

                //map_canvas_bl.Visibility = Visibility.Hidden;
                //map_canvas_eb.Visibility = Visibility.Visible;
                
            }
            selectedBorderland = selectedBL;

            FillMap();
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
