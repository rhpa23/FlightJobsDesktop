using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Infrastructure;
using FlightJobs.Infrastructure.Services.Interfaces;
using FlightJobs.Model.Models;
using FlightJobsDesktop.Mapper;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using Notification.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Lógica interna para GenerateView.xaml
    /// </summary>
    public partial class GenerateView : Window
    {
        private NotificationManager _notificationManager;
        private GenerateJobViewModel _generateJobViewModel;
        private IJobService _jobService;

        public GenerateView()
        {
            InitializeComponent();
        }

        public GenerateView(GenerateJobViewModel generateJobViewModel)
        {
            InitializeComponent();
            _generateJobViewModel = generateJobViewModel;
            _notificationManager = new NotificationManager();
            _jobService = MainWindow.JobServiceFactory.Create();
        }

        private void UpdateFrameDataContext()
        {
            var content = contentFrame.Content as FrameworkElement;
            if (content != null)
                content.DataContext = _generateJobViewModel;
        }

        private void NavigationView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (ModernWpf.Controls.NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                sender.Header = selectedItem.Content;
                string pageName = "FlightJobsDesktop.Views.Modals." + (string)selectedItem.Tag;
                NavigateToPageControl(pageName);

                if (selectedItem == ConfirmJobModalPageItem)
                {
                    BtnConfirmBorder.Visibility = Visibility.Visible;
                    BtnNextBorder.Visibility = Visibility.Hidden;
                }
                else
                {
                    BtnConfirmBorder.Visibility = Visibility.Hidden;
                    BtnNextBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void NavigateToPageControl(string pageName)
        {
            Type pageType = typeof(FlightTypeModal).Assembly.GetType(pageName);
            
            //content.DataContext = frame.DataContext;
            contentFrame.Navigate(pageType, this);
            UpdateFrameDataContext();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                contentFrame.Navigate(typeof(FlightTypeModal));
                nvGenerate.SelectedItem = FlightTypeModalPageItem;

                _generateJobViewModel.Capacity = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<CustomPlaneCapacityModel, CapacityViewModel>(AppProperties.UserStatistics.CustomPlaneCapacity);

                var capacitiesModel = await _jobService.GetPlaneCapacities(AppProperties.UserLogin.UserId);
                _generateJobViewModel.CapacityList = new AutoMapper.Mapper(DbModelToViewModelMapper.MapperCfg)
                    .Map<IList<CustomPlaneCapacityModel>, IList<CapacityViewModel>>(capacitiesModel);
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be loaded. Please try again later.", NotificationType.Error, "WindowAreaGenerateJob");
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = nvGenerate.SelectedItem;
            if (selectedItem != null)
            {
                contentFrame.Navigate(typeof(FlightTypeModal));
                if (selectedItem == FlightTypeModalPageItem)
                {
                    nvGenerate.SelectedItem = CustomCapacityModalPageItem;
                }
                else if (selectedItem == CustomCapacityModalPageItem)
                {
                    nvGenerate.SelectedItem = ConfirmJobModalPageItem;
                }
            }
        }

        private void contentFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }

        private async void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var progress = _notificationManager.ShowProgressBar("Loading...", false, true, "WindowAreaLoading");
            BtnConfirm.IsEnabled = false;
            try
            {
                var selectedList = _generateJobViewModel.JobItemList.Where(x => x.IsSelected).ToList();
                if (selectedList.Count() > 0)
                {
                    var confirmJobModel = new AutoMapper.Mapper(ViewModelToDbModelMapper.MapperCfg)
                                    .Map<GenerateJobViewModel, ConfirmJobModel>(_generateJobViewModel);

                    confirmJobModel.UserId = AppProperties.UserLogin.UserId;

                    await _jobService.ConfirmJob(confirmJobModel);
                    _notificationManager.Show("Success", "Confirmed with success.", NotificationType.Success, "WindowAreaGenerateJob");
                    progress.Dispose();
                    await Task.Delay(3000);
                    DialogResult = true;
                }
                else
                {
                    _notificationManager.Show("Warning", "Select jobs to confirm.", NotificationType.Warning, "WindowAreaGenerateJob");
                }
            }
            catch (Exception)
            {
                _notificationManager.Show("Error", "Data could not be Saved. Please try again later.", NotificationType.Error, "WindowAreaGenerateJob");
            }
            finally
            {
                BtnConfirm.IsEnabled = true;
                progress.Dispose();
            }

        }

        //private void contentFrame_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    UpdateFrameDataContext();
        //}
    }
}
