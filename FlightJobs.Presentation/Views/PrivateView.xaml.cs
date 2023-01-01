using Microsoft.Win32;
using ScottPlot;
using System;
using System.Drawing;
using System.Globalization;
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

            LoadChart();
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

        private void LoadChart()
        {
            
            var plt = WpfPlot1.Plot;
            plt.Title("Total in 6 months: $ 129,946      Month goal: $ 31,306", true, Color.Black, 16); //TODO:  
            plt.Style(ScottPlot.Style.Monospace);
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en"); 
            plt.SetCulture(culture);

            // generate 6 Months of data
            int pointCount = 6;
            double[] values = DataGen.RandomWalk(null, pointCount);
            double[] days = new double[pointCount];
            DateTime day1 = DateTime.Now.AddDays(-DateTime.Now.Day).AddMonths(-6);
            for (int i = 0; i < days.Length; i++)
            {
                days[i] = day1.AddMonths(1).AddMonths(i).ToOADate();

                plt.AddText(string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", values[i]), x: days[i], y: values[i], size: 14, color: Color.Black);
            }

            plt.AddScatter(days, values);
            plt.XAxis.TickLabelFormat("yyyy\\/MMM", dateTimeFormat: true);
            // define tick spacing as 1 Month (every Month will be shown)
            plt.XAxis.ManualTickSpacing(1, ScottPlot.Ticks.DateTimeUnit.Month);
            WpfPlot1.Refresh();
        }
    }
}
