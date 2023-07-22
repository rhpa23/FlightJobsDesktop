using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para AirlineJoinModal.xam
    /// </summary>
    public partial class AirlineJoinModal : UserControl
    {
        internal static DockPanel _loadingPanel;
        internal static StackPanel _loadingProgressPanel;
        private static int _loadingCount;
        private NotificationManager _notificationManager;
        private IAirlineService _airlineService;
        private IInfraService _infraService;
        private Flyout _flyoutConfirmJoin;

        public AirlineJoinModal()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _infraService = MainWindow.InfraServiceFactory.Create();
            _loadingPanel = LoadingPanel;
            _loadingProgressPanel = LoadingProgressPanel;
            _notificationManager = new NotificationManager();
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

        private void HideConfirmJoinPopup()
        {
            if (_flyoutConfirmJoin != null)
            {
                _flyoutConfirmJoin.Hide();
            }
        }

        private void FlyoutConfirmJoin_Opened(object sender, object e)
        {
            _flyoutConfirmJoin = (Flyout)sender;
        }

        private async Task UpdateDataGrid(int pageNumber)
        {
            if (pageNumber > 0)
            {
                var airlinesFilter = (AirlineFilterViewModel)DataContext;

                var filter = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                                      .Map<AirlineFilterViewModel, PaginatedAirlinersFilterModel>(airlinesFilter);

                filter.UserId = AppProperties.UserLogin.UserId;

                var pagedAirlines = await _airlineService.GetByFilter(null, null, pageNumber, filter);

                foreach (var airline in pagedAirlines.Airlines)
                {
                    airline.Logo = _infraService.GetApiUrl() + airline.Logo;
                }

                var airlineViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<PaginatedAirlinersModel, AirlineFilterViewModel>(pagedAirlines);

                airlineViewModel.AirlineName = airlinesFilter.AirlineName;
                airlineViewModel.AirlineCountry = airlinesFilter.AirlineCountry;
                airlineViewModel.PageSize = airlineViewModel.PageSize > airlineViewModel.TotalItemCount ? airlineViewModel.TotalItemCount : airlineViewModel.PageSize;
                DataContext = airlineViewModel;
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

        private void EnabledNaveagtionButtons(bool isEnabled)
        {
            var airlineFilter = (AirlineFilterViewModel)DataContext;
            airlineFilter.HasPreviousPage = airlineFilter.HasNextPage = isEnabled;
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (AirlineFilterViewModel)DataContext;
                logbook.PageNumber += 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (AirlineFilterViewModel)DataContext;
                logbook.PageNumber -= 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (AirlineFilterViewModel)DataContext;
                logbook.PageNumber = 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (AirlineFilterViewModel)DataContext;
                logbook.PageNumber = logbook.PageCount;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            try
            {
                await UpdateDataGrid(1);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
            }
        }

        private async void btnViewHiredPilots_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            try
            {
                var id = ((AppBarButton)sender).Tag;
                var pilotsHired = await _airlineService.GetAirlinePilotsHired((int)id);
                var pilotsHiredViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<UserModel>, IList<PilotHiredViewModel>>(pilotsHired);
                
                HideLoading();
                var modal = new PilotsHiredModal();
                modal.DataContext = new PilotsHiredViewModel() { PilotsHired = pilotsHiredViewModel };
                ShowModal("List of pilot hired", modal);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
            }
        }

        private async void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var airlineId = (int)((FrameworkElement)sender).Tag;
                if (await _airlineService.JoinAirline(airlineId, AppProperties.UserLogin.UserId))
                {
                    ((Window)Parent).DialogResult = true;
                }
            }
            catch (ApiException ex)
            {
                _notificationManager.Show("Warning", ex.ErrorMessage, NotificationType.Warning, "WindowArea");
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
                EnabledNaveagtionButtons(true);
                HideConfirmJoinPopup();
            }
        }

        private void BtnJoinAirlineNo_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmJoinPopup();
        }

        private async void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                await UpdateDataGrid(1);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
            }
        }

        private async void BtnFilterClear_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading();
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                var joinAirlineViewModel = (AirlineFilterViewModel)DataContext;
                joinAirlineViewModel.AirlineName = "";
                joinAirlineViewModel.AirlineCountry = "";
                await UpdateDataGrid(1);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                HideLoading();
            }
        }
    }
}
