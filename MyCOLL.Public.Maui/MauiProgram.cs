using Microsoft.Extensions.Logging;
using MyCOLL.RCL.Services;
using MyCOLL.Shared;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace MyCOLL.Public.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // --- 1. REGISTAR OS SERVIÇOS DA RCL ---
            builder.Services.AddBlazoredLocalStorage(config =>
            {
                // Configuração crítica para evitar crashes no Android
                config.JsonSerializerOptions.WriteIndented = false;
            });

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<CarrinhoService>();
            builder.Services.AddScoped<ApiService>();

            // --- 2. CONFIGURAR HTTP CLIENT COM DEV TUNNEL ---

            // O teu link do Dev Tunnel (sem o /swagger no fim)
            string devTunnelUrl = "https://kcqxvj3l-7242.uks1.devtunnels.ms";

#if ANDROID
            builder.Services.AddScoped(sp =>
            {
                // Com Dev Tunnel, não precisamos de handlers complexos para SSL!
                // O certificado é válido e aceite nativamente pelo Android.
                return new HttpClient
                {
                    BaseAddress = new Uri(devTunnelUrl)
                };
            });
#else
            // No Windows podes continuar a usar localhost ou o Dev Tunnel também
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7242")
            });
#endif

            return builder.Build();
        }
    }
}