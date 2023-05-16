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
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para AirlineHireFboModal.xam
    /// </summary>
    public partial class AirlineHireFboModal : UserControl
    {
        private NotificationManager _notificationManager;
        private IAirlineService _airlineService;

        private Flyout _flyoutConfirmHire;

        public AirlineHireFboModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingHireFbo");
            try
            {
                LoadFbosData();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaHireFbo");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void LoadFbosData(string icao = "")
        {
            if (AppProperties.UserStatistics.Airline != null)
            {
                var fbosList = await _airlineService.GetFbos(icao, AppProperties.UserStatistics.Airline.Id);
                
                var fbosViewModel = new HiredFBOsViewModel();
                fbosViewModel.AirlineName = AppProperties.UserStatistics.Airline.Name;
                fbosViewModel.HiredFBOs = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                                            .Map<IList<AirlineFboDbModel>, IList<AirlineFboViewModel>>(fbosList);

                fbosViewModel.HiredFBOs.ToList().ForEach(x => { 
                    x.AirlineNameAux = AppProperties.UserStatistics.Airline?.Name; 
                    x.AirlineBankBalanceAux = string.Format("F{0:C}", AppProperties.UserStatistics.Airline?.BankBalance);
                });

                DataContext = fbosViewModel;
            }
        }

        private void HideConfirmHirePopup()
        {
            if (_flyoutConfirmHire != null)
            {
                _flyoutConfirmHire.Hide();
            }
        }

        private void FlyoutConfirmHire_Opened(object sender, object e)
        {
            _flyoutConfirmHire = (Flyout)sender;
        }

        private async void BtnHire_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingHireFbo");
            try
            {
                var fboToHire = ((FrameworkElement)sender).DataContext as AirlineFboViewModel;
                var airlineFbosHired = await _airlineService.HireFbo(fboToHire.Icao, AppProperties.UserLogin.UserId);
                AppProperties.UserStatistics.Airline.HiredFBOs = airlineFbosHired;
                ((Window)Parent).DialogResult = true;
            }
            catch (ApiException ex)
            {
                var obj = JObject.Parse(ex.ErrorMessage);
                _notificationManager.Show("Warning", (string)obj["Data"]["responseText"], NotificationType.Warning, "WindowAreaHireFbo");
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaHireFbo");
            }
            finally
            {
                HideConfirmHirePopup();
                progress.Dispose();
            }
        }

        private void BtnCancelHire_Click(object sender, RoutedEventArgs e)
        {
            HideConfirmHirePopup();
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingHireFbo");
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                var fobViewModel = (HiredFBOsViewModel)DataContext;
                LoadFbosData(fobViewModel.Filter.Icao);
                progress.Dispose();

            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaHireFbo");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private void BtnFilterClear_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoadingHireFbo");
            try
            {
                Flyout f = FlyoutService.GetFlyout(BtnFilter) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }

                var fobViewModel = (HiredFBOsViewModel)DataContext;
                fobViewModel.Filter = new AirlineFboViewFilterModel();
                LoadFbosData();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowAreaHireFbo");
            }
            finally
            {
                progress.Dispose();
            }
        }
       
    }
}
