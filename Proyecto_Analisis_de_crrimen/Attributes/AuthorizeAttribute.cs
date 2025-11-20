using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;

namespace Proyecto_Analisis_de_crimen.Attributes
{
    /// <summary>
    /// Atributo personalizado que requiere que el usuario esté autenticado para acceder a una acción.
    /// 
    /// Este atributo implementa el patrón Action Filter de ASP.NET Core y se ejecuta
    /// ANTES de que se ejecute la acción del controlador.
    /// 
    /// Funcionamiento:
    /// 1. Verifica si existe un UserId en la sesión (indica usuario autenticado)
    /// 2. Si NO está autenticado, redirige al login guardando la URL original
    /// 3. Después del login, el usuario es redirigido a la página que intentaba acceder
    /// 
    /// Uso: [RequireAuth] antes de cualquier acción que requiera autenticación
    /// </summary>
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método que se ejecuta ANTES de la acción del controlador.
        /// 
        /// Este es el punto de intercepción donde verificamos la autenticación.
        /// Si el usuario no está autenticado, cancelamos la ejecución de la acción
        /// y redirigimos al login.
        /// </summary>
        /// <param name="context">Contexto de la acción que contiene información de la petición HTTP</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtener el ID del usuario desde la sesión
            // Si no hay sesión activa, userId será null
            var userId = context.HttpContext.Session.GetInt32("UserId");
            
            // Si no hay usuario autenticado, redirigir al login
            if (!userId.HasValue)
            {
                // Guardar la URL de retorno para redirigir después del login
                // Esto permite que el usuario vuelva a la página que intentaba acceder
                var request = context.HttpContext.Request;
                var returnUrl = request.Path + request.QueryString;
                
                // Codificar la URL para que sea segura en el query string
                // Ejemplo: "/EscenaCrimen/Index" se convierte en "%2FEscenaCrimen%2FIndex"
                var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
                var loginUrl = $"/Auth/Login?returnUrl={encodedReturnUrl}";
                
                // Cancelar la ejecución de la acción y redirigir al login
                context.Result = new RedirectResult(loginUrl);
            }
            
            // Si el usuario está autenticado, continuar con la ejecución normal
            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Atributo personalizado que requiere que el usuario sea Administrador para acceder a una acción.
    /// 
    /// Este atributo es más restrictivo que RequireAuth, ya que además de requerir autenticación,
    /// también verifica que el usuario tenga el rol de Administrador (RolId = 1).
    /// 
    /// Funcionamiento:
    /// 1. Verifica que el usuario esté autenticado (UserId existe)
    /// 2. Verifica que el RolId sea igual a 1 (Administrador)
    /// 3. Si alguna condición falla, redirige a la página de "Acceso Denegado"
    /// 
    /// Uso: [RequireAdmin] antes de acciones que solo administradores pueden ejecutar
    /// </summary>
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Método que se ejecuta ANTES de la acción del controlador.
        /// 
        /// Verifica tanto la autenticación como el rol de administrador.
        /// </summary>
        /// <param name="context">Contexto de la acción que contiene información de la petición HTTP</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtener información del usuario desde la sesión
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var rolId = context.HttpContext.Session.GetInt32("RolId");

            // Verificar que el usuario esté autenticado Y sea administrador
            // RolId = 1 corresponde al rol de Administrador en el sistema
            if (!userId.HasValue || rolId != 1)
            {
                // Si no cumple los requisitos, redirigir a la página de acceso denegado
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "AccessDenied" })
                );
            }
            
            // Si el usuario es administrador, continuar con la ejecución normal
            base.OnActionExecuting(context);
        }
    }
}




