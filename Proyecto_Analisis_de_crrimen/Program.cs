using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar Entity Framework Core con SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada en appsettings.json");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

    // Logging detallado solo en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Registrar el servicio de comparaci�n (CORE del sistema)
builder.Services.AddScoped<ComparacionService>();

// Configurar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Verificar y crear la base de datos si no existe
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        // Intentar conectar a la base de datos
        var canConnect = context.Database.CanConnect();
        if (!canConnect)
        {
            logger.LogWarning("No se pudo conectar a la base de datos. Intentando crear la base de datos...");
            try
            {
                // Crear la base de datos si no existe
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Base de datos creada exitosamente.");
            }
            catch (Exception dbEx)
            {
                logger.LogError(dbEx, "Error al crear la base de datos: {Message}", dbEx.Message);
            }
        }
        else
        {
            logger.LogInformation("Conexión a la base de datos establecida correctamente.");
            // Asegurar que las tablas existan
            await context.Database.EnsureCreatedAsync();
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al verificar la conexión a la base de datos: {Message}", ex.Message);
    // No detenemos la aplicación, solo registramos el error
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();