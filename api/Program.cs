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
using api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Allowed origins for your PWA + local dev
var allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://localhost:4200",
    "https://decpwa.web.app",
    "https://decpwa.firebaseapp.com"
};

// Parse Railway DATABASE_URL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    var builderNpgsql = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };

    connectionString = builderNpgsql.ConnectionString;
}
else
{
    connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new Exception("Default connection string not found.");
}

// Log connection string (without password)
var npgsqlBuilder = new NpgsqlConnectionStringBuilder(connectionString);
Console.WriteLine($"Using DB: Host={npgsqlBuilder.Host}; Port={npgsqlBuilder.Port}; User={npgsqlBuilder.Username}; Database={npgsqlBuilder.Database}; SSL={npgsqlBuilder.SslMode}");

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
builder.Services.AddScoped<ITokenService, TokenService>();

// CORS policy: allow PWA origins, any header/method, and credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("PwaCorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
//          .AllowCredentials(); // important for auth headers
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY") 
                       ?? builder.Configuration["TokenKey"];

        if (string.IsNullOrEmpty(tokenKey))
            throw new Exception("Token key not found");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Forwarded headers for proxies
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Data Protection keys persisted to Railway volume
if (Directory.Exists("/persisted-keys"))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/persisted-keys"))
        .SetApplicationName("talented-contentment");
}
else
{
    Console.WriteLine("Warning: /persisted-keys volume not found. Using ephemeral keys.");
}

// Use PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

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
        Console.WriteLine($"Error migrating DB: {ex.Message}");
        throw;
    }
}

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseForwardedHeaders();
// app.UseHttpsRedirection();
app.UseRouting();

// Handle preflight OPTIONS requests automatically
app.UseCors("PwaCorsPolicy");

// Optional: catch-all for OPTIONS requests (extra safety)
// app.Use(async (context, next) =>
// {
//     if (context.Request.Method == "OPTIONS")
//     {
//         context.Response.StatusCode = 200;
//         await context.Response.CompleteAsync();
//         return;
//     }
//     await next();
// });

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
