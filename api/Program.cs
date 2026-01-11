using System.Text;
using api.Data;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "https://decpwa.web.app",
    "https://decpwa.firebaseapp.com"
};

// -------------------------------
// DATABASE CONNECTION
// -------------------------------
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default");

// Convert Railway DATABASE_URL (postgres://user:pass@host:port/db) to Npgsql format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');

    var builderConn = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };

    connectionString = builderConn.ToString();
}

// -------------------------------
// SERVICES
// -------------------------------
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(connectionString);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PwaCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Read token key from environment variable first, fallback to appsettings.json
        var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY") 
               ?? builder.Configuration["TokenKey"];

        if (string.IsNullOrEmpty(tokenKey))
            throw new Exception("Token key not found - Program.cs");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Trust all proxy networks (required for Railway / Fly / ACA)
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// -------------------------------
// APPLY MIGRATIONS
// -------------------------------
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error migrating database: {ex.Message}");
        throw;
    }
}

// -------------------------------
// MIDDLEWARE
// -------------------------------
app.UseForwardedHeaders();

app.UseCors("PwaCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
