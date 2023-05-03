using FlightJobsDesktop.Views.Modals;
using System;
using System.Collections.Generic;
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

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para AirlinesView.xam
    /// </summary>
    public partial class AirlinesView : UserControl
    {
        public AirlinesView()
        {
            InitializeComponent();
        }

        private void ShowModal(string title, object content)
        {

            Window window = new Window
            {
                Title = title,
                Content = content,
                Width = ((UserControl)content).MinWidth,
                Height = ((UserControl)content).MinHeight + 40,
                //SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = true,
                WindowStyle = WindowStyle.ToolWindow
            };

            window.ShowDialog();
        }

        private void BtnJoinAirliner_Click(object sender, RoutedEventArgs e)
        {
            var modal = new JoinAirlineModal();
            ShowModal("Search airline to join", modal);
        }

        private void BtnBuyAirline_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
