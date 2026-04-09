using MyCOLL.Shared;
using MyCOLL.Shared.DTOs;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace MyCOLL.RCL.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        // --- VOLTAMOS À VERSÃO SIMPLES ---
        // Já não precisamos de "truques" para o Android. O endereço do Dev Tunnel funciona para tudo.
        public string UrlApi => _http.BaseAddress?.ToString().TrimEnd('/') ?? "";

        public string? JwtToken { get; set; }

        public ApiService(HttpClient http, AuthenticationStateProvider authStateProvider,ILocalStorageService localStorage)
        {
            _http = http;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        // --- AUTENTICAÇÃO ---
        public async Task<string?> Login(LoginDto login)
        {
            var result = await _http.PostAsJsonAsync("api/Auth/login", login);
            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<LoginResponse>();
                await ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsAuthenticated(response!.Token);
                return null;
            }
            return "Login falhou.";
        }

        public async Task<string?> Registar(RegistoDto registo)
        {
            var result = await _http.PostAsJsonAsync("api/Auth/register", registo);
            if (result.IsSuccessStatusCode) return null;

            var erro = await result.Content.ReadAsStringAsync();
            return erro;
        }

        public async Task Logout()
        {
            await ((ApiAuthenticationStateProvider)_authStateProvider).MarkUserAsLoggedOut();
        }

        // --- PRODUTOS ---
        public async Task<List<Produto>> GetProdutos()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<Produto>>("api/Produtos") ?? new List<Produto>();
            }
            catch
            {
                return new List<Produto>();
            }
        }

        public async Task<Produto?> GetProdutoDetalhe(int id)
        {
            return await _http.GetFromJsonAsync<Produto>($"api/Produtos/{id}");
        }

        // --- UPLOAD ---
        public async Task<string?> UploadImagemAsync(IBrowserFile ficheiro)
        {
            if (ficheiro == null) return null;
            long maxFileSize = 1024 * 1024 * 5;

            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(ficheiro.OpenReadStream(maxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(ficheiro.ContentType);
                content.Add(fileContent, "ficheiro", ficheiro.Name);

                var response = await _http.PostAsync("api/Produtos/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    return result?.Url;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no upload: {ex.Message}");
            }
            return null;
        }

        // --- OUTROS MÉTODOS (Categorias, Encomendas, etc.) ---
        // (Estes métodos não mudaram, podes manter os que tinhas ou copiar abaixo)

        public async Task<List<Categoria>> GetCategorias() => await _http.GetFromJsonAsync<List<Categoria>>("api/Categorias") ?? new List<Categoria>();

        public async Task<bool> EnviarEncomenda(Encomenda encomenda)
        {
            // 1. Tenta obter o token da propriedade ou do LocalStorage
            string token = JwtToken;
            if (string.IsNullOrEmpty(token))
            {
                token = await _localStorage.GetItemAsync<string>("authToken");
            }

            // 2. Se não houver token, não vale a pena tentar (retorna falso ou força logout)
            if (string.IsNullOrEmpty(token)) return false;

            // 3. ANEXAR O CRACHÁ (TOKEN) AO CABEÇALHO DO PEDIDO
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 4. Agora sim, enviar a encomenda
            var result = await _http.PostAsJsonAsync("api/Encomendas", encomenda);

            return result.IsSuccessStatusCode;
        }

        public async Task<List<Encomenda>> GetMinhasEncomendas()
        {
            // Garante que o token vai no header
            var token = await _localStorage.GetItemAsync<string>("authToken");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _http.GetFromJsonAsync<List<Encomenda>>("api/Encomendas/MeusPedidos") ?? new List<Encomenda>();
        }
        public async Task<List<Produto>> GetMeusProdutos()
        {
            if (!await PrepararToken()) return new List<Produto>();
            return await _http.GetFromJsonAsync<List<Produto>>("api/Produtos/MeusProdutos") ?? new List<Produto>();
        }
        public async Task<bool> CriarProduto(Produto p)
        {
            if (!await PrepararToken()) return false; 
            var result = await _http.PostAsJsonAsync("api/Produtos", p);
            return result.IsSuccessStatusCode;
        }
        public async Task<bool> EditarProduto(int id, Produto p)
        {
            if (!await PrepararToken()) return false;
            var result = await _http.PutAsJsonAsync($"api/Produtos/{id}", p);
            return result.IsSuccessStatusCode;
        }
        public async Task<bool> EliminarProduto(int id)
        {
            if (!await PrepararToken()) return false;
            var result = await _http.DeleteAsync($"api/Produtos/{id}");
            return result.IsSuccessStatusCode;
        }

        public async Task<PerfilDto?> GetMeuPerfil()
        {
            if (!await PrepararToken()) return null;
            try { return await _http.GetFromJsonAsync<PerfilDto>("api/Auth/perfil"); } catch { return null; }
        }

        public async Task<string?> AtualizarPerfil(PerfilDto perfil)
        {
            if (!await PrepararToken()) return "Não autenticado.";
            var result = await _http.PutAsJsonAsync("api/Auth/perfil", perfil);
            if (result.IsSuccessStatusCode) return null;
            return await result.Content.ReadAsStringAsync();
        }


        public async Task<List<DetalheEncomenda>> GetMinhasVendas()
        {
            if (!await PrepararToken()) return new List<DetalheEncomenda>();

            return await _http.GetFromJsonAsync<List<DetalheEncomenda>> ("api/Encomendas/VendasFornecedor")
                ?? new List<DetalheEncomenda> ();
        }

        private async Task<bool> PrepararToken()
        {
            string token = JwtToken;
            if (string.IsNullOrEmpty(token))
            {
                token = await _localStorage.GetItemAsync<string>("authToken");
            }

            if (string.IsNullOrEmpty(token)) return false;

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return true;
        }
    }

    public class LoginResponse { public string Token { get; set; } = ""; public DateTime Expiration { get; set; } }
    public class UploadResult { public string Url { get; set; } = ""; }
}