// InfoSistemas HubWeb v6.0
using InfoSistemas.HubWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Estado de autenticacao por sessao Blazor (scoped)
builder.Services.AddScoped<AuthState>();

// LicencaApiService chama os endpoints /api/admin/... (precisam de X-Admin-Key)
builder.Services.AddHttpClient<LicencaApiService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LicencaApi:BaseUrl"] ?? "http://localhost:5100");
    c.DefaultRequestHeaders.Add("X-Admin-Key",
        builder.Configuration["LicencaApi:AdminKey"] ?? "INFOSISTEMAS-ADMIN-KEY-2026");
});

// AuthApiService chama /api/auth/login (NAO precisa de X-Admin-Key)
builder.Services.AddHttpClient<AuthApiService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LicencaApi:BaseUrl"] ?? "http://localhost:5100");
});

// UsuarioApiService chama /api/admin/usuarios (precisa de X-Admin-Key)
builder.Services.AddHttpClient<UsuarioApiService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LicencaApi:BaseUrl"] ?? "http://localhost:5100");
    c.DefaultRequestHeaders.Add("X-Admin-Key",
        builder.Configuration["LicencaApi:AdminKey"] ?? "INFOSISTEMAS-ADMIN-KEY-2026");
});

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
