# Token Refresh Implementation - FlightJobsConnectorClientAPI

## Visão Geral

Implementei um sistema de renovação automática de token de acesso (access token) através de um interceptador de requisições HTTP (DelegatingHandler). O sistema renova o token automaticamente quando recebe uma resposta 401 (Unauthorized) da API.

## Componentes Implementados

### 1. **TokenRefreshHandler** (`TokenRefreshHandler.cs`)
Um `DelegatingHandler` customizado que:
- Intercepta todas as requisições HTTP
- Detecta respostas 401 (Unauthorized)
- Automaticamente tenta renovar o token usando o refresh token
- Retenta a requisição original com o novo token
- Utiliza `SemaphoreSlim` para evitar múltiplas tentativas simultâneas de refresh

```csharp
TokenRefreshHandler tokenRefreshHandler = new TokenRefreshHandler(
    ApiBaseUrl,
    RefreshAccessTokenAsync
);
```

### 2. **RefreshAccessTokenAsync** (método privado em `FlightJobsConnectorClientAPI`)
Responsável por:
- Validar que o refresh token existe
- Fazer requisição POST para `/auth/refresh`
- Atualizar os tokens armazenados (`_accessToken` e `_refreshToken`)
- Usar um `HttpClient` temporário para evitar recursão infinita

### 3. **Armazenamento de Tokens**
- **_accessToken**: Token de acesso usado nas requisições autenticadas
- **_refreshToken**: Token de refresh usado para renovar o access token

## Fluxo de Funcionamento

```
1. Usuário faz login
   ↓
2. Tokens (access e refresh) são armazenados
   ↓
3. Requisição é feita com o access token
   ↓
4. Se receber 401:
   ├─ TokenRefreshHandler intercepta
   ├─ Chama RefreshAccessTokenAsync()
   ├─ Renova tokens via POST /auth/refresh
   └─ Retenta a requisição original
   ↓
5. Se sucesso: retorna resposta
   Se falha: retorna erro
```

## Alterações Realizadas

### FlightJobsConnectorClientAPI.cs

1. **Adicionado armazenamento de refresh token**:
   ```csharp
   private static string _refreshToken;
   ```

2. **Modificado construtor** para usar `TokenRefreshHandler`:
   ```csharp
   public FlightJobsConnectorClientAPI()
   {
       HttpClientHandler innerHandler = new HttpClientHandler()
       {
           AllowAutoRedirect = false
       };

       TokenRefreshHandler tokenRefreshHandler = new TokenRefreshHandler(
           ApiBaseUrl,
           RefreshAccessTokenAsync
       );
       tokenRefreshHandler.InnerHandler = innerHandler;

       _client = new HttpClient(tokenRefreshHandler);
       // ... resto do código
   }
   ```

3. **Adicionado método RefreshAccessTokenAsync()**:
   - Renova o token quando chamado pelo handler
   - Usa um HttpClient temporário para evitar loop infinito

4. **Atualizado método Login()**:
   - Agora armazena tanto `access_token` quanto `refresh_token`
   ```csharp
   _accessToken = result.access_token;
   _refreshToken = result.refresh_token;
   ```

5. **Adicionado método Logout()**:
   - Limpa os tokens ao fazer logout
   ```csharp
   public void Logout()
   {
       _accessToken = null;
       _refreshToken = null;
       _client.DefaultRequestHeaders.Authorization = null;
   }
   ```

## Exemplo de Uso

```csharp
// Inicializar
var api = new FlightJobsConnectorClientAPI();

// Login
var loginResponse = await api.Login("email@example.com", "password");

// Fazer requisições - o token será renovado automaticamente se necessário
var pendingJobs = await api.GetPendingUserJobs();

// Logout
api.Logout();
```

## Segurança

- O refresh token é armazenado em memória (variável estática)
- Múltiplas tentativas de refresh simultâneas são evitadas com `SemaphoreSlim`
- Se o refresh falhar, a requisição original retorna com o erro 401
- Um novo `HttpClient` temporário é criado para a requisição de refresh (evita handlers aninhados)

## Considerações

- **Atualizações de Token**: Os tokens são atualizados automaticamente em cada refresh bem-sucedido
- **Timeout de Rede**: Se a rede falhar no refresh, a requisição original falha normalmente
- **Sincronização**: O semáforo garante que apenas uma renovação aconteça por vez
- **Sem Recursão**: O handler de refresh não é aplicado ao HttpClient temporário usado para renovar tokens

## Testes Recomendados

1. Teste login com credenciais válidas
2. Teste se requisições funcionam com token válido
3. Teste renovação automática simulando token expirado (retorno 401)
4. Teste múltiplas requisições simultâneas com token expirado
5. Teste logout
