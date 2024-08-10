using ProtoWebServerLab.Middlewares;
using ProtoWebServerLab.Redis;
using Serilog;

namespace ProtoWebServerLab.Module
{
    public class ServiceWorker
    {
        private readonly WebApplicationBuilder m_builder;
        private readonly WebApplication m_app;

        public ServiceWorker(string[] args)
        {
            m_builder = WebApplication.CreateBuilder(args);

            RegisterServices();

            m_app = m_builder.Build();
        }

        public bool Initialize()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .CreateLogger();

                if (false == RegisterMiddlewareAll())
                {
                    Serilog.Log.Logger.Error($"Error in ServiceWorker.Initialize() - Func[RegisterMiddlewares(...)] call Error!!!");
                    return false;
                }

                // http 요청이 지정된 앤드포인트로 디스패치 되도록 라우터 설정
                m_app.UseRouting();

                // 특정 라우터 구분없이 컨트롤러 행동에 대한 모든 엔드포인트 추가 
                m_app.MapControllers();

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error($"Exception in ServiceWorker.Initialize() - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public void SetConfigure(ILoggerFactory loggerFactory)
        {
            var process_path = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
            var log_path = @$"{process_path}/logs/serilog.txt";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(log_path)
                .CreateLogger();

            loggerFactory.AddSerilog();
        }

        public bool RegisterMiddlewareAll()
        {
            // Middleware setting
            m_app.UseSessionManagementMiddleware();


            //

            return true;
        }

        public void RegisterMiddleware<TMiddleware>(TMiddleware middleware) where TMiddleware : class
        {
            m_app.UseMiddleware<TMiddleware>();
        }

        private bool RegisterServices()
        {
            // 의존성 주입 진행
            m_builder.Services.AddSingleton<IClusterRedisCache, ClusterRedisCache>();

            // web api 컨트롤러 사용에 필요한 서비스 등록 
            m_builder.Services.AddControllers();

            return true;
        }

        public async Task StartAsync(string url)
        {
            await m_app.RunAsync(url);
        }

        public void Start(string url)
        {
            m_app.Run(url);
        }

    }
}
