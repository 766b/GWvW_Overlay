using GWvW_Overlay.Annotations;
using GWvW_Overlay.Resources.Lang;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace GWvW_Overlay
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            var abinfo = new AppInfo();
            this.Title = Strings.about;
            DataContext = abinfo;
        }

        private void HyperlinkNavigate([NotNull] object sender, [NotNull] RequestNavigateEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }

    public class AppInfo
    {
        public string Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

                string version = assembly.GetName().Version.ToString();

                return version;
            }
        }
    }

}
