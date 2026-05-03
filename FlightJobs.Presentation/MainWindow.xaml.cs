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
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notification.Wpf;
using Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private static bool _isLogout;
        internal static Ellipse LicenseOverdueEllipse { get; set; }

        private FlightJobsConnectSim _flightJobsConnectSim = new FlightJobsConnectSim();
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(MainWindow));

        public static IAbstractFactory<IJobService> JobServiceFactory;
        public static IAbstractFactory<IUserAccessService> UserServiceFactory;
        public static IAbstractFactory<IInfraService> InfraServiceFactory;
        public static IAbstractFactory<ISqLiteDbContext> SqLiteContextFactory;
        public static IAbstractFactory<IPilotService> PilotServiceFactory;

        private UserSettingsViewModel _userSettings;

        public MainWindow(IAbstractFactory<IInfraService> factoryInfra,
                          IAbstractFactory<IJobService> factoryJob, 
                          IAbstractFactory<IUserAccessService> factoryUser,
                          IAbstractFactory<IPilotService> factoryPilot,
                          IAbstractFactory<ISqLiteDbContext> factorySqLiteContext)
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();

            JobServiceFactory = factoryJob;
            UserServiceFactory = factoryUser;
            PilotServiceFactory = factoryPilot;
            InfraServiceFactory = factoryInfra;
            SqLiteContextFactory = factorySqLiteContext;

            ResizeMode = ResizeMode.CanResizeWithGrip;

            NavigationBar = nvMain;
            _loadingPanel = LoadingPanel;
            _loadingProgressPanel = LoadingProgressPanel;
            _isLogout = false;

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

        private async Task CheckForUpdates()
        {
            try
            {
                using (var manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/rhpa23/FlightJobsDesktop"))
                {
                    var updateInfo = await manager.CheckForUpdate();

                    if (updateInfo.ReleasesToApply.Any())
                    {
                        await manager.UpdateApp();
                        _notificationManager.ShowButtonWindow("Application updated. The new version will take effect when restarted.");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Update application failed.", ex);
                _notificationManager.Show("Error", "Update application failed.", NotificationType.Error, "WindowArea", TimeSpan.FromSeconds(40));
            }
        }

        public static void EnableNavigationBar()
        {
            foreach (var item in NavigationBar.MenuItems)
            {
                if (item is NavigationViewItem && ((NavigationViewItem)item).Name != "ExitApp")
                {
                    ((NavigationViewItem)item).IsEnabled = true;
                }
            }
        }

        public static void DisableNavigationBar()
        {
            foreach (var item in NavigationBar.MenuItems)
            {
                if (item is NavigationViewItem && ((NavigationViewItem)item).Name != "ExitApp")
                {
                    ((NavigationViewItem)item).IsEnabled = false;
                }
            }
        }

        private async Task LoadData()
        {
            ShowLoading();
            try
            {
                _userSettings = LoadSettingsFromFile();
               // AppProperties.UserLogin
               // await UserServiceFactory.Create().Login(AppProperties.UserLogin.Email, AppProperties.UserLogin.Password);

                await JobServiceFactory.Create().GetAllUserJobs();
                await UserServiceFactory.Create().LoadUserStatisticsProperties();

                contentFrame.Navigate(typeof(HomeView));
                nvMain.SelectedItem = HomeViewPageItem;

                LoadAllSettingsData();

                _flightJobsConnectSim.Initialize();

                TxbTitle.Text = $"FlightJobs Desktop - {Assembly.GetExecutingAssembly().GetName().Version}";
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
                HideLoading();
            }
            finally
            {
                HideLoading();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadData();
            await CheckForUpdates();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isLogout)
            {
                this.Hide();
                ShowInTaskbar = false;
                e.Cancel = true;

                _notificationManager.ShowButtonWindow("FlightJobs still running in the system tray.");
            }
        }

        private void ShowMainWindow()
        {
            if (!_isLogout)
            {
                Show();
                WindowState = WindowState.Normal;
                ShowInTaskbar = true;
            }
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

        internal UserSettingsViewModel LoadSettingsFromFile()
        {
            try
            {
                var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlightJobsDesktop\\ResourceData\\Settings.json");
                var userSettings = new UserSettingsViewModel();

                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Directory.GetParent(path).FullName);

                    userSettings = new UserSettingsViewModel()
                    {
                        StartInSysTray = false,
                        ExitWithFS = true,
                        AutoStartJob = true,
                        AutoFinishJob = true,
                        ShowLandingData = true,
                        ThemeName = "Dark",
                        SelectedHostOption = 1
                    };
                    string jsonData = JsonConvert.SerializeObject(userSettings, Formatting.None);
                    File.WriteAllText(path, jsonData);
                }
                else
                {
                    var jsonSettingsData = File.ReadAllText(path);
                    userSettings = JsonConvert.DeserializeObject<UserSettingsViewModel>(jsonSettingsData);
                }

                var infraService = MainWindow.InfraServiceFactory.Create();
                infraService.SetApiUrl("http://localhost:3002/");
                //infraService.SetApiUrl("https://flightjobs.vercel.app/");

                return userSettings;
            }
            catch (Exception ex)
            {
                _log.Error(ex); // Just to log the Exception
                throw ex;
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
            Logout();
        }

        private void Logout()
        {
            var loginWindow = new Login(InfraServiceFactory,
                                        JobServiceFactory,
                                        UserServiceFactory,
                                        new MainWindow(InfraServiceFactory, JobServiceFactory, UserServiceFactory, PilotServiceFactory,
                                                       SqLiteContextFactory));

            loginWindow.Show();

            Flyout f = FlyoutService.GetFlyout(BtnUserMenu) as Flyout;
            if (f != null)
            {
                f.Hide();
            }
            _isLogout = true;
            Close();
        }

        private void ExitApp_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
