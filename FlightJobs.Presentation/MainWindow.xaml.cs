using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views;
using FlightJobsDesktop.Views.Account;
using FlightJobsDesktop.Views.Modals;
using log4net;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FlightJobsDesktop
{
    /// <summary>
    /// Lógica interna para Test2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotificationManager _notificationManager;
        internal static NavigationView NavigationBar { get; set; }
        internal static DockPanel _loadingPanel;
        internal static StackPanel _loadingProgressPanel;
        private static int _loadingCount;
        internal static Ellipse LicenseOverdueEllipse { get; set; }

        private FlightJobsConnectSim _flightJobsConnectSim = new FlightJobsConnectSim();
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public static IAbstractFactory<IJobService> JobServiceFactory;
        public static IAbstractFactory<IUserAccessService> UserServiceFactory;
        public static IAbstractFactory<IInfraService> InfraServiceFactory;
        public static IAbstractFactory<IAirlineService> AirlineServiceFactory;
        public static IAbstractFactory<ISqLiteDbContext> SqLiteContextFactory;
        public static IAbstractFactory<IPilotService> PilotServiceFactory;

        private UserSettingsViewModel _userSettings;

        public MainWindow(IAbstractFactory<IInfraService> factoryInfra,
                          IAbstractFactory<IJobService> factoryJob, 
                          IAbstractFactory<IUserAccessService> factoryUser,
                          IAbstractFactory<IPilotService> factoryPilot,
                          IAbstractFactory<IAirlineService> factoryAirline,
                          IAbstractFactory<ISqLiteDbContext> factorySqLiteContext)
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();

            JobServiceFactory = factoryJob;
            UserServiceFactory = factoryUser;
            PilotServiceFactory = factoryPilot;
            InfraServiceFactory = factoryInfra;
            AirlineServiceFactory = factoryAirline;
            SqLiteContextFactory = factorySqLiteContext;

            ResizeMode = ResizeMode.CanResizeWithGrip;

            NavigationBar = nvMain;
            LicenseOverdueEllipse = EllipseLicense;
            _loadingPanel = LoadingPanel;
            _loadingProgressPanel = LoadingProgressPanel;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("favicon-ok.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    ShowMainWindow();
                };
            var miShow = new System.Windows.Forms.MenuItem() { Text = "Show" };
            miShow.Click += delegate (object sender, EventArgs args) { ShowMainWindow(); };
            var miExit = new System.Windows.Forms.MenuItem() { Text = "Exit" };
            miExit.Click += delegate (object sender, EventArgs args) { Application.Current.Shutdown(); };
            
            ni.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { miShow, miExit });
            _log.Info("Initialized");
        }

        private void ShowModal(string title, object content)
        {

            Window window = new Window
            {
                Title = title,
                Content = content,
                Width = ((UserControl)content).MinWidth,
                Height = ((UserControl)content).MinHeight + 40,
                //SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.ToolWindow,
                Topmost = true,
            };

            window.ShowDialog();
        }

        private async void LoadData()
        {
            ShowLoading();
            try
            {
                LoadSettingsFromFile();

                await JobServiceFactory.Create().GetAllUserJobs(AppProperties.UserLogin.UserId);
                await UserServiceFactory.Create().LoadUserStatisticsProperties(AppProperties.UserLogin.UserId);

                contentFrame.Navigate(typeof(HomeView));
                nvMain.SelectedItem = HomeViewPageItem;

                LoadAllSettingsData();

                _flightJobsConnectSim.Initialize();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
                HideLoading();
                ShowModal("Select Host", new SelectHostUrlModal(_userSettings));
                LoadData();
            }
            finally
            {
                HideLoading();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            ShowInTaskbar = false;
            e.Cancel = true;

            _notificationManager.ShowButtonWindow("FlightJobs still running in the system tray.");
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        public static void ShowLoading(bool hideProgressPanel = false)
        {
            _loadingPanel.Visibility = Visibility.Visible;
            _loadingProgressPanel.Visibility = hideProgressPanel ? Visibility.Collapsed : Visibility.Visible;
            _loadingCount++;
        }

        public static void HideLoading()
        {
            _loadingCount--;

            if (_loadingCount <= 0)
            {
                _loadingPanel.Visibility = Visibility.Collapsed;
                _loadingCount = 0;
            }
        }

        private void NavigateToPageControl(string pageName)
        {
            Type pageType = typeof(HomeView).Assembly.GetType(pageName);
            contentFrame.Navigate(pageType);
        }

        private void LoadSettingsFromFile()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var jsonSettings = File.ReadAllText(System.IO.Path.Combine(path, "ResourceData\\Settings.json"));
            _userSettings = JsonConvert.DeserializeObject<UserSettingsViewModel>(jsonSettings);

            var infraService = MainWindow.InfraServiceFactory.Create();
            var selectHost = new SelectHostViewModel();

            switch (_userSettings.SelectedHostOption)
            {
                case 1:
                    infraService.SetApiUrl(selectHost.Option1HostUrl);
                    break;
                case 2:
                    infraService.SetApiUrl(selectHost.Option2HostUrl);
                    break;
                case 3:
                    infraService.SetApiUrl(selectHost.Option3HostUrl);
                    break;
                case 4:
                    infraService.SetApiUrl(selectHost.Option4HostUrl);
                    break;
                default:
                    break;
            }
        }

        private void LoadAllSettingsData()
        {
            ThemeManager.Current.ApplicationTheme = _userSettings.ThemeName == "Light" ? ApplicationTheme.Light : ApplicationTheme.Dark;
            ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, _userSettings.ThemeName);

            _userSettings.Username = AppProperties.UserLogin.UserName;
            _userSettings.WeightUnit = AppProperties.UserStatistics.WeightUnit;
            _userSettings.ReceiveAlertEmails = AppProperties.UserStatistics.SendAirlineBillsWarning && AppProperties.UserStatistics.SendLicenseWarning;
            _userSettings.CurrentSimData = FlightJobsConnectSim.CommonSimData;

            var userSettingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
            userSettingsModel.UserId = AppProperties.UserLogin.UserId;

            AppProperties.UserSettings = userSettingsModel;
            DataContext = _userSettings;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(HomeView));
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;
                if (selectedItem != null)
                {
                    string tag = (string)selectedItem.Tag;
                    sender.Header = selectedItem.Content;
                    string pageName = "FlightJobsDesktop.Views." + tag;
                    NavigateToPageControl(pageName);
                }
            }
        }

        private void BtnLogoff_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Login(InfraServiceFactory, 
                                        JobServiceFactory, 
                                        UserServiceFactory, 
                                        new MainWindow(InfraServiceFactory, JobServiceFactory, UserServiceFactory, PilotServiceFactory, 
                                                       AirlineServiceFactory, SqLiteContextFactory));

            loginWindow.Show();

            Flyout f = FlyoutService.GetFlyout(BtnUserMenu) as Flyout;
            if (f != null)
            {
                f.Hide();
            }

            Close();
        }

        private void ExitApp_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        internal static void SetLicenseOverdueEllipseVisibility()
        {
            LicenseOverdueEllipse.Visibility = AppProperties.UserStatistics.LicensesOverdue == null ||
                                               AppProperties.UserStatistics.LicensesOverdue.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }


        //private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string pageName = "FlightJobsDesktop.Views." + (string)PrivateViewPageItem.Tag;
        //    NavigateToPageControl(pageName);
        //    nvMain.SelectedItem = PrivateViewPageItem;
        //}


    }
}
