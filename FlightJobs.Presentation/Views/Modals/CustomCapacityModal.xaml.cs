using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
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
    public partial class CustomCapacityModal : UserControl
    {
        private NotificationManager _notificationManager;
        private IJobService _jobService;

        public CustomCapacityModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            
            _jobService = MainWindow.JobServiceFactory.Create();
        }
        private async void LoadCapacityList()
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            var capacitiesModel = await _jobService.GetPlaneCapacities(AppProperties.UserLogin.UserId);
            generateJobData.CapacityList = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                .Map<IList<CustomPlaneCapacityModel>, IList<CapacityViewModel>>(capacitiesModel);
        }


        private void TxtAlternativeRange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void BtnNewAndSave_Click(object sender, RoutedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            generateJobData.Capacity = new CapacityViewModel();
            TxtCapacityName.Focus();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var generateJobData = (GenerateJobViewModel)DataContext;
                var capacityModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                    .Map<CapacityViewModel, CustomPlaneCapacityModel>(generateJobData.Capacity);
                capacityModel.UserId = AppProperties.UserLogin.UserId;
                await _jobService.SavePlaneCapacity(capacityModel);
                _notificationManager.Show("Success", $"Capacity saved.", NotificationType.Success, "WindowArea");
                LoadCapacityList();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        private async void BtnRemoveYes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var generateJobData = (GenerateJobViewModel)DataContext;
                var capacityModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                    .Map<CapacityViewModel, CustomPlaneCapacityModel>(generateJobData.Capacity);
                capacityModel.UserId = AppProperties.UserLogin.UserId;
                await _jobService.RemovePlaneCapacity(capacityModel);
                _notificationManager.Show("Success", $"Capacity removed.", NotificationType.Success, "WindowArea");
                LoadCapacityList();

                Flyout f = FlyoutService.GetFlyout(BtnRemove) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "WindowArea");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            if (generateJobData.Capacity != null && generateJobData.CapacityList.Count > 0)
            {
                lsvCapacityList.SelectedIndex = generateJobData.CapacityList.ToList().FindIndex(x => x.Id == generateJobData.Capacity.Id);
            }
            else
            {
                generateJobData.Capacity = new CapacityViewModel();
            }
        }

        private void lsvCapacityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            if (lsvCapacityList.SelectedItem != null)
            {
                generateJobData.Capacity = (CapacityViewModel)lsvCapacityList.SelectedItem;
            }
        }
    }
}
