using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;

namespace Proyecto_Analisis_de_crimen.Attributes
{
    /// <summary>
    /// Requiere autenticación para acceder a una acción. Verifica UserId en sesión.
    /// Si no está autenticado, redirige al login guardando returnUrl para volver después.
    /// </summary>
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Se ejecuta ANTES de la acción. Verifica autenticación y redirige si es necesario.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            
            if (!userId.HasValue)
            {
                // Guardar URL original para redirigir después del login
                var request = context.HttpContext.Request;
                var returnUrl = request.Path + request.QueryString;
                var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
                var loginUrl = $"/Auth/Login?returnUrl={encodedReturnUrl}";
                
                context.Result = new RedirectResult(loginUrl);
            }
            
            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Requiere que el usuario sea Administrador (RolId = 1) para acceder a una acción.
    /// Redirige a AccessDenied si no cumple los requisitos.
    /// </summary>
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Se ejecuta ANTES de la acción. Verifica autenticación y rol de administrador.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var rolId = context.HttpContext.Session.GetInt32("RolId");

            if (!userId.HasValue || rolId != 1)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "AccessDenied" })
                );
            }
            
            base.OnActionExecuting(context);
        }
    }
}
