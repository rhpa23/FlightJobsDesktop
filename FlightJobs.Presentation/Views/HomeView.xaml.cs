using FlightJobs.Infrastructure;
using FlightJobsDesktop.Views.Home;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        internal static TabControl TabHome { get; set; }
        internal static Ellipse DebtAirlineEllipse { get; set; }

        public HomeView()
        {
            InitializeComponent();
            TabHome = TabControlHome;
            DebtAirlineEllipse = EllipseAirlines;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl && ((TabControl)e.Source).SelectedContent is ManagerJobsView)
            {
                var managerView = (ManagerJobsView) ((TabControl)e.Source).SelectedContent;
                managerView.LoadManagerView();
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SetEllipseAirlinesVIsibility();
            MainWindow.SetLicenseOverdueEllipseVisibility();
        }

        internal static void SetEllipseAirlinesVIsibility()
        {
            DebtAirlineEllipse.Visibility = AppProperties.UserStatistics.Airline?.DebtValue == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
    }
}
