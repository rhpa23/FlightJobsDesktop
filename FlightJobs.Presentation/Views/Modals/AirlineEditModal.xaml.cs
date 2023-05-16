using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ValidationRules;
using FlightJobsDesktop.ViewModels;
using Microsoft.Win32;
using Notification.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
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
    /// Interação lógica para AirlineEditModal.xam
    /// </summary>
    public partial class AirlineEditModal : UserControl
    {
        private NotificationManager _notificationManager;
        private IAirlineService _airlineService;

        public bool IsCreateAirline { get; set; }

        public AirlineEditModal()
        {
            InitializeComponent();
            _notificationManager = new NotificationManager();
            _airlineService = MainWindow.AirlineServiceFactory.Create();
        }

        private void LoadCountries()
        {
            var images = Properties.Resources.ResourceManager
                       .GetResourceSet(CultureInfo.CurrentCulture, true, true)
                       .Cast<DictionaryEntry>()
                       .Where(x => x.Value.GetType() == typeof(Bitmap))
                       .Select(x => new DictionaryEntry() { Key = x.Key.ToString().Replace("_", " "), Value = $"/img/flags/{ x.Key}.jpg" })
                       .OrderBy(x => x.Key)
                       .ToList();

            CboxAirlineCountry.ItemsSource = images;
            if (AppProperties.UserStatistics.Airline?.Country != null)
            {
                var index = images.FindIndex(x => AppProperties.UserStatistics.Airline.Country.ToLower().Contains(x.Key.ToString().ToLower()));
                CboxAirlineCountry.SelectedIndex = index;
            }
        }

        private void SaveLogoImg(string filePath)
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

        private void LoadLogoImg(string filename)
        {
            try
            {
                filename = new FileInfo(filename).Name;
                DirectoryInfo directoryInfo = new DirectoryInfo("img");

                var imgLocalPath = $"{directoryInfo.FullName}\\{filename}";
                ImgLogoPreview.Source = new BitmapImage(new Uri(imgLocalPath));
            }
            catch (Exception)
            {
                //empty
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadCountries();
                if (IsCreateAirline)
                {
                    DataContext = new AirlineViewModel();
                }
                else
                {
                    var airlineView = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg).Map<AirlineModel, AirlineViewModel>(AppProperties.UserStatistics.Airline);
                    DataContext = airlineView;
                    LoadLogoImg(airlineView.Logo);
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaAirlineEdit");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var airlineView = (AirlineViewModel)DataContext;
                var airlineModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg).Map<AirlineViewModel, AirlineModel>(airlineView);
                airlineModel.Country = CboxAirlineCountry.SelectedItem != null ? ((DictionaryEntry)CboxAirlineCountry.SelectedItem).Key.ToString() : "Brazil";
                if (airlineModel.Logo != null)
                {
                    SaveLogoImg(airlineModel.Logo);
                    airlineModel.Logo = new FileInfo(airlineModel.Logo).Name;
                }
                
                if (IsCreateAirline)
                {
                    airlineModel = await _airlineService.CreateAirline(airlineModel, AppProperties.UserLogin.UserId);
                }
                else
                {
                    await _airlineService.UpdateAirline(airlineModel, AppProperties.UserLogin.UserId);
                }

                AppProperties.UserStatistics.Airline = airlineModel;

                ((Window)this.Parent).DialogResult = true;
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Airline data could not be saved. Please try again later.", NotificationType.Error, "WindowAreaAirlineEdit");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).DialogResult = false;
        }

        private void TextChanged(object sender, EventArgs e)
        {
            BtnSaveBorder.IsEnabled = MinimumCharacterRule.IsValid(TxtAirlineName.Text, 6) &&
                                    (CboxAirlineCountry.SelectedIndex >= 0);
        }

        private void BtnLogo_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog() { Title = "Airline logo", Filter = "Image Files|*.jpg;*.jpeg;*.png;..." };
            
            if (fileDialog.ShowDialog((Window)Parent).Value)
            {
                var airlineView = (AirlineViewModel)DataContext;
                airlineView.Logo = fileDialog.FileName;
                ImgLogoPreview.Source = new BitmapImage(new Uri(fileDialog.FileName));
            }
        }
    }
}
