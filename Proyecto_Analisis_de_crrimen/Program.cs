using Microsoft.EntityFrameworkCore;
using Proyecto_Analisis_de_crimen.Models;
using Proyecto_Analisis_de_crimen.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// REGISTRO DE SERVICIOS
// ============================================

// Registrar el servicio de controladores y vistas (MVC)
// Esto permite que la aplicación use el patrón Model-View-Controller
builder.Services.AddControllersWithViews();

// ============================================
// CONFIGURACIÓN DE BASE DE DATOS (Entity Framework Core)
// ============================================
// Obtener la cadena de conexión desde appsettings.json
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

// Registrar el servicio de autenticación
// Maneja la validación de credenciales y verificación de usuarios
builder.Services.AddScoped<Proyecto_Analisis_de_crimen.Services.AuthenticationService>();

// ============================================
// CONFIGURACIÓN DE SESIONES HTTP
// ============================================
// Las sesiones permiten almacenar información del usuario entre peticiones HTTP.
// En este sistema, se usan para mantener el estado de autenticación.

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // La sesión expira después de 30 min de inactividad
    options.Cookie.HttpOnly = true;                  // Prevenir acceso desde JavaScript (protección XSS)
    options.Cookie.IsEssential = true;               // Cookie esencial (no requiere consentimiento GDPR)
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;  // HTTPS si está disponible
});

// ============================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ============================================
var app = builder.Build();

// ============================================
// INICIALIZACIÓN DE BASE DE DATOS
// ============================================
// Verificar y crear la base de datos si no existe al iniciar la aplicación.
// Esto es útil para desarrollo, pero en producción se recomienda usar migraciones.

try
{
    // Crear un scope para acceder a los servicios registrados
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
                // Crear la base de datos y las tablas si no existen
                // EnsureCreatedAsync crea la BD y el esquema basado en los modelos
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
            // Asegurar que las tablas existan (por si la BD existe pero no las tablas)
            await context.Database.EnsureCreatedAsync();
        }
    }
}
catch (Exception ex)
{
    // Si hay un error, registrarlo pero no detener la aplicación
    // Esto permite que la app inicie aunque haya problemas con la BD
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error al verificar la conexión a la base de datos: {Message}", ex.Message);
    // No detenemos la aplicación, solo registramos el error
}

// ============================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ============================================
// El pipeline define el orden en que se procesan las peticiones HTTP.
// El orden es CRÍTICO: cada middleware procesa la petición en secuencia.

// Configurar manejo de errores según el ambiente
if (!app.Environment.IsDevelopment())
{
    // En producción: usar página de error personalizada
    app.UseExceptionHandler("/Home/Error");
    // HSTS (HTTP Strict Transport Security): forzar HTTPS
    app.UseHsts();
}

// Redirigir peticiones HTTP a HTTPS (seguridad)
app.UseHttpsRedirection();

// Servir archivos estáticos (CSS, JS, imágenes) desde wwwroot
app.UseStaticFiles();

// Habilitar enrutamiento de peticiones a controladores
app.UseRouting();

// IMPORTANTE: Session debe ir antes de UseAuthorization
app.UseSession();

// Middleware personalizado para autenticación basada en sesión
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
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, rolId.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(rolNombre))
        {
            claims.Add(new System.Security.Claims.Claim("RolNombre", rolNombre));
        }

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "Session");
        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
    }

    await next();
});

// Habilitar autorización (verifica permisos basados en los claims)
app.UseAuthorization();

// ============================================
// CONFIGURACIÓN DE RUTAS
// ============================================
// Define cómo se mapean las URLs a controladores y acciones
// Patrón: /{controller}/{action}/{id?}
// Ejemplo: /EscenaCrimen/Index/5 -> EscenaCrimenController.Index(5)

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================
// INICIAR LA APLICACIÓN
// ============================================
// La aplicación comienza a escuchar peticiones HTTP
app.Run();