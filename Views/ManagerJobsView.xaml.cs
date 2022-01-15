using FlightJobsDesktop.Views.Modals;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views
{
    /// <summary>
    /// Interação lógica para PendingJobs.xam
    /// </summary>
    public partial class ManagerJobsView : UserControl
    {
        public ManagerJobsView()
        {
            InitializeComponent();

            this.Map.IsScriptNotifyAllowed = true;
            this.Map.ScriptNotify += Map_ScriptNotify;
            this.Map.NavigateToLocal("\\web\\web-map.html");
        }

        private void Map_ScriptNotify(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlScriptNotifyEventArgs e)
        {
            string[] splitedValue = e.Value.Split('#');
            
            if (splitedValue.Length == 2)
            {
                string inputName = splitedValue[0];
                string inputValue = splitedValue[1];

                switch (inputName)
                {
                    case "Departure":
                        TxtDeparture.Text = inputValue;
                        break;
                    case "Arrival":
                        TxtArrival.Text = inputValue;
                        break;
                    case "Alternative":
                        TxtAlternative.Text = inputValue;
                        break;
                    default:
                        break;
                }
            }
            
        }

        private void TxtArrival_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtDeparture.Text.Length == 4 && TxtArrival.Text.Length == 4)
            {
                LoadMap();
            }
        }

        private void LoadMap()
        {
            string curDir = Directory.GetCurrentDirectory();

            string moqfile = File.ReadAllText(String.Format("{0}\\web\\moq.json", curDir));

            this.Map.InvokeScriptAsync("eval", new string[] { $"ShowMap({moqfile})" });
        }

        private void SwapIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var departure = TxtDeparture.Text;
            var arrival  = TxtArrival.Text;
            TxtDeparture.Text = arrival;
            TxtArrival.Text = departure;
        }

        private void BtnDestinationTips_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Destination Tips", new DestinationTipsModal());
        }

        private void BtnAlternativeTips_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Alternative Tips", new AlternativeTipsModal());
        }

        private void BtnCustoCapacity_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Custom capacity", new CustomCapacityModal());
        }

        private void BtnGerate_Click(object sender, RoutedEventArgs e)
        {
            ShowModal("Confirm jobs", new ConfirmJobModal());
        }

        private void ShowModal(string title, object content)
        {
            Window window = new Window
            {
                Title = title,
                Content = content,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow
            };

            window.ShowDialog();
        }
    }
}
