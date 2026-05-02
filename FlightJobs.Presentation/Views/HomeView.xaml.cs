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
        }

        public static void DisableTabControl()
        {
            for (int i = 0; i < TabHome.Items.Count; i++)
            {
                ((TabItem)TabHome.Items[i]).IsEnabled = false;
            }            
        }

        public static void EnableTabControl()
        {
            for (int i = 0; i < TabHome.Items.Count; i++)
            {
                ((TabItem)TabHome.Items[i]).IsEnabled = true;
            }
        }
    }
}
