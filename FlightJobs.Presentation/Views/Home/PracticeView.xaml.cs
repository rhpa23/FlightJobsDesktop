using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobsDesktop.ViewModels;
using ModernWpf.Controls;
using Notification.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private IAirportDataProvider _airportDataProvider;
        private Flyout _flyoutConfirm;

        private PracticeViewModel _practiceViewModel;

        public PracticeView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
            _airportDataProvider = MainWindow.AirportDataProviderFactory.Create();

            _practiceViewModel = (PracticeViewModel)DataContext;
        }

        private async Task<IEnumerable> GetIcaoSugestions(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new string[0];
            text = text.Trim().ToUpperInvariant();
            
            var cached = await _airportDataProvider.GetAirportAsync(text);
            if (cached != null)
            {
                return new[] { $"{cached.Ident} - {cached.Name}" };
            }

            return new[] { text };
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

        private async void BtnStartOk_Click(object sender, RoutedEventArgs e)
        {
            if (_practiceViewModel.ArrivalICAO?.Length >= 4)
            {
                var icao = _practiceViewModel.ArrivalICAO.Substring(0, 4).ToUpperInvariant();
                MainWindow.ShowLoading();
                try
                {
                    var airport = await _airportDataProvider.GetAirportAsync(icao);
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
                    else
                    {
                        _notificationManager.Show("Aviso", $"Não foi possível carregar os dados de {icao} do simulador ou cache.", NotificationType.Warning, "WindowArea");
                    }
                }
                finally
                {
                    MainWindow.HideLoading();
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

        private async void BtnPacticeReset_Click(object sender, RoutedEventArgs e)
        {
            if (_practiceViewModel.ArrivalICAO?.Length >= 4)
            {
                var icao = _practiceViewModel.ArrivalICAO.Substring(0, 4).ToUpperInvariant();
                var airport = await _airportDataProvider.GetAirportAsync(icao);
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
