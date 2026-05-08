using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobs.Infrastructure.Services.Interfaces;
using log4net;
using ModernWpf.Controls;
using Notification.Wpf;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FlightJobsDesktop.Views.Home
{
    /// <summary>
    /// Interação lógica para WebAppViewModel.xam
    /// </summary>
    public partial class WebAppView : UserControl
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WebAppView));

        public WebAppView()
        {
            InitializeComponent();
        }

        private async void LoadWebApp()
        {
            try
            {
                string siteUrl = "https://flightjobs.vercel.app";

                // Inicializa o WebView2 de forma assíncrona
                await WebAppControl.EnsureCoreWebView2Async(null);

                // Configura opções adicionais se necessário
                WebAppControl.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                WebAppControl.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // Navega para a URL
                WebAppControl.Source = new Uri(siteUrl);

                _logger.Info($"Navegação para {siteUrl} iniciada com sucesso (WebView2).");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erro ao carregar WebApp: {ex.Message}", ex);
                MessageBox.Show($"Erro ao carregar a aplicação web: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWebApp();
        }
    }
}
