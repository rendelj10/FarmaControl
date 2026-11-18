
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Repositories;
using Proyecto_FarmaControl.Services;
using Proyecto_FarmaControl.Models.settings;
using Proyecto_FarmaControl.Middlewares;
using Proyecto_FarmaControl.Mapping;
using Proyecto_FarmaControl.Entensions;

using static Proyecto_FarmaControl.Authorication.JwtPermissionPolicies;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURACION MVC
// ========================================

builder.Services.AddControllersWithViews();

// ========================================
// 2. CONFIGURACION SQL SERVER
// ========================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========================================
// 3. CONFIGURACION JWT
// ========================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.Key) ||
    Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException(
        "La clave JWT debe tener al menos 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "Debe configurar Issuer y Audience.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)),

                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };

        // Leer JWT desde cookies seguras
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.HttpContext.Items.TryGetValue(
                    JwtRefreshMiddleware.RefreshedTokenKey,
                    out var refreshedToken) &&
                    refreshedToken is string token &&
                    !string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }
                else
                {
                    context.Token =
                        context.Request.Cookies["access_token"];
                }

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.Redirect("/Auth/Login");

                return Task.CompletedTask;
            }
        };
    });

// ========================================
// 4. CONFIGURACION AUTORIZACION
// ========================================

builder.Services.AddAuthorization();

builder.Services.AddSingleton<
    IAuthorizationPolicyProvider,
    Proyecto_FarmaControl.Authorication.JwtPermissionPolicies.PermissionPolicyProvider>();

builder.Services.AddScoped<
    IAuthorizationHandler,
    Proyecto_FarmaControl.Authorication.JwtPermissionPolicies.PermissionAuthorizationHandler>();

// ========================================
// 5. CONFIGURACION SMTP
// ========================================

builder.Services.Configure<SmtpSetting>(
    builder.Configuration.GetSection("Smtp"));

// ========================================
// 6. CONFIGURACION AUTOMAPPER
// ========================================

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<Proyecto_FarmaControl.Mapping.UserProfile>();
});

// ========================================
// 7. REGISTRO DE REPOSITORIOS
// ========================================

builder.Services.AddScoped<
    ICategoriaRepository,
    CategoriaRepository>();

builder.Services.AddScoped<
    IMedicamentoRepository,
    MedicamentoRepository>();

builder.Services.AddScoped<
    ILoteRepository,
    LoteRepository>();

builder.Services.AddScoped<
    IUsuarioRepository,
    UsuarioRepository>();

builder.Services.AddScoped<
    IRoleRepository,
    RolRepository>();

// ========================================
// 8. REGISTRO DE SERVICIOS
// ========================================

builder.Services.AddScoped<
    ICategoriaService,
    CategoriaService>();

builder.Services.AddScoped<
    IMedicamentoService,
    MedicamentoService>();

builder.Services.AddScoped<
    ILoteService,
    LoteService>();

builder.Services.AddScoped<
    ITokenService,
    TokenService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

// ========================================
// 9. CONSTRUIR APLICACION
// ========================================

var app = builder.Build();

// ========================================
// 10. CONFIGURACION DE ERRORES
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ========================================
// 11. CONFIGURACION HTTP
// ========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// ========================================
// 12. RENOVACION AUTOMATICA JWT
// ========================================

app.UseMiddleware<JwtRefreshMiddleware>();

// ========================================
// 13. AUTENTICACION Y AUTORIZACION
// ========================================

app.UseAuthentication();

app.UseAuthorization();

// ========================================
// 14. RUTAS MVC
// ========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========================================
// 15. INICIAR APLICACION
// ========================================

app.Run();
