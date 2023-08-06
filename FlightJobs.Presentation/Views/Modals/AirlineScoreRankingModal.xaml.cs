using FlightJobs.Infrastructure.Services.Interfaces;
using log4net;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace FlightJobsDesktop.Views.Modals
{
    /// <summary>
    /// Interação lógica para AirlineScoreRankingModal.xam
    /// </summary>
    public partial class AirlineScoreRankingModal : UserControl
    {
        private IAirlineService _airlineService;
        private static readonly ILog _log = LogManager.GetLogger(typeof(AirlineScoreRankingModal));

        public AirlineScoreRankingModal()
        {
            InitializeComponent();
            _airlineService = MainWindow.AirlineServiceFactory.Create();

            var chartFont = new Font("Segoe UI", 10);
            ChartScoreRanking.Series[0].Font = chartFont;

            ChartScoreRanking.ChartAreas[0].AxisX.LabelStyle.Font = chartFont;
            ChartScoreRanking.ChartAreas[0].AxisY.LabelStyle.Font = chartFont;

            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
            {
                ChartScoreRanking.BackColor = ColorTranslator.FromHtml("#FF2B2B2B");
                ChartScoreRanking.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                ChartScoreRanking.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;
                ChartScoreRanking.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.White;
                ChartScoreRanking.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.White;
                ChartScoreRanking.ChartAreas[0].BackColor = Color.Black;
                ChartScoreRanking.ChartAreas[0].AxisX.TitleForeColor = Color.White;
                ChartScoreRanking.ChartAreas[0].AxisY.TitleForeColor = Color.White;
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = await _airlineService.GetRanking();

                ChartScoreRanking.Titles.Clear();
                ChartScoreRanking.Titles.Add(new Title("Airliners Score Ranking", Docking.Top, new Font("Segoe UI", 10), Color.White));
                ChartScoreRanking.Series[0].Points.Clear();

                var colors = new Color[] { Color.PowderBlue, Color.Peru, Color.Green, Color.BlueViolet, Color.Brown };
                var random = new Random();

                foreach (var airline in list)
                {
                    var dataPoint = ChartScoreRanking.Series[0].Points.Add(airline.AirlineScore);
                    dataPoint.Label = $"{airline.Name}: {airline.AirlineScore}";
                    dataPoint.Color = colors[random.Next(0, 4)];
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}
