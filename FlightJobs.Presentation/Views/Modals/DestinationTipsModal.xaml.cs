using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interação lógica para DestinationTipsModal.xam
    /// </summary>
    public partial class DestinationTipsModal : UserControl
    {
        private string _departureICAO;

        private NotificationManager _notificationManager;
        private IJobService _jobService;

        public TipsDataGridViewModel SelectedJobTip { get; set; }
        public bool CloneEvent { get; set; }

        public DestinationTipsModal()
        {
            InitializeComponent();
        }

        public DestinationTipsModal(string departureICAO)
        {
            InitializeComponent();
            _departureICAO = departureICAO;
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
        }

        private async Task LoadDataGrid()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "ModalAreaLoading");
            try
            {
                if (!string.IsNullOrEmpty(_departureICAO) && _departureICAO.Length > 3)
                {
                    var list = await _jobService.GetArrivalTips(_departureICAO.Substring(0, 4), AppProperties.UserLogin.UserId);

                    var tipJobsListView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<IList<SearchJobTipsModel>, IList<TipsDataGridViewModel>>(list);

                    var tipsDataGridViewModel = new TipsDataGridViewModel();
                    tipsDataGridViewModel.Tips = tipJobsListView;
                    DataContext = tipsDataGridViewModel;
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Arrival jobs tips could not be loaded. Please try again later.", NotificationType.Error, "ModalArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataGrid();
        }

        async void CloneJob(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "ModalAreaLoading");
            try
            {
                var jobId = (long)((Button)sender).Tag;
                await _jobService.CloneJob(jobId, AppProperties.UserLogin.UserId);
                CloneEvent = true;
                ((Window)Parent).Close();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Clone job could not be run. Please try again later.", NotificationType.Error, "ModalArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataGrid();
        }

        private void dataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedJobTip = (TipsDataGridViewModel)dataGrid.SelectedItem;
            ((Window)Parent).Close();
        }
    }
}
