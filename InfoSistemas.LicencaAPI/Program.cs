using InfoSistemas.LicencaAPI.Data;
using InfoSistemas.LicencaAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Le a connection string — suporta variavel de ambiente direta do Railway
// Railway: MYSQL_CONNECTION_STRING
// Local:   ConnectionStrings:MySql no appsettings.json
var connStr =
    Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("MySql")
    ?? throw new Exception("MYSQL_CONNECTION_STRING nao configurada!");

// Usa versao fixa do MySQL (evita AutoDetect que requer conexao na inicializacao)
// Compativel com MySQL 5.7+ e MariaDB
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

builder.Services.AddDbContext<LicencaDbContext>(opt =>
    opt.UseMySql(connStr, serverVersion, mySqlOpt =>
    {
        mySqlOpt.EnableRetryOnFailure(3);
        mySqlOpt.CommandTimeout(30);
    }));

builder.Services.AddScoped<LicencaService>();
builder.Services.AddScoped<AlertaService>();
builder.Services.AddHostedService<AlertaBackgroundService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InfoSistemas LicencaAPI", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new()
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
});

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Criar/migrar banco automaticamente
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<LicencaDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[AVISO] EnsureCreated: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

// Health check simples
app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

app.Run();
