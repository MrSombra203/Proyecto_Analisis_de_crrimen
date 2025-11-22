using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;

var builder = WebApplication.CreateBuilder(args);

// REGISTRO DE SERVICIOS

// MVC: Controladores y vistas
builder.Services.AddControllersWithViews();

// BASE DE DATOS: Entity Framework Core con SQL Server
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

// SERVICIOS PERSONALIZADOS
builder.Services.AddScoped<ComparacionService>();
builder.Services.AddScoped<Proyecto_Analisis_de_crimen.Services.AuthenticationService>();

// SESIONES HTTP: Para mantener estado de autenticación entre peticiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Protección XSS
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// INICIALIZACIÓN DE BASE DE DATOS
// Verifica conexión y crea BD/tablas si no existen (útil para desarrollo)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        var canConnect = context.Database.CanConnect();
        
        if (!canConnect)
        {
            logger.LogWarning("No se pudo conectar a la base de datos. Intentando crear...");
            try
            {
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
            await context.Database.EnsureCreatedAsync();
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al verificar la conexión a la base de datos: {Message}", ex.Message);
}

// PIPELINE HTTP (el orden es crítico)

// Manejo de errores (producción)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Forzar HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Archivos estáticos desde wwwroot
app.UseRouting();
app.UseSession(); // IMPORTANTE: Debe ir antes de UseAuthorization

// Middleware personalizado: Autenticación basada en sesión
app.Use(async (context, next) =>
{
    var session = context.Session;
    var userId = session.GetInt32("UserId");
    var nombreUsuario = session.GetString("NombreUsuario");
    var rolId = session.GetInt32("RolId");
    var rolNombre = session.GetString("RolNombre");

    if (userId.HasValue && !string.IsNullOrEmpty(nombreUsuario))
    {
        // Crear claims para el usuario autenticado
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, nombreUsuario)
        };

        if (rolId.HasValue)
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, rolId.Value.ToString()));

        if (!string.IsNullOrEmpty(rolNombre))
            claims.Add(new System.Security.Claims.Claim("RolNombre", rolNombre));

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Session");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }

    await next();
});

app.UseAuthorization();

// RUTAS: Mapeo de URLs a controladores
// Patrón: /{controller}/{action}/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
