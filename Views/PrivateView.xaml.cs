using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class PrivateView : UserControl
    {
        public PrivateView()
        {
            InitializeComponent();
        }

        private void Avatar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            bool? result = file.ShowDialog();

            if (result.Value && File.Exists(file.FileName))
            {
                ImgAvatar.Source = new BitmapImage(new Uri(file.FileName, UriKind.Absolute));
            }
        }

        private void ImgGraduation_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
