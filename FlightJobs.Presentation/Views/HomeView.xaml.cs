﻿using System.Windows.Controls;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl && ((TabControl)e.Source).SelectedContent is ManagerJobsView)
            {
                var managerView = (ManagerJobsView) ((TabControl)e.Source).SelectedContent;
                managerView.LoadManagerView();
            }
        }
    }
}