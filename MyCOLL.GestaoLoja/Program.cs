using Microsoft.AspNetCore.Components.Authorization;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using MyCOLL.GestaoLoja.Components;

using MyCOLL.GestaoLoja.Components.Account;

using MyCOLL.Data;

using MyCOLL.RCL.Services;

using Blazored.LocalStorage;



var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddRazorComponents()

    .AddInteractiveServerComponents();



builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IdentityUserAccessor>();

builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();



builder.Services.AddAuthentication(options =>

{

    options.DefaultScheme = IdentityConstants.ApplicationScheme;

    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;

})

    .AddIdentityCookies();



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>

    options.UseSqlServer(connectionString));



builder.Services.AddQuickGridEntityFrameworkAdapter();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)

    .AddRoles<IdentityRole>() ///linha adicionada 

    .AddEntityFrameworkStores<ApplicationDbContext>()

    .AddSignInManager()

    .AddDefaultTokenProviders();



builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddHttpClient<ApiService>(client =>

{
    client.BaseAddress = new Uri("https://localhost:7242");

});



// 2. Registar o ApiService (que agora é usado também pela Gestão)

builder.Services.AddScoped<ApiService>();
builder.Services.AddBlazoredLocalStorage();



var app = builder.Build();



// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}

else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

    app.UseHsts();

    app.UseMigrationsEndPoint();

}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.

app.MapAdditionalIdentityEndpoints();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // 1. Vais buscar o Ambiente (Environment) para saber onde estão as pastas
        var env = services.GetRequiredService<IWebHostEnvironment>();

        // 2. Agora passas o 'env.WebRootPath' como segundo argumento
        // O env.WebRootPath traduz-se em algo como ".../MyCOLL.API/wwwroot"
        await DbInitializer.Initialize(services, env.WebRootPath);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro ao popular a base de dados.");
    }
}
app.Run();