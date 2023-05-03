using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using ModernWpf.Controls;
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
    /// Interação lógica para JoinAirlineModal.xam
    /// </summary>
    public partial class JoinAirlineModal : UserControl
    {
        private IAirlineService _airlineService;
        private NotificationManager _notificationManager;
        private IInfraService _infraService;

        public JoinAirlineModal()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _notificationManager = new NotificationManager();
            _infraService = MainWindow.InfraServiceFactory.Create();
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
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
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
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
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
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
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
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
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
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
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
                progress.Dispose();
            }
        }

        private async void btnViewHiredPilots_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            try
            {
                var id = ((AppBarButton)sender).Tag;
                var pilotsHired = await _airlineService.GetAirlinePilotsHired((int)id);
                var pilotsHiredViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<UserModel>, IList<PilotHiredViewModel>>(pilotsHired);
                
                progress.Dispose();
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
                progress.Dispose();
            }
        }
    }
}
