using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Service;
using Proyecto_FarmaControl.Repositorio;
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

        // ========================================
        // LECTURA DEL JWT DESDE COOKIES
        // ========================================

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

            OnTokenValidated = async context =>
            {
                var userIdText = context.Principal?
                    .FindFirstValue(ClaimTypes.NameIdentifier);
                var sessionId = context.Principal?
                    .FindFirst(TokenService.SessionClaimType)?.Value;

                if (!int.TryParse(userIdText, out int userId) ||
                    userId <= 0)
                {
                    context.Fail("Sesión inválida.");
                    return;
                }

                var users = context.HttpContext.RequestServices
                    .GetRequiredService<IUsuarioRepository>();
                var user = await users.GetUsuarioByIdWithPermissionAsync(
                    userId);

                if (user == null ||
                    !TokenService.IsCurrentSession(user, sessionId))
                {
                    context.Fail("La sesión fue revocada o expiró.");
                }
            },

            OnChallenge = context =>
            {
                context.HandleResponse();
                return JwtRefreshMiddleware.RespondUnauthenticatedAsync(
                    context.HttpContext);
            }
        };
    });

// ========================================
// 4. CONFIGURACION DE AUTORIZACION
// ========================================

builder.Services.AddAuthorization(options =>
{
    // Estas políticas también usan el handler con consulta a SQL.
    // Evitar RequireAssertion sobre claims antiguos del JWT.
    foreach (var name in new[]
    {
        "admin.panel",
        "permiso.actualizar",
        "permiso.borrar"
    })
    {
        options.AddPolicy(name, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(name)));
    }
});

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

// CATEGORIAS
builder.Services.AddScoped<
    ICategoriaRepository,
    CategoriaRepository>();

// MEDICAMENTOS
builder.Services.AddScoped<
    IMedicamentoRepository,
    MedicamentoRepository>();

// LOTES
builder.Services.AddScoped<
    ILoteRepository,
    LoteRepository>();

// LABORATORIOS
builder.Services.AddScoped<
    ILaboratorioRepository,
    LaboratorioRepository>();

// PRESENTACIONES
builder.Services.AddScoped<
    IPresentacionRepository,
    PresentacionRepository>();

// METODOS DE PAGO
builder.Services.AddScoped<
    IMetodoPagoRepository,
    MetodoPagoRepository>();

// TURNOS DE CAJA
builder.Services.AddScoped<
    ITurnoCajaRepository,
    TurnoCajaRepository>();

// PAGOS
builder.Services.AddScoped<
    IPagoRepository,
    PagoRepository>();

// SUCURSALES
builder.Services.AddScoped<
    ISucursalRepository,
    SucursalRepository>();

// ALMACENES
builder.Services.AddScoped<
    IAlmacenRepository,
    AlmacenRepository>();

// PROVEEDORES
builder.Services.AddScoped<
    IProveedorRepository,
    ProveedorRepository>();

// COMPRAS
builder.Services.AddScoped<
    ICompraRepository,
    CompraRepository>();

// VENTAS
builder.Services.AddScoped<
    IVentaRepository,
    VentaRepository>();

// AJUSTES DE INVENTARIO
builder.Services.AddScoped<
    IAjusteInventarioRepository,
    AjusteInventarioRepository>();

// ========================================
// CLIENTES - NUEVO REGISTRO
// ========================================

builder.Services.AddScoped<
    IClienteRepository,
    ClienteRepository>();

// ========================================
// REPOSITORIOS DE SEGURIDAD
// ========================================

// USUARIOS
builder.Services.AddScoped<
    IUsuarioRepository,
    UsuarioRepository>();

// ROLES
builder.Services.AddScoped<
    IRoleRepository,
    RolRepository>();

// PERMISOS
builder.Services.AddScoped<
    IPermissionRepository,
    PermissionRepository>();

// REPORTES DE SOLO LECTURA
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();

// ========================================
// 8. REGISTRO DE SERVICIOS
// ========================================

// CATEGORIAS
builder.Services.AddScoped<
    ICategoriaService,
    CategoriaService>();

// MEDICAMENTOS
builder.Services.AddScoped<
    IMedicamentoService,
    MedicamentoService>();

// LOTES
builder.Services.AddScoped<
    ILoteService,
    LoteService>();

// LABORATORIOS
builder.Services.AddScoped<
    ILaboratorioService,
    LaboratorioService>();

// PRESENTACIONES
builder.Services.AddScoped<
    IPresentacionService,
    PresentacionService>();

// METODOS DE PAGO
builder.Services.AddScoped<
    IMetodoPagoService,
    MetodoPagoService>();

// TURNOS DE CAJA
builder.Services.AddScoped<
    ITurnoCajaService,
    TurnoCajaService>();

// PAGOS
builder.Services.AddScoped<
    IPagoService,
    PagoService>();

// SUCURSALES
builder.Services.AddScoped<
    ISucursalService,
    SucursalService>();

// ALMACENES
builder.Services.AddScoped<
    IAlmacenService,
    AlmacenService>();

// PROVEEDORES
builder.Services.AddScoped<
    IProveedorService,
    ProveedorService>();

// COMPRAS
builder.Services.AddScoped<
    ICompraService,
    CompraService>();

// VENTAS
builder.Services.AddScoped<
    IVentaService,
    VentaService>();

// AJUSTES DE INVENTARIO
builder.Services.AddScoped<
    IAjusteInventarioService,
    AjusteInventarioService>();

// ========================================
// CLIENTES - NUEVO REGISTRO
// ========================================

builder.Services.AddScoped<
    IClienteService,
    ClienteService>();

// REPORTES DE SOLO LECTURA
builder.Services.AddScoped<IReporteService, ReporteService>();

// ========================================
// SERVICIOS DE SEGURIDAD Y SOPORTE
// ========================================

// TOKENS
builder.Services.AddScoped<
    ITokenService,
    TokenService>();

// CORREOS
builder.Services.AddScoped<
    IEmailService,
    EmailService>();

// AUTENTICACION
builder.Services.AddScoped<
    IAuthService,
    AuthService>();

// ROLES Y PERMISOS
builder.Services.AddScoped<
    IRolePermissionService,
    RolePermissionService>();

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
