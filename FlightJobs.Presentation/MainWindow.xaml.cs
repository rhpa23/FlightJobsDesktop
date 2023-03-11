using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Factorys;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views;
using ModernWpf;
using Newtonsoft.Json;
using Notification.Wpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FlightJobsDesktop
{
    /// <summary>
    /// Lógica interna para Test2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static IAbstractFactory<IJobService> JobServiceFactory;
        public static IAbstractFactory<IUserAccessService> UserServiceFactory;
        public static IAbstractFactory<IInfraService> InfraServiceFactory;

        private UserSettingsViewModel _userSettings;

        public MainWindow(IAbstractFactory<IInfraService> factoryInfra,
                          IAbstractFactory<IJobService> factoryJob, 
                          IAbstractFactory<IUserAccessService> factoryUser)
        {
            InitializeComponent();

            JobServiceFactory = factoryJob;
            UserServiceFactory = factoryUser;
            InfraServiceFactory = factoryInfra;

            ResizeMode = ResizeMode.CanResizeWithGrip;
            

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

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(HomeView));
            nvMain.SelectedItem = HomeViewPageItem;

            LoadSettings();
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
            var jsonSettings = File.ReadAllText(System.IO.Path.Combine(path, "ResourceData\\Settings.json"));
            _userSettings = JsonConvert.DeserializeObject<UserSettingsViewModel>(jsonSettings);
            _userSettings.SimConnectStatus = "Waiting for sim start...";

            ThemeManager.Current.ApplicationTheme = _userSettings.ThemeName == "Light" ? ApplicationTheme.Light : ApplicationTheme.Dark;
            ControlzEx.Theming.ThemeManager.Current.ChangeThemeBaseColor(Application.Current, _userSettings.ThemeName);

            _userSettings.Username = AppProperties.UserLogin.UserName;
            _userSettings.WeightUnit = AppProperties.UserStatistics.WeightUnit;
            _userSettings.ReceiveAlertEmails = AppProperties.UserStatistics.SendAirlineBillsWarning && AppProperties.UserStatistics.SendLicenseWarning;

            var userSettingsModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<UserSettingsViewModel, UserSettingsModel>(_userSettings);
            userSettingsModel.UserId = AppProperties.UserLogin.UserId;

            AppProperties.UserSettings = userSettingsModel;
            DataContext = _userSettings;
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(HomeView));
            }
            else
            {
                var selectedItem = (ModernWpf.Controls.NavigationViewItem)args.SelectedItem;
                if (selectedItem != null)
                {
                    sender.Header = selectedItem.Content;
                    string pageName = "FlightJobsDesktop.Views." + (string)selectedItem.Tag;
                    NavigateToPageControl(pageName);
                }
            }
        }



        private void TitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            string pageName = "FlightJobsDesktop.Views." + (string)PrivateViewPageItem.Tag;
            NavigateToPageControl(pageName);
            nvMain.SelectedItem = PrivateViewPageItem;
        }


    }
}
