using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para DestinationTipsModal.xam
    /// </summary>
    public partial class AlternativeTipsModal : UserControl
    {
        private string _arrivalICAO;

        private NotificationManager _notificationManager;
        private IJobService _jobService;

        public TipsDataGridViewModel SelectedJobTip { get; set; }

        public AlternativeTipsModal()
        {
            InitializeComponent();
        }

        public AlternativeTipsModal(string arrivalICAO)
        {
            InitializeComponent();
            _arrivalICAO = arrivalICAO;
            _notificationManager = new NotificationManager();
        }

        private async Task LoadDataGrid()
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "ModalArea");
            try
            {
                if (!string.IsNullOrEmpty(_arrivalICAO) && _arrivalICAO.Length > 3)
                {
                    var list = await _jobService.GetAlternativeTips(_arrivalICAO.Substring(0, 4), (int)RangeNumberBox.Value);

                    var tipJobsListView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<IList<SearchJobTipsModel>, IList<TipsDataGridViewModel>>(list);

                    var tipsDataGridViewModel = new TipsDataGridViewModel();
                    tipsDataGridViewModel.Tips = tipJobsListView;
                    DataContext = tipsDataGridViewModel;
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Alternative jobs tips could not be loaded. Please try again later.", NotificationType.Error, "ModalArea");
            }
            finally
            {
                progress.Dispose();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _jobService = MainWindow.JobServiceFactory.Create();
            await LoadDataGrid();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedJobTip = (TipsDataGridViewModel)dataGrid.SelectedItem;
            ((Window)Parent).Close();
        }

        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataGrid();
        }
    }
}
