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
        public CustomCapacityModal()
        {
            InitializeComponent();
        }

        private void TxtAlternativeRange_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void BtnNewAndSave_Click(object sender, RoutedEventArgs e)
        {
            if (BtnNewAndSave.Content.ToString() == "New")
            {

                TxtCapacityName.Text = string.Empty;
                TxtCargoCapacityWeight.Text = string.Empty;
                TxtPassengerCapacity.Text = string.Empty;

                BtnUpdate.IsEnabled = BtnRemove.IsEnabled = false;
                BtnNewAndSave.Content = "Save";
            }
            else
            {
                BtnUpdate.IsEnabled = BtnRemove.IsEnabled = true;
                BtnNewAndSave.Content = "New";
            }
        }
    }
}
