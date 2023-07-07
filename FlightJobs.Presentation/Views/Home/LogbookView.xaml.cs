using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
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
    /// Interação lógica para LogbookView.xam
    /// </summary>
    public partial class LogbookView : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;
        private ISqLiteDbContext _sqLiteDbContext;

        public LogbookView()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
            _sqLiteDbContext = MainWindow.SqLiteContextFactory.Create();
        }

        private IEnumerable GetIcaoSugestions(string text)
        {
            var list = _sqLiteDbContext.GetAirportsByIcaoAndName(text);
            return list.Select(x => $"{x.Ident} - {x.Name}").ToArray();
        }

        private async Task UpdateDataGrid(int pageNumber)
        {
            if (pageNumber > 0)
            {
                var logbook = (LogbookViewModel)DataContext;

                var filter = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                                      .Map<FilterLogbook, FilterJobsModel>(logbook.Filter);

                filter.UserId = AppProperties.UserLogin.UserId;
                filter.DepartureICAO = !string.IsNullOrEmpty(filter.DepartureICAO) && filter.DepartureICAO.Length >= 4 ? filter.DepartureICAO.Substring(0, 4) : "";
                filter.ArrivalICAO = !string.IsNullOrEmpty(filter.ArrivalICAO) && filter.ArrivalICAO.Length >= 4 ? filter.ArrivalICAO.Substring(0, 4) : "";

                var userLogbookJobs = await _jobService.GetLogbookUserJobs(null, null, pageNumber, filter);
                var logbookViewModel = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<PaginatedJobsModel, LogbookViewModel>(userLogbookJobs);

                logbookViewModel.Filter = logbook.Filter;

                logbookViewModel.PageSize = logbookViewModel.PageSize > logbookViewModel.TotalItemCount ? logbookViewModel.TotalItemCount : logbookViewModel.PageSize;

                DataContext = logbookViewModel;
            }
        }

        private void EnabledNaveagtionButtons(bool isEnabled)
        {
            var logbook = (LogbookViewModel)DataContext;
            logbook.HasPreviousPage = logbook.HasNextPage = isEnabled;
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (LogbookViewModel)DataContext;
                logbook.PageNumber += 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (LogbookViewModel)DataContext;
                logbook.PageNumber -= 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (LogbookViewModel)DataContext;
                logbook.PageNumber = 1;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
                EnabledNaveagtionButtons(true);
            }
        }

        private async void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            EnabledNaveagtionButtons(false);
            try
            {
                var logbook = (LogbookViewModel)DataContext;
                logbook.PageNumber = logbook.PageCount;
                await UpdateDataGrid(logbook.PageNumber);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
                EnabledNaveagtionButtons(true);
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

        private async void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                await UpdateDataGrid(1);

                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
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

        private async void BtnFilterClear_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
            try
            {
                var logbook = (LogbookViewModel)DataContext;
                logbook.Filter = new FilterLogbook();
                await UpdateDataGrid(1);

                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
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

        private async void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoading();
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
                MainWindow.HideLoading();
            }
        }
    }
}
