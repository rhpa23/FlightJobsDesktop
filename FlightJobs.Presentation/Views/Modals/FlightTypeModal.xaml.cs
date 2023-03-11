using FlightJobs.Infrastructure;
using FlightJobs.Model.Enum;
using FlightJobsDesktop.ViewModels;
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

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para FlightTypeModal.xam
    /// </summary>
    public partial class FlightTypeModal : UserControl
    {
        public GenerateJobViewModel GenerateJobView { get; set; }

        public FlightTypeModal()
        {
            InitializeComponent();
        }

        private void CheckTypeByTag()
        {
            var generateJobData = (GenerateJobViewModel)DataContext;
            var itens = RdbAviationType.Items;
            for (int i = 0; i < itens.Count; i++)
            {
                var radioButton = ((RadioButton)itens[i]);
                radioButton.IsChecked = (radioButton.Tag.ToString() == generateJobData.AviationType.ToString());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckTypeByTag();
        }

        private void RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RdbAviationType.SelectedItem != null)
            {
                var selItem = (RadioButton)RdbAviationType.SelectedItem;
                var generateJobData = (GenerateJobViewModel)DataContext;
                generateJobData.AviationType = (AviationType)Enum.Parse(typeof(AviationType), selItem.Tag.ToString());
            }
        }
    }
}
