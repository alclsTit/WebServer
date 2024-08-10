namespace ProtoWebServerLab.Middlewares
{

    public class SessionManagement 
    {
        private readonly RequestDelegate m_next;

        public SessionManagement(RequestDelegate next)
        {
            m_next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Serilog.Log.Logger.Information($"SessionManagement.InvokeAsync() called!!!");

            await m_next(context);
        }
    }
}
