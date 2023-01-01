using ControlzEx.Theming;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void ThemeSwitch_Toggled(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ThemeSwitch.IsOn)
            {
                ThemeManager.Current.ChangeTheme(((App)Application.Current), "Dark.Blue");
            }
            else
            {
                ThemeManager.Current.ChangeTheme(((App)Application.Current), "Light.Blue");
            }
        }
    }
}
