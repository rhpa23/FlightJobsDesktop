using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using log4net;
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
    /// Interação lógica para PendingJobs.xam
    /// </summary>
    public partial class ManagerJobsView : UserControl
    {
        internal static DockPanel _loadingPanel;
        internal static StackPanel _loadingProgressPanel;

        private NotificationManager _notificationManager;
        private IJobService _jobService;
        private IInfraService _infraService;
        private ISqLiteDbContext _sqLiteDbContext;
        private IUserAccessService _userAccessService;

        private GenerateJobViewModel _generateJobViewModel;

        private string _mapUrl;
        private string _mapUrlQuery;

        private Flyout _flyoutConfirmRemove;
        private static readonly ILog _log = LogManager.GetLogger(typeof(ManagerJobsView));

        public ManagerJobsView()
        {
            InitializeComponent();

            _generateJobViewModel = new GenerateJobViewModel();
            _notificationManager = new NotificationManager();
            _loadingPanel = LoadingPanel;
            _loadingProgressPanel = LoadingProgressPanel;
            _jobService = MainWindow.JobServiceFactory.Create();
            _infraService = MainWindow.InfraServiceFactory.Create();
            _userAccessService = MainWindow.UserServiceFactory.Create();
            _sqLiteDbContext = MainWindow.SqLiteContextFactory.Create();

            _mapUrl = $"{_infraService.GetApiUrl()}Maps/GenerateJobsMap";
            _mapUrlQuery =  "?departure={0}&arrival={1}&alternative={2}&username={3}";
            PanelFlightInfo.IsEnabled = true;
        }

        private void HideConfirmRemovePopup()
        {
            if (_flyoutConfirmRemove != null)
            {
                _flyoutConfirmRemove.Hide();
            }
        }

        private void FlyoutConfirmRemove_Opened(object sender, object e)
        {
            _flyoutConfirmRemove = (Flyout)sender;
        }

        private void SwapIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var departure = _generateJobViewModel.Departure;
            var arrival  = _generateJobViewModel.Arrival;
            _generateJobViewModel.Departure = arrival;
            _generateJobViewModel.Arrival = departure;
        }

        private void BtnDestinationTips_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading(true);
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
            MainWindow.HideLoading();
        }

        private void BtnAlternativeTips_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading(true);
            var modal = new AlternativeTipsModal(_generateJobViewModel.ArrivalICAO);
            ShowModal("Alternative Tips", modal);
            if (modal.SelectedJobTip != null)
            {
                txtAlternative.Text = GetIcaoInfo(modal.SelectedJobTip.AirportICAO);
                AutoSuggestBoxSuggestionChosen();
            }
            MainWindow.HideLoading();
        }

        private async void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PanelFlightInfo.IsEnabled = false;
                MapWebView.Visibility = Visibility.Hidden;
                PanelImgMap.Visibility = Visibility.Collapsed;
                PanelAddJob.Visibility = Visibility.Visible;
                LoadAddJobPanel();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
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
            BtnGenerateBorder.IsEnabled = _generateJobViewModel.DepartureICAO?.Length > 3 && _generateJobViewModel.ArrivalICAO?.Length > 3;

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
            var list = _sqLiteDbContext.GetAirportsByIcaoAndName(text);
            return list.Select(x => $"{x.Ident} - {x.Name}" ).ToArray();
        }

        private string GetIcao(string text)
        {
            if (!string.IsNullOrEmpty(text) && text?.Length > 3)
            {
                return text.Substring(0, 4);
            }
            return "";
        }

        private string GetIcaoInfo(string text)
        {
            if (!string.IsNullOrEmpty(text) && text?.Length > 3)
            {
                var info = _sqLiteDbContext.GetAirportByIcao(text.Substring(0,4));
                return $"{info?.Ident} - {info?.Name}";
            }
            return "";
        }

        private void AutoSuggestBoxSuggestionChosen()
        {
            try
            {
                if (_generateJobViewModel.DepartureICAO?.Length > 3 && _generateJobViewModel.ArrivalICAO?.Length > 3)
                {
                    var departure = _generateJobViewModel.DepartureICAO;
                    var arrival = _generateJobViewModel.ArrivalICAO;
                    var alternative = _generateJobViewModel.AlternativeICAO;
                    var user = AppProperties.UserLogin.UserName;

                    string url = _mapUrl + string.Format(_mapUrlQuery, departure, arrival, alternative, user);
                    MapWebView.Navigate(url);
                    MapWebView.Visibility = Visibility.Visible;
                    PanelImgMap.Visibility = Visibility.Collapsed;
                    PanelAddJob.Visibility = Visibility.Collapsed;

                    _generateJobViewModel.Dist = GeoCalculationsUtil.CalcDistance(departure, arrival);
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
            sender.Text = args.SelectedItem.ToString();
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
                    //MapWebView.InvokeScript("eval", new string[] { "var node = document.getElementsByTagName('body')[0]; var map = document.getElementById('mapContainer'); while (node.hasChildNodes()) { node.removeChild(node.lastChild); } node.appendChild(map);" });
                    var queryDictionary = System.Web.HttpUtility.ParseQueryString(MapWebView.Source.Query);

                    var departureParam = GetIcaoInfo(queryDictionary[0].ToString());
                    var arrivalParam = GetIcaoInfo(queryDictionary[1].ToString());
                    var alternativeParam = GetIcaoInfo(queryDictionary[2].ToString());

                    txtDeparture.Text = string.IsNullOrEmpty(departureParam) ? txtDeparture.Text : departureParam;
                    txtArrival.Text = string.IsNullOrEmpty(arrivalParam) ? txtArrival.Text : arrivalParam;
                    txtAlternative.Text = string.IsNullOrEmpty(alternativeParam) ? txtAlternative.Text : alternativeParam;

                    if (string.IsNullOrEmpty(departureParam) && txtDeparture.Text.Length > 3) // To fix when select departure and set arrival in the map.
                    {
                        AutoSuggestBoxSuggestionChosen();
                    }
                    else
                    {
                        _generateJobViewModel.Dist = GeoCalculationsUtil.CalcDistance(GetIcao(txtDeparture.Text), GetIcao(txtArrival.Text));
                        lblDistance.Content = _generateJobViewModel.DistDesc;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Map data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        internal void LoadManagerView()
        {
            MainWindow.ShowLoading();
            try
            {
                if (PanelImgMap.Visibility == Visibility.Visible)
                {
                    MapWebView.Visibility = Visibility.Hidden;
                }

                var userJobsListView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<IList<JobModel>, IList<CurrentJobViewModel>>(AppProperties.UserJobs);
                _generateJobViewModel.PendingJobs = userJobsListView;
                
                DataContext = _generateJobViewModel;
                
                lsvPendingJobs.ItemsSource = _generateJobViewModel.PendingJobs;
                lsvPendingJobs.SelectedIndex = AppProperties.UserJobs.ToList().FindIndex(x => x.IsActivated);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Map data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var autoSuggestBox = (AutoSuggestBox)sender;
            if (autoSuggestBox.Text?.Length >= 4 && autoSuggestBox.Items?.Count > 0)
            {
                autoSuggestBox.Text = GetIcaoInfo(autoSuggestBox.Text);
                AutoSuggestBoxSuggestionChosen();
            }
        }

        private async void lsvPendingJobs_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                var selected = (CurrentJobViewModel)lsvPendingJobs.SelectedValue;
                if (selected != null)
                {
                    await _jobService.ActivateJob(AppProperties.UserLogin.UserId, selected.Id);
                    await _jobService.GetAllUserJobs(AppProperties.UserLogin.UserId);// Reload the job list
                    HomeView.TabHome.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void BtnRemoveJobYes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedId = ((FrameworkElement)sender).Tag;
                if (selectedId != null)
                {
                    var jobService = MainWindow.JobServiceFactory.Create();
                    await jobService.RemoveJob(AppProperties.UserLogin.UserId, (int)selectedId);

                    _notificationManager.Show("Success", $"Job removed.", NotificationType.Success, "WindowArea");

                    await jobService.GetAllUserJobs(AppProperties.UserLogin.UserId);// To reload pedding list
                    
                    var userJobsListView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<IList<JobModel>, IList<CurrentJobViewModel>>(AppProperties.UserJobs);
                    _generateJobViewModel.PendingJobs = userJobsListView;
                    lsvPendingJobs.ItemsSource = _generateJobViewModel.PendingJobs;
                    lsvPendingJobs.SelectedIndex = AppProperties.UserJobs.ToList().FindIndex(x => x.IsActivated);
                    HomeView.TabHome.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be removed. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideConfirmRemovePopup();
            }
        }

        private void BtnRemoveJobNo_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmRemovePopup();
        }

        private async void BtnSimbrief_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading();
                if (string.IsNullOrEmpty(AppProperties.UserSettings.LocalSettings?.SimbriefUsername))
                {
                    _notificationManager.Show("Warning", "Please update your Simbrief Username in the Settings page.", NotificationType.Warning, "WindowArea");
                }
                else
                {
                    var simBriefData = await _userAccessService.GetSimBriefData(AppProperties.UserSettings.LocalSettings.SimbriefUsername);
                    txtDeparture.Text = GetIcaoInfo(simBriefData.DepartureICAO.IcaoCode);
                    txtArrival.Text = GetIcaoInfo(simBriefData.ArrivalICAO.IcaoCode);
                    txtAlternative.Text = GetIcaoInfo(simBriefData.AlternativeICAO.IcaoCode);
                    AutoSuggestBoxSuggestionChosen();
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access SimBrief data. Please check your Simbrief Username in the Settings page or try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading();
                var simBriefData = await _userAccessService.GetRandomFlight(_generateJobViewModel.DepartureICAO, _generateJobViewModel.AlternativeICAO);
                txtDeparture.Text = GetIcaoInfo(simBriefData.DepartureICAO);
                txtArrival.Text = GetIcaoInfo(simBriefData.ArrivalICAO);
                txtAlternative.Text = GetIcaoInfo(simBriefData.AlternativeICAO);
                AutoSuggestBoxSuggestionChosen();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access FlightJobs random data. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void BtnToggleMap_Click(object sender, RoutedEventArgs e)
        {
            MapWebView.Visibility = Visibility.Visible;
            PanelImgMap.Visibility = Visibility.Collapsed;
            var departure = _generateJobViewModel.DepartureICAO;
            var arrival = _generateJobViewModel.ArrivalICAO;
            var alternative = _generateJobViewModel.AlternativeICAO;
            var user = AppProperties.UserLogin.UserName;

            string url = _mapUrl + string.Format(_mapUrlQuery, departure, arrival, alternative, user);
            MapWebView.Navigate(url);
            
        }

        #region Add job

        private void UpdateFrameDataContext()
        {
            var content = contentFrame.Content as FrameworkElement;
            if (content != null)
                content.DataContext = _generateJobViewModel;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                sender.Header = selectedItem.Content;
                string pageName = "FlightJobsDesktop.Views.Modals." + (string)selectedItem.Tag;
                NavigateToPageControl(pageName);

                if (selectedItem == ConfirmJobModalPageItem)
                {
                    BtnConfirmBorder.Visibility = Visibility.Visible;
                    BtnNextBorder.Visibility = Visibility.Hidden;
                }
                else
                {
                    BtnConfirmBorder.Visibility = Visibility.Hidden;
                    BtnNextBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void NavigateToPageControl(string pageName)
        {
            Type pageType = typeof(FlightTypeModal).Assembly.GetType(pageName);

            //content.DataContext = frame.DataContext;
            contentFrame.Navigate(pageType, this);
            UpdateFrameDataContext();
        }

        private void ShowLoading(bool hideProgressPanel = false)
        {
            _loadingPanel.Visibility = Visibility.Visible;
            _loadingProgressPanel.Visibility = hideProgressPanel ? Visibility.Collapsed : Visibility.Visible;
            //_loadingCount++;
        }

        private void HideLoading()
        {
            //_loadingCount--;

            //if (_loadingCount <= 0)
            //{
                _loadingPanel.Visibility = Visibility.Collapsed;
            //    _loadingCount = 0;
            //}
        }

        private async void LoadAddJobPanel()
        {
            try
            {
                contentFrame.Navigate(typeof(FlightTypeModal));
                nvGenerate.SelectedItem = FlightTypeModalPageItem;

                _generateJobViewModel.Capacity = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<CustomPlaneCapacityModel, CapacityViewModel>(AppProperties.UserStatistics.CustomPlaneCapacity);

                var capacitiesModel = await _jobService.GetPlaneCapacities(AppProperties.UserLogin.UserId);
                _generateJobViewModel.CapacityList = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<CustomPlaneCapacityModel>, IList<CapacityViewModel>>(capacitiesModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Data could not be loaded. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = nvGenerate.SelectedItem;
            if (selectedItem != null)
            {
                contentFrame.Navigate(typeof(FlightTypeModal));
                if (selectedItem == FlightTypeModalPageItem)
                {
                    nvGenerate.SelectedItem = CustomCapacityModalPageItem;
                }
                else if (selectedItem == CustomCapacityModalPageItem)
                {
                    nvGenerate.SelectedItem = ConfirmJobModalPageItem;
                }
            }
        }

        private void contentFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            BtnConfirm.IsEnabled = false;
            try
            {
                var selectedList = _generateJobViewModel.JobItemList.Where(x => x.IsSelected).ToList();
                if (selectedList.Count() > 0)
                {
                    var confirmJobModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                                    .Map<GenerateJobViewModel, ConfirmJobModel>(_generateJobViewModel);

                    confirmJobModel.UserId = AppProperties.UserLogin.UserId;

                    // SAVE
                    await _jobService.ConfirmJob(confirmJobModel);
                    _notificationManager.Show("Success", "Confirmed with success.", NotificationType.Success, "WindowArea");

                    _generateJobViewModel = new GenerateJobViewModel();
                    await _jobService.GetAllUserJobs(AppProperties.UserLogin.UserId);// To reload pedding list
                    lblDistance.Content = "0";
                    PanelImgMap.Visibility = Visibility.Visible;
                    LoadManagerView();
                    HideLoading();
                    HomeView.TabHome.SelectedIndex = 0;
                }
                else
                {
                    _notificationManager.Show("Warning", "Select jobs to confirm.", NotificationType.Warning, "WindowArea");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Data could not be Saved. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                BtnConfirm.IsEnabled = true;
                HideLoading();
            }

        }

        #endregion

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            PanelFlightInfo.IsEnabled = true;
            MapWebView.Visibility = Visibility.Visible;
            PanelImgMap.Visibility = Visibility.Collapsed;
            PanelAddJob.Visibility = Visibility.Hidden;
        }
    }
}
