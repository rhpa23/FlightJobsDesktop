using FlightJobs.Domain.Navdata.Interface;
using FlightJobs.Domain.Navdata.Utils;
using FlightJobs.Infrastructure.Services;
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
        private bool _isWebViewInitialized = false;
        public string SiteUrl { get; set; }
        public string Page { get; set; }

        public WebAppView()
        {
            InitializeComponent();
        }

        public async Task NavegateToPage(string page)
        {
            Page = page;

            if (WebAppControl?.CoreWebView2 == null)
                return;

            string url = $"{SiteUrl}/{Page}";
            // Navega para a URL
            WebAppControl.Source = new Uri(url);

            _logger.Info($"Navegação para {url} iniciada com sucesso (WebView2).");
        }

        private async void LoadWebApp()
        {
            try
            {
                // Evita recarregar se já foi inicializado
                if (_isWebViewInitialized)
                    return;

                var infraService = MainWindow.InfraServiceFactory.Create();
                SiteUrl = infraService.GetApiUrl();
                string url = string.IsNullOrEmpty(Page) ? SiteUrl : $"{SiteUrl}/{Page}";
                string accessToken = infraService.GetAccessToken();
                string refreshToken = infraService.GetRefreshToken();

                // Inicializa o WebView2 de forma assíncrona
                await WebAppControl.EnsureCoreWebView2Async(null);

                _isWebViewInitialized = true;

                // Configura opções adicionais se necessário
                WebAppControl.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                WebAppControl.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // Configura evento para injetar tokens no localStorage após navegação
                WebAppControl.CoreWebView2.NavigationCompleted += async (sender, args) =>
                {
                    if (args.IsSuccess && !string.IsNullOrEmpty(accessToken))
                    {
                        string script = $@"
                            try {{
                                localStorage.setItem('token', '{accessToken}');
                                localStorage.setItem('refresh_token', '{refreshToken}');                                
                                localStorage.setItem('flightjobs_desktop', 'true');
                            }} catch(e) {{
                                console.error('Erro ao injetar tokens:', e);
                            }}
                        ";
                        await WebAppControl.ExecuteScriptAsync(script);
                        _logger.Info("Tokens injetados no localStorage do WebView2.");
                    }
                };

                // Navega para a URL
                WebAppControl.Source = new Uri(url);

                _logger.Info($"Navegação para {url} iniciada com sucesso (WebView2).");
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
