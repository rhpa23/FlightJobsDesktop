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

        private Flyout _flyoutEditSave;
        private Flyout _flyoutRemove;
        private IList<UIElement> _flyoutEditSaveUIElements = new List<UIElement>();

        public CustomCapacityModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            
            _jobService = MainWindow.JobServiceFactory.Create();
        }
        private async Task LoadCapacityList()
        {
            MainWindow.ShowLoading();
            try
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
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Error when try to access Flightjobs online data.", NotificationType.Error, "WindowArea");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void LoadThumbImg(string filename)
        {
            try
            {
                if (filename != null)
                {
                    var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var rootPath = System.IO.Path.Combine(localPath, $"FlightJobsDesktop\\images");

                    DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
                    filename = new FileInfo(filename).Name;

                    var imgLocalPath = $"{directoryInfo.FullName}\\{filename}";
                    if (File.Exists(imgLocalPath))
                        ImgPreview.Source = new BitmapImage(new Uri(imgLocalPath));
                    else
                        ImgPreview.Source = new BitmapImage(new Uri("/img/background/default_thumb-capacity.jpg", UriKind.Relative));
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
                    var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var rootPath = System.IO.Path.Combine(localPath, $"FlightJobsDesktop\\images");

                    DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
                    if (!Directory.Exists(rootPath)) directoryInfo = Directory.CreateDirectory(rootPath);
                    
                    var destPath = $"{directoryInfo.FullName}\\{new FileInfo(filePath).Name}";
                    File.Copy(filePath, destPath, true);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Logo image could not be saved. Please run as administrator.", NotificationType.Error, "WindowArea");
            }
        }

        private void TxtAlternativeRange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private async Task SaveCapacity()
        {
            try
            {
                MainWindow.ShowLoading();
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
                await LoadCapacityList();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "NotificationAreaCapacity");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private async void UpdateCapacity()
        {
            try
            {
                MainWindow.ShowLoading();
                var generateJobData = (GenerateJobViewModel)DataContext;
                var capacityModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                    .Map<CapacityViewModel, CustomPlaneCapacityModel>(generateJobData.Capacity);
                capacityModel.UserId = AppProperties.UserLogin.UserId;
                if (capacityModel.ImagePath != null)
                {
                    SaveThumbImg(capacityModel.ImagePath);
                    capacityModel.ImagePath = new FileInfo(capacityModel.ImagePath).Name;
                }
                await _jobService.UpdatePlaneCapacity(capacityModel);
                _notificationManager.Show("Success", $"Capacity updated.", NotificationType.Success, "NotificationAreaCapacity");
                await LoadCapacityList();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Data could not be updated. Please try again later.", NotificationType.Error, "NotificationAreaCapacity");
            }
            finally
            {
                MainWindow.HideLoading();
            }
        }

        private void HideEditPopup()
        {
            if (_flyoutEditSave != null)
            {
                _flyoutEditSave.Hide();
            }
        }

        private void HideRemovePopup()
        {
            if (_flyoutRemove != null)
            {
                _flyoutRemove.Hide();
            }
        }

        private void SelectCapacityListLine(FrameworkElement sender)
        {
            var selectedId = sender.Tag != null ? (int)sender.Tag : 0;

            var generateJobData = (GenerateJobViewModel)DataContext;

            if (generateJobData.CapacityList.Any(x => x.Id == selectedId))
            {
                generateJobData.Capacity = generateJobData.CapacityList.FirstOrDefault(x => x.Id == selectedId);
                lsvCapacityList.SelectedIndex = generateJobData.CapacityList.ToList().FindIndex(x => x.Id == selectedId);
            }
        }

        private async void BtnRemoveYes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.ShowLoading();
                var generateJobData = (GenerateJobViewModel)DataContext;
                var capacityModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                    .Map<CapacityViewModel, CustomPlaneCapacityModel>(generateJobData.Capacity);
                capacityModel.UserId = AppProperties.UserLogin.UserId;
                await _jobService.RemovePlaneCapacity(capacityModel);
                _notificationManager.Show("Success", $"Capacity removed.", NotificationType.Success, "NotificationAreaCapacity");
                await LoadCapacityList();

                HideRemovePopup();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _notificationManager.Show("Error", "Data could not be saved. Please try again later.", NotificationType.Error, "NotificationAreaCapacity");
            }
            finally
            {
                MainWindow.HideLoading();
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
                UpdateCapacity();
            }
        }

        private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;

            foreach (var item in _flyoutEditSaveUIElements)
            {
                if (item is TextBox txBox && txBox.Name == "TxtCapacityName")
                {
                    generateJobData.Capacity.Name = txBox.Text;
                }
                if (item is NumberBox numBox)
                {
                    int numBoxIntValue = string.IsNullOrEmpty(numBox.Text) ? -1 : int.Parse(numBox.Text);
                    switch (numBox.Name)
                    {
                        case "TxtPassengerCapacity":
                            generateJobData.Capacity.PassengersNumber = numBoxIntValue;
                            break;
                        case "TxtPassengerWeight":
                            generateJobData.Capacity.PassengerWeight = numBoxIntValue;
                            break;
                        case "TxtCargoCapacityWeight":
                            generateJobData.Capacity.CargoWeight = numBoxIntValue;
                            break;
                    }
                }
            }
            //SAVE
            UpdateCapacity();
            HideEditPopup();
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            HideEditPopup();
        }

        private void BtnCancelRemove_Click(object sender, RoutedEventArgs e)
        {
            HideRemovePopup();
        }

        private void Flyout1_Opened(object sender, object e)
        {
            if (sender != null)
            {
                _flyoutEditSave = (Flyout)sender;
                if (lsvCapacityList.SelectedValue != null)
                    lsvCapacityList.SelectedValue = lsvCapacityList.SelectedValue;

                _flyoutEditSaveUIElements.Clear();
                var generateJobData = (GenerateJobViewModel)DataContext;
                var cont = (StackPanel)_flyoutEditSave.Content;
                foreach (var item in cont.Children)
                {
                    if (item is TextBox txBox && txBox.Name == "TxtCapacityName")
                    {
                        txBox.Text = generateJobData.Capacity.Name;
                        _flyoutEditSaveUIElements.Add(txBox);
                    }
                    if (item is NumberBox numBox)
                    {
                        _flyoutEditSaveUIElements.Add(numBox);

                        switch (numBox.Name)
                        {
                            case "TxtPassengerCapacity":
                                numBox.Text = generateJobData.Capacity.PassengersNumber.ToString();
                                break;
                            case "TxtPassengerWeight":
                                numBox.Text = generateJobData.Capacity.PassengerWeight.ToString();
                                break;
                            case "TxtCargoCapacityWeight":
                                numBox.Text = generateJobData.Capacity.CargoWeight.ToString();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void Flyout2_Opened(object sender, object e)
        {
            if (sender != null)
            {
                _flyoutRemove = (Flyout)sender;
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            SelectCapacityListLine((FrameworkElement)sender);
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            SelectCapacityListLine((FrameworkElement)sender);
        }

        private void BtnNewAndSave_Click(object sender, RoutedEventArgs e)
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            generateJobData.Capacity = new CapacityViewModel();
            LoadThumbImg(null);
            TxtNewCapacityName.Focus();
        }

        private async void BtnSaveNew_Click(object sender, RoutedEventArgs e)
        {
            await SaveCapacity();
            var generateJobData = (GenerateJobViewModel)DataContext;
            var capacity = generateJobData.CapacityList.FirstOrDefault(x => x.Name.ToLower() == TxtNewCapacityName.Text.ToLower());
            generateJobData.Capacity = capacity;
            lsvCapacityList.SelectedIndex = generateJobData.CapacityList.ToList().FindIndex(x => x.Id == capacity.Id);
            FlyoutNewCapacity.Hide();
        }

        private void BtnCancelNew_Click(object sender, RoutedEventArgs e)
        {
            FlyoutNewCapacity.Hide();
        }
    }
}
