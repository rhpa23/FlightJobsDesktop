using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using log4net;
using Microsoft.Win32;
using ModernWpf.Controls;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static CapacityViewModel _lastCapacitySelected;
        private static readonly ILog _log = LogManager.GetLogger(typeof(CustomCapacityModal));

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

            if (_lastCapacitySelected != null && generateJobData.CapacityList.Any(x => x.Id == _lastCapacitySelected.Id))
            {
                generateJobData.Capacity = generateJobData.CapacityList.FirstOrDefault(x => x.Id == _lastCapacitySelected.Id);
                lsvCapacityList.SelectedIndex = generateJobData.CapacityList.ToList().FindIndex(x => x.Id == _lastCapacitySelected.Id);
            }
        }

        private void LoadThumbImg(string filename)
        {
            try
            {
                if (filename != null)
                {
                    filename = new FileInfo(filename).Name;
                    DirectoryInfo directoryInfo = new DirectoryInfo("img");

                    var imgLocalPath = $"{directoryInfo.FullName}\\{filename}";
                    if (File.Exists(imgLocalPath))
                        ImgPreview.Source = new BitmapImage(new Uri(imgLocalPath));
                }
                else
                {
                    ImgPreview.Source = new BitmapImage(new Uri("/img/background/default_thumb-capacity.jpg", UriKind.Relative)); 
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }
        }

        private void SaveThumbImg(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo("img");
                    if (!Directory.Exists("img")) directoryInfo = Directory.CreateDirectory("img");

                    var destPath = $"{directoryInfo.FullName}\\{new FileInfo(filePath).Name}";
                    File.Copy(filePath, destPath, true);
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Logo image could not be saved. Please run as administrator.", NotificationType.Error, "WindowArea");
            }
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
            LoadThumbImg(null);
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
                if (capacityModel.ImagePath != null)
                {
                    SaveThumbImg(capacityModel.ImagePath);
                    capacityModel.ImagePath = new FileInfo(capacityModel.ImagePath).Name;
                }
                await _jobService.SavePlaneCapacity(capacityModel);
                _notificationManager.Show("Success", $"Capacity saved.", NotificationType.Success, "NotificationAreaCapacity");
                LoadCapacityList();
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "NotificationAreaCapacity");
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
                _notificationManager.Show("Success", $"Capacity removed.", NotificationType.Success, "NotificationAreaCapacity");
                LoadCapacityList();

                Flyout f = FlyoutService.GetFlyout(BtnRemove) as Flyout;
                if (f != null)
                {
                    f.Hide();
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "NotificationAreaCapacity");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            if (generateJobData.Capacity != null && generateJobData.CapacityList.Count > 0)
            {
                lsvCapacityList.SelectedIndex = generateJobData.CapacityList.ToList().FindIndex(x => x.Id == generateJobData.Capacity.Id);
                LoadThumbImg(generateJobData.Capacity.ImagePath);
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
                LoadThumbImg(generateJobData.Capacity.ImagePath);
                _lastCapacitySelected = generateJobData.Capacity;
                AppProperties.UserStatistics.CustomPlaneCapacity = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                    .Map<CapacityViewModel, CustomPlaneCapacityModel>(generateJobData.Capacity);
            }
        }

        private void BtnAircraftImg_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog() { Title = "Aircraft image (330x115)", Filter = "Image Files|*.jpg;*.jpeg;*.png;..." };

            if (fileDialog.ShowDialog((Window)Parent).Value)
            {
                var generateJobView = (GenerateJobViewModel)DataContext;
                generateJobView.Capacity.ImagePath = fileDialog.FileName;
                ImgPreview.Source = new BitmapImage(new Uri(fileDialog.FileName));
            }
        }
    }
}
