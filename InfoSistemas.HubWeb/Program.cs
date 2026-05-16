// InfoSistemas HubWeb v6.0 - Build 2026-05-16
using InfoSistemas.HubWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar porta do Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "5200";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient<LicencaApiService>(c =>
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
