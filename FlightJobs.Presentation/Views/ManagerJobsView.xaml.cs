using FlightJobsDesktop.Utils;
using FlightJobsDesktop.ViewModels;
using FlightJobsDesktop.Views.Modals;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

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

            //this.Map.IsScriptNotifyAllowed = true;
            //this.Map.ScriptNotify += Map_ScriptNotify;
            //this.Map.NavigateToLocal("\\web\\web-map.html");
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
                        txtDeparture.Text = inputValue;
                        break;
                    case "Arrival":
                        txtArrival.Text = inputValue;
                        break;
                    case "Alternative":
                        txtAlternative.Text = inputValue;
                        break;
                    default:
                        break;
                }
            }
            
        }

        private void TxtArrival_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtDeparture.Text.Length == 4 && txtArrival.Text.Length == 4)
            {
                //LoadMap();
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
            var departure = txtDeparture.Text;
            var arrival  = txtArrival.Text;
            txtDeparture.Text = arrival;
            txtArrival.Text = departure;
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


        private async void AutoSuggestBoxICAO_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //Set the ItemsSource to be your filtered dataset
                var text = sender.Text;
                if (text.Length > 1)
                {
                    sender.ItemsSource = await Task.Run(() => GetIcaoSugestions(text));
                }
                else
                {
                    sender.ItemsSource = new string[] { "No suggestion.." };
                }
            }
        }

        private IEnumerable GetIcaoSugestions(string text)
        {
            var list = AirportDatabaseFile.FindAirportInfoByTerm(text);
            return list.Select(x => $"{x.ICAO} - {x.Name}" ).ToArray();
        }

        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
        }


        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                // Use args.QueryText to determine what to do.
            }
        }
    }
}
