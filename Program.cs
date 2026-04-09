using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using PlataformaEscolar.API.Data;
using PlataformaEscolar.API.Middleware;
using PlataformaEscolar.API.Security;
using PlataformaEscolar.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURAR BASE DE DATOS
// ========================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionString no configurada. Usa: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"tu_conexion\"");
    }
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ========================================
// 2. CONFIGURAR AUTENTICACIÓN JWT
// ========================================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key no configurada o muy corta (<32 caracteres). Usa: dotnet user-secrets set \"Jwt:Key\" \"clave_de_32_caracteres\"");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// 3. CONFIGURAR CORS SEGURO
// ========================================
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5033" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Content-Length", "X-JSON-Response");
    });
});

// ========================================
// 4. CONFIGURAR RATE LIMITING
// ========================================
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*/api/auth/login",
            Limit = 5,
            Period = "1m"
        },
        new RateLimitRule
        {
            Endpoint = "*/api/auth/register",
            Limit = 10,
            Period = "1h"
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };

    options.IpWhitelist = new List<string> { "127.0.0.1" };
});

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

// ========================================
// 5. CONFIGURAR VALIDACIÓN FLUENT
// ========================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDTO>();

// ========================================
// 6. REGISTRAR SERVICIOS DE SEGURIDAD
// ========================================
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISecurityLogger, SecurityLogger>();

// ========================================
// 7. CONFIGURAR CONTROLADORES
// ========================================
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Ingresa el token JWT con prefijo 'Bearer '"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// ========================================
// 8. CONFIGURAR PIPELINE MIDDLEWARE
// ========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PlataformaEscolar API v1");
        options.RoutePrefix = "swagger";
    });
}

// ========================================
// 9. MIDDLEWARE DE SEGURIDAD
// ========================================
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseIpRateLimiting();
app.UseCors("AllowSpecific");
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ========================================
// 10. CREAR BD SI NO EXISTE
// ========================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("Base de datos verificada/creada");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error verificando BD: {ex.Message}");
    }
}

// ========================================
// 11. INICIAR APLICACIÓN
// ========================================
app.Run();

