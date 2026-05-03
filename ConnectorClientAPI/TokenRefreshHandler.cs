using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectorClientAPI
{
    /// <summary>
    /// DelegatingHandler que intercepta requisições HTTP e renova o token de acesso automaticamente
    /// quando recebe uma resposta 401 (Unauthorized).
    /// </summary>
    public class TokenRefreshHandler : DelegatingHandler
    {
        private Func<Task<bool>> _refreshTokenCallback;
        private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public TokenRefreshHandler(Func<Task<bool>> refreshTokenCallback)
        {
            _refreshTokenCallback = refreshTokenCallback;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Realiza a requisição inicial
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Se a resposta for 401 (Unauthorized), tenta renovar o token
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Usa um semáforo para evitar múltiplas tentativas simultâneas de refresh
                await _refreshLock.WaitAsync(cancellationToken);
                try
                {
                    // Tenta renovar o token através do callback
                    bool refreshed = await _refreshTokenCallback();

                    if (refreshed)
                    {
                        // Se o token foi renovado, tenta realizar a requisição novamente
                        response.Dispose();
                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshLock.Release();
                }
            }

            return response;
        }
    }

    /// <summary>
    /// Response model para o endpoint de refresh token
    /// </summary>
    public class RefreshTokenResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }

        public UserRefreshTokenResponse User { get; set; }
    }

    public class UserRefreshTokenResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
