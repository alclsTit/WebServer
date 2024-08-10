using System.Runtime.CompilerServices;

namespace ProtoWebServerLab.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionManagementMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<SessionManagement>();
    }
}
