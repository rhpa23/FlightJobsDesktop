using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using log4net;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interação lógica para DestinationTipsModal.xam
    /// </summary>
    public partial class ConfirmJobModal : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(ConfirmJobModal));

        public ConfirmJobModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            
            _jobService = MainWindow.JobServiceFactory.Create();
        }

        private void TxtAlternativeRange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                GenerateView.ShowLoading();
                var generateJobData = (GenerateJobViewModel)DataContext;
                if (generateJobData.Capacity == null)
                {
                    _notificationManager.Show("Error", "There is no Capacity selected. Please select a Capacity.", NotificationType.Error, "WindowArea");
                }

                var generateJobFilter = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                        .Map<GenerateJobViewModel, GenerateJobModel>(generateJobData);

                generateJobFilter.UserId = AppProperties.UserLogin.UserId;

                var listJobsModel = await _jobService.GenerateConfirmJobs(generateJobFilter);
                generateJobData.JobItemList = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                        .Map<IList<JobListItemModel>, IList<JobItemViewModel>>(listJobsModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Could not load data. Please try again later.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                GenerateView.HideLoading();
            }
        }

        private void lsvColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader &&
                ((GridViewColumnHeader)e.OriginalSource).Content.ToString() == "Select all")
            {
                lsvJobItens.SelectAll();
            }
        }

        private void lsvJobItens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            long totalPax = 0;
            long totalCargo = 0;
            long totalPay = 0;
            generateJobData.JobItemList.ToList().ForEach(x => x.IsSelected = false);
            foreach (JobItemViewModel item in lsvJobItens.SelectedItems)
            {
                totalPay += item.Pay;
                if (item.IsCargo)
                {
                    totalCargo += item.Cargo;
                }
                else
                {
                    totalPax += item.Pax;
                }
                generateJobData.JobItemList.First(x => x.Id == item.Id).IsSelected = true;
            }
            generateJobData.WeightUnit = AppProperties.UserStatistics.WeightUnit;
            generateJobData.SelectedPax = totalPax;
            generateJobData.SelectedCargo = totalCargo;
            generateJobData.SelectedPay = totalPay;
            generateJobData.SelectedTotalPayload = totalCargo + (totalPax * generateJobData.Capacity.PassengerWeight);
        }
    }
}
