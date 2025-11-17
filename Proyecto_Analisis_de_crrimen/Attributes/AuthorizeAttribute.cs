using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;

namespace Proyecto_Analisis_de_crimen.Attributes
{
    public class RequireAuthAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Guardar la URL de retorno para redirigir después del login
                var request = context.HttpContext.Request;
                var returnUrl = request.Path + request.QueryString;
                
                // Usar RedirectToAction con query string para el returnUrl
                var encodedReturnUrl = Uri.EscapeDataString(returnUrl);
                var loginUrl = $"/Auth/Login?returnUrl={encodedReturnUrl}";
                context.Result = new RedirectResult(loginUrl);
            }
            base.OnActionExecuting(context);
        }
    }

    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var rolId = context.HttpContext.Session.GetInt32("RolId");

            if (!userId.HasValue || rolId != 1) // 1 = Administrador
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Auth", action = "AccessDenied" })
                );
            }
            base.OnActionExecuting(context);
        }
    }
}




