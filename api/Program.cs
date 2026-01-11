using System.Text;
using api.Data;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IO;

// Build the app
var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "https://decpwa.web.app",
    "https://decpwa.firebaseapp.com"
};

// Parse Railway DATABASE_URL into Npgsql connection string
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // DATABASE_URL format: postgres://username:password@host:port/dbname
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    var builderNpgsql = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Prefer
    };

    connectionString = builderNpgsql.ConnectionString;
}
else
{
    // Null-safe fallback
    connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new Exception("Default connection string not found in configuration.");
}

// Optional: log which connection string is being used (without password)
var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
Console.WriteLine($"Using database connection string: Host={npgsqlBuilder.Host};Port={npgsqlBuilder.Port};Username={npgsqlBuilder.Username};Database={npgsqlBuilder.Database};SSL Mode={npgsqlBuilder.SslMode}");

// Add services
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

// Configure forwarded headers for proxies (Railway / ACA / Fly.io)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Data Protection to persist keys to Railway volume
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/persisted-keys"))
    .SetApplicationName("talented-contentment");

var app = builder.Build();

// Apply migrations
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

// Configure middleware
app.UseForwardedHeaders();
app.UseCors("PwaCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
