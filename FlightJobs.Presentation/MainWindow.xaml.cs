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
using log4net;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FlightJobsDesktop
{
    /// <summary>
    /// Lógica interna para Test2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static NavigationView NavigationBar { get; set; }

        private FlightJobsConnectSim _flightJobsConnectSim = new FlightJobsConnectSim();
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public static IAbstractFactory<IJobService> JobServiceFactory;
        public static IAbstractFactory<IUserAccessService> UserServiceFactory;
        public static IAbstractFactory<IInfraService> InfraServiceFactory;
        public static IAbstractFactory<IAirlineService> AirlineServiceFactory;
        public static IAbstractFactory<ISqLiteDbContext> SqLiteContextFactory;

        private UserSettingsViewModel _userSettings;

        public MainWindow(IAbstractFactory<IInfraService> factoryInfra,
                          IAbstractFactory<IJobService> factoryJob, 
                          IAbstractFactory<IUserAccessService> factoryUser,
                          IAbstractFactory<IAirlineService> factoryAirline,
                          IAbstractFactory<ISqLiteDbContext> factorySqLiteContext)
        {
            InitializeComponent();

            JobServiceFactory = factoryJob;
            UserServiceFactory = factoryUser;
            InfraServiceFactory = factoryInfra;
            AirlineServiceFactory = factoryAirline;
            SqLiteContextFactory = factorySqLiteContext;

            ResizeMode = ResizeMode.CanResizeWithGrip;

            NavigationBar = nvMain;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("favicon.ico");
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(HomeView));
            nvMain.SelectedItem = HomeViewPageItem;

            LoadSettings();

            _flightJobsConnectSim.Initialize();
            //_timerCheckSimConnection.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            ShowInTaskbar = false;
            e.Cancel = true;
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
        }

        private void NavigateToPageControl(string pageName)
        {
            Type pageType = typeof(HomeView).Assembly.GetType(pageName);
            contentFrame.Navigate(pageType);
        }

        private void LoadSettings()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var jsonSettings = File.ReadAllText(Path.Combine(path, "ResourceData\\Settings.json"));
            _userSettings = JsonConvert.DeserializeObject<UserSettingsViewModel>(jsonSettings);

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
                                        new MainWindow(InfraServiceFactory, JobServiceFactory, UserServiceFactory, AirlineServiceFactory, SqLiteContextFactory));

            loginWindow.AutoSingIn = false;
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


        //private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string pageName = "FlightJobsDesktop.Views." + (string)PrivateViewPageItem.Tag;
        //    NavigateToPageControl(pageName);
        //    nvMain.SelectedItem = PrivateViewPageItem;
        //}


    }
}
