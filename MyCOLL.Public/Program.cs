// using MyCOLL.Public.Components;
// using MyCOLL.RCL.Components.Layout; // Necessário para encontrar o MainLayout

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services.AddRazorComponents()
//     .AddInteractiveServerComponents();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error", createScopeForErrors: true);
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// app.UseStaticFiles();
// app.UseAntiforgery();

// // AQUI ESTÁ A CORREÇÃO:
// // Temos de dizer ao Blazor para procurar componentes (Páginas) no Assembly da RCL
// app.MapRazorComponents<App>()
//     .AddInteractiveServerRenderMode()
//     .AddAdditionalAssemblies(typeof(MainLayout).Assembly);

// app.Run();


using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyCOLL.Public;
using MyCOLL.Public.Components; 
using Blazored.LocalStorage;
using MyCOLL.RCL.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
// builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<MyCOLL.RCL.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Configurar o Endereço da API (O "Carteiro" precisa de saber onde ir)
var apiUrl = "https://localhost:7242/"; 

// Registar o HttpClient com o endereço base
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

//Configurar o Cofre Local (Para guardar o Token)
builder.Services.AddBlazoredLocalStorage();

//Configurar o Sistema de Autorização do Blazor
builder.Services.AddAuthorizationCore();

//Registar o Guarda Personalizado (Liga o Token ao Estado de Login)
builder.Services.AddScoped<ApiAuthenticationStateProvider>(); 
builder.Services.AddScoped<AuthenticationStateProvider>(p => 
    p.GetRequiredService<ApiAuthenticationStateProvider>()); 

// Registar o Serviço de Comunicação (ApiService)
builder.Services.AddScoped<ApiService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CarrinhoService>();



await builder.Build().RunAsync();