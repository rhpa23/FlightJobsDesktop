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
    /// Interação lógica para AirlineJobsLedgerModal.xam
    /// </summary>
    public partial class AirlineJobsLedgerModal : UserControl
    {
        private IAirlineService _airlineService;
        private NotificationManager _notificationManager;

        public AirlineJobsLedgerModal()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
            _notificationManager = new NotificationManager();
        }

        private void EnabledNaveagtionButtons(bool isEnabled)
        {
            var model = (PaginatedAirlineJobLedgerViewModel)DataContext;
            model.HasPreviousPage = model.HasNextPage = isEnabled;
        }

        private async Task UpdateDataGrid(int pageNumber)
        {
            if (pageNumber > 0)
            {
                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;

                var filter = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                                      .Map<FilterAirlineJobLedger, FilterJobsModel>(ledgerViewModel.Filter);

                filter.UserId = AppProperties.UserLogin.UserId;

                var airlineJobsLedger = await _airlineService.GetAirlineLedger(AppProperties.UserStatistics.Airline.Id, pageNumber, filter);
                var jobsLedgerViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<PaginatedAirlineJobLedgerModel, PaginatedAirlineJobLedgerViewModel>(airlineJobsLedger);

                jobsLedgerViewModel.AirlineJobs.ToList().ForEach(x => x.Job.WeightUnit = AppProperties.UserSettings.WeightUnit);
                jobsLedgerViewModel.Filter = ledgerViewModel.Filter;

                jobsLedgerViewModel.PageSize = jobsLedgerViewModel.PageSize > jobsLedgerViewModel.TotalItemCount ? jobsLedgerViewModel.TotalItemCount : jobsLedgerViewModel.PageSize;

                DataContext = jobsLedgerViewModel;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            try
            {
                await UpdateDataGrid(1);
                progress.Dispose();
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            EnabledNaveagtionButtons(false);
            try
            {
                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;
                ledgerViewModel.PageNumber += 1;
                await UpdateDataGrid(ledgerViewModel.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            EnabledNaveagtionButtons(false);
            try
            {
                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;
                ledgerViewModel.PageNumber -= 1;
                await UpdateDataGrid(ledgerViewModel.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            EnabledNaveagtionButtons(false);
            try
            {
                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;
                ledgerViewModel.PageNumber = 1;
                await UpdateDataGrid(ledgerViewModel.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            EnabledNaveagtionButtons(false);
            try
            {
                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;
                ledgerViewModel.PageNumber = ledgerViewModel.PageCount;
                await UpdateDataGrid(ledgerViewModel.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                await UpdateDataGrid(1);
                progress.Dispose();

            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void BtnFilterClear_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingLedger");
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                var ledgerViewModel = (PaginatedAirlineJobLedgerViewModel)DataContext;
                ledgerViewModel.Filter = new FilterAirlineJobLedger();
                await UpdateDataGrid(1);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaAirlineLedger");
            }
            finally
            {
                progress.Dispose();
            }
        }
    }
}
