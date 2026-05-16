// InfoSistemas HubWeb v6.0
using InfoSistemas.HubWeb.Services;

var builder = WebApplication.CreateBuilder(args);

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
