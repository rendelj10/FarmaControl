
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Repositories;
using Proyecto_FarmaControl.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar MVC y controladores API
builder.Services.AddControllersWithViews();

// Obtener la cadena de conexión desde la configuración
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión DefaultConnection.");
}

// Configurar SQL Server y Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar repositorios
builder.Services.AddScoped<
    ICategoriaRepository,
    CategoriaRepository>();

builder.Services.AddScoped<
    IMedicamentoRepository,
    MedicamentoRepository>();

// Registrar servicios
builder.Services.AddScoped<
    ICategoriaService,
    CategoriaService>();

builder.Services.AddScoped<
    IMedicamentoService,
    MedicamentoService>();

// Construir la aplicación
var app = builder.Build();

// Configuración de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configuración HTTP
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Rutas de controladores API
app.MapControllers();

// Rutas MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
