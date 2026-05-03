using FlightJobs.Connect.MSFS.SDK;
using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Common;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using ModernWpf.Controls;
using Notification.Wpf;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para PracticeViewModel.xam
    /// </summary>
    public partial class PracticeView : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;
        private ISqLiteDbContext _sqLiteDbContext;
        private Flyout _flyoutConfirm;

        private PracticeViewModel _practiceViewModel;

        public PracticeView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
            _sqLiteDbContext = MainWindow.SqLiteContextFactory.Create();

            _practiceViewModel = (PracticeViewModel)DataContext;
        }

        private IEnumerable GetIcaoSugestions(string text)
        {
            var list = _sqLiteDbContext.GetAirportsByIcaoAndName(text);
            return list.Select(x => $"{x.Ident} - {x.Name}").ToArray();
        }

        private void HideConfirmPopup()
        {
            if (_flyoutConfirm != null)
            {
                _flyoutConfirm.Hide();
            }
        }
        private async void AutoSuggestBoxICAO_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                var text = sender.Text;
                if (text.Length > 1)
                {
                    sender.ItemsSource = await Task.Run(() => GetIcaoSugestions(text));
                }
                else
                {
                    sender.ItemsSource = new string[] { "" };
                }
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmPopup();
        }

        private void BtnStartOk_Click(object sender, RoutedEventArgs e)
        {
            if (_practiceViewModel.ArrivalICAO?.Length >= 4)
            {
                var icao = _practiceViewModel.ArrivalICAO.Substring(0, 4);
                var airport = _sqLiteDbContext.GetAirportByIcao(icao);
                if (airport != null)
                {
                    ConnectorView.LoadPracticeData(airport);
                    PracticeStartGrid.Visibility = Visibility.Collapsed;
                    PracticeFinishGrid.Visibility = Visibility.Visible;
                    ArrivalICAOLabel.Text = _practiceViewModel.ArrivalICAO;
                    HomeView.DisableTabControl();
                    MainWindow.DisableNavigationBar();
                    BtnPacticeFinishBorder.IsEnabled = true;
                    HideConfirmPopup();
                }
            }
        }

        private void FlyoutConfirm_Opened(object sender, object e)
        {
            _flyoutConfirm = (Flyout)sender;
        }

        private void BtnPacticeFinish_Click(object sender, RoutedEventArgs e)
        {
            PracticeStartGrid.Visibility = Visibility.Visible;
            PracticeFinishGrid.Visibility = Visibility.Collapsed;
            HomeView.EnableTabControl();
            MainWindow.EnableNavigationBar();
        }

        private void BtnPacticeReset_Click(object sender, RoutedEventArgs e)
        {

            if (_practiceViewModel.ArrivalICAO?.Length >= 4)
            {
                var icao = _practiceViewModel.ArrivalICAO.Substring(0, 4);
                var airport = _sqLiteDbContext.GetAirportByIcao(icao);
                if (airport != null)
                {
                    ConnectorView.LoadPracticeData(airport);
                    HomeView.DisableTabControl();
                    MainWindow.DisableNavigationBar();
                }
            }
        }
    }
}
