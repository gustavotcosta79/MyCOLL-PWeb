using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace MyCOLL.RCL.Services
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        // Este método corre sempre que a app precisa de saber "Quem sou eu?"
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. Tentar ler o token do "cofre"
            var savedToken = await _localStorage.GetItemAsync<string>("authToken");

            // 2. Se não houver token, retornar "Anónimo" (Não logado)
            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // 3. Se houver token, configurar o HttpClient para o usar sempre
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);
             
            // 4. Ler os dados do token (Claims) e criar a identidade
            var claims = JwtParser.ParseClaimsFromJwt(savedToken);
            var identity = new ClaimsIdentity(claims, "jwt"); // "jwt" é o tipo de autenticação

            // 5. Retornar "Autenticado"
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        // Método para ser chamado quando fazemos Login com sucesso
        public async Task MarkUserAsAuthenticated(string token)
        {
            await _localStorage.SetItemAsync("authToken", token); // Guardar
            
            var claims = JwtParser.ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Avisar o Blazor: "Ei, o estado mudou! Atualiza os menus!"
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        // Método para ser chamado no Logout
        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("authToken"); // Apagar
            
            var identity = new ClaimsIdentity(); // Vazio = Anónimo
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
    }
}