
using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Data;

using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Repositories;
using Proyecto_FarmaControl.Interfaces;
using Proyecto_FarmaControl.Repositories;
using Proyecto_FarmaControl.Services;



var builder = WebApplication.CreateBuilder(args);

// Configurar MVC
builder.Services.AddControllersWithViews();

// Configurar conexión con SQL Server

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<
    ICategoriaRepository,
    CategoriaRepository>();

builder.Services.AddScoped<
    ICategoriaRepository,
    CategoriaRepository>();

builder.Services.AddScoped<
    ICategoriaService,
    CategoriaService>();




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

// Rutas MVC
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

    


app.Run();
