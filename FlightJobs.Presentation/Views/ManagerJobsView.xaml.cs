using FlightJobsDesktop.Utils;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Device.Location;
using Notification.Wpf;
using FlightJobsDesktop.Mapper;
using System.Collections.Generic;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Model.Models;
using System.Windows.Input;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interação lógica para PendingJobs.xam
    /// </summary>
    public partial class ManagerJobsView : UserControl
    {
        private NotificationManager _notificationManager;

        private GenerateJobViewModel _generateJobViewModel;

        private string mapUrl = $"{new InfraService().GetApiUrl()}Maps/GenerateJobsMap";
        private string mapUrlQuery = "?departure={0}&arrival={1}&alternative={2}&username={3}";

        public ManagerJobsView()
        {
            InitializeComponent();

            _generateJobViewModel = new GenerateJobViewModel();
            _notificationManager = new NotificationManager();
        }

        private void SwapIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var departure = _generateJobViewModel.DepartureICAO;
            var arrival  = _generateJobViewModel.ArrivalICAO;
            _generateJobViewModel.DepartureICAO = arrival;
            _generateJobViewModel.ArrivalICAO = departure;
        }

        private void BtnDestinationTips_Click(object sender, RoutedEventArgs e)
        {
            var modal = new DestinationTipsModal(_generateJobViewModel.DepartureICAO);
            ShowModal("Arrival Tips", modal);
            if (modal.CloneEvent)
            {
                txtDeparture.Text = "";
                txtArrival.Text = "";
                txtAlternative.Text = "";
                //TODO: Refresh pending list
            }
            else
            {
                if (modal.SelectedJobTip != null)
                {
                    txtArrival.Text = GetIcaoInfo(modal.SelectedJobTip.AirportICAO);
                    AutoSuggestBoxSuggestionChosen();
                }
            }
        }

        private void BtnAlternativeTips_Click(object sender, RoutedEventArgs e)
        {
            var modal = new AlternativeTipsModal(_generateJobViewModel.ArrivalICAO);
            ShowModal("Alternative Tips", modal);
            if (modal.SelectedJobTip != null)
            {
                txtAlternative.Text = GetIcaoInfo(modal.SelectedJobTip.AirportICAO);
                AutoSuggestBoxSuggestionChosen();
            }
        }

        private void BtnCustoCapacity_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Custom capacity", new CustomCapacityModal());
        }

        private void BtnGerate_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Confirm jobs", new ConfirmJobModal());
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
                WindowStyle = WindowStyle.ToolWindow
            };

            window.ShowDialog();
        }


        private async void AutoSuggestBoxICAO_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            BtnArrivalTips.IsEnabled = _generateJobViewModel.DepartureICAO?.Length > 3;
            BtnAlternativeTips.IsEnabled = _generateJobViewModel.DepartureICAO?.Length > 3 && _generateJobViewModel.ArrivalICAO?.Length > 3;

            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
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

        private IEnumerable GetIcaoSugestions(string text)
        {
            var list = AirportDatabaseFile.FindAirportInfoByTerm(text);
            return list.Select(x => $"{x.ICAO} - {x.Name}" ).ToArray();
        }

        private string GetIcaoInfo(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var info = AirportDatabaseFile.FindAirportInfo(text);
                return $"{info.ICAO} - {info.Name}";
            }
            return "";
        }

        public int CalcDistance(string departure, string arrival)
        {
            var departureInfo = AirportDatabaseFile.FindAirportInfo(departure);
            var arrivalInfo = AirportDatabaseFile.FindAirportInfo(arrival);
            if (departureInfo != null && arrivalInfo != null)
            {
                var departureCoord = new GeoCoordinate(departureInfo.Latitude, departureInfo.Longitude);
                var arrivalCoord = new GeoCoordinate(arrivalInfo.Latitude, arrivalInfo.Longitude);

                var distMeters = departureCoord.GetDistanceTo(arrivalCoord);
                var distMiles = (int)DataConversion.ConvertMetersToMiles(distMeters);
                return distMiles;
            }
            return 0;
        }

        private void AutoSuggestBoxSuggestionChosen()
        {
            try
            {
                if (_generateJobViewModel.DepartureICAO?.Length > 3 && _generateJobViewModel.ArrivalICAO?.Length > 3)
                {
                    var departure = _generateJobViewModel.DepartureICAO?.Substring(0, 4);
                    var arrival = _generateJobViewModel.ArrivalICAO?.Substring(0, 4);
                    var alternative = _generateJobViewModel.AlternativeICAO?.Length >= 4 ? _generateJobViewModel.AlternativeICAO.Substring(0, 4) : "";
                    var user = AppProperties.UserLogin.UserName;

                    string url = mapUrl + string.Format(mapUrlQuery, departure, arrival, alternative, user);
                    MapWebView.Navigate(url);

                    _generateJobViewModel.Dist = CalcDistance(departure, arrival);
                    lblDistance.Content = _generateJobViewModel.DistDesc;
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Map data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            AutoSuggestBoxSuggestionChosen();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
        }

        private void MapWebView_DOMContentLoaded(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlDOMContentLoadedEventArgs e)
        {
            try
            {
                if (MapWebView.Source.HostNameType != UriHostNameType.Unknown)
                {
                    MapWebView.InvokeScript("eval", new string[] { "var node = document.getElementsByTagName('body')[0]; var map = document.getElementById('mapContainer'); while (node.hasChildNodes()) { node.removeChild(node.lastChild); } node.appendChild(map);" });
                    var queryDictionary = System.Web.HttpUtility.ParseQueryString(MapWebView.Source.Query);

                    txtDeparture.Text = GetIcaoInfo(queryDictionary[0].ToString());
                    txtArrival.Text = GetIcaoInfo(queryDictionary[1].ToString());
                    txtAlternative.Text = GetIcaoInfo(queryDictionary[2].ToString());
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Map data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        internal void LoadManagerView()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowArea");
            try
            {
                string url = mapUrl + string.Format(mapUrlQuery, "", "", "", AppProperties.UserLogin.UserName);
                MapWebView.Navigate(url);

                var userJobsListView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<IList<JobModel>, IList<CurrentJobViewModel>>(AppProperties.UserJobs);
                _generateJobViewModel.PendingJobs = userJobsListView;
                
                DataContext = _generateJobViewModel;

                lsvPendingJobs.SelectedIndex = AppProperties.UserJobs.ToList().FindIndex(x => x.IsActivated);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Map data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var autoSuggestBox = (AutoSuggestBox)sender;
            if (autoSuggestBox.Text?.Length >= 4 && autoSuggestBox.Items?.Count > 0)
            {
                autoSuggestBox.Text = autoSuggestBox.Items.CurrentItem.ToString();
                AutoSuggestBoxSuggestionChosen();
            }
        }

        private async void lsvPendingJobs_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowArea");
            try
            {
                var jobService = new JobService();
                var selected = (CurrentJobViewModel)lsvPendingJobs.SelectedValue;
                await jobService.ActivateJob(AppProperties.UserLogin.UserId, selected.Id);
                await jobService.GetAllUserJobs(AppProperties.UserLogin.UserId);// Reload the job list
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

    }
}
