using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConnectorClientAPI.Examples
{
    /// <summary>
    /// Exemplos de como usar a API com renovação automática de token
    /// </summary>
    public class TokenRefreshExamples
    {
        public static async Task BasicLoginAndUsageExample()
        {
            // Criar instância da API
            var api = new FlightJobsConnectorClientAPI();

            try
            {
                // 1. Fazer login
                var loginResponse = await api.Login("user@example.com", "password123");

                if (loginResponse != null)
                {
                    Console.WriteLine($"Login bem-sucedido!");
                    Console.WriteLine($"User ID: {loginResponse.UserId}");
                    Console.WriteLine($"Email: {loginResponse.Email}");

                    // 2. O token será renovado automaticamente se expirar durante as requisições
                    var pendingJobs = await api.GetPendingUserJobs();
                    Console.WriteLine($"Pending jobs: {pendingJobs.Count}");

                    var activeJob = await api.GetActiveUserJob();
                    if (activeJob != null)
                    {
                        Console.WriteLine($"Active job: {activeJob.Id}");
                    }

                    var statistics = await api.GetUserStatistics();
                    Console.WriteLine($"BankBalance: {statistics.BankBalance}");

                    // 3. Fazer logout ao terminar
                    api.Logout();
                    Console.WriteLine("Logout bem-sucedido!");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição: {ex.Message}");
            }
        }

        public static async Task ErrorHandlingExample()
        {
            var api = new FlightJobsConnectorClientAPI();

            try
            {
                // Tentar login com credenciais inválidas
                var loginResponse = await api.Login("invalid@example.com", "wrongpass");

                if (loginResponse == null)
                {
                    Console.WriteLine("Login falhou - credenciais inválidas");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro de conexão: {ex.Message}");
            }
            finally
            {
                // Sempre fazer logout para limpar os tokens
                api.Logout();
            }
        }

        public static async Task LongRunningOperationExample()
        {
            var api = new FlightJobsConnectorClientAPI();

            // Login
            var loginResponse = await api.Login("user@example.com", "password123");

            if (loginResponse != null)
            {
                // Se o token expirar durante esta operação,
                // o TokenRefreshHandler renovará automaticamente
                try
                {
                    var activeJob = await api.GetActiveUserJob();

                    if (activeJob != null)
                    {
                        // Simular operação longa onde o token poderia expirar
                        await Task.Delay(5000);

                        // Esta requisição usará o token renovado automaticamente
                        var stats = await api.GetUserStatistics();
                        Console.WriteLine("Operação longa completada com sucesso!");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Erro durante operação longa: {ex.Message}");
                }
                finally
                {
                    api.Logout();
                }
            }
        }
    }
}
