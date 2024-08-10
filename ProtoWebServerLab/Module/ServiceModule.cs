namespace ProtoWebServerLab.Module
{
    public class ServiceModule
    {
        public ServiceWorker Worker { get; private set; }

        public ServiceModule(string[] args)
        {
            Worker = new ServiceWorker(args);
        }

        public bool Initialize()
        {
            if (false == Worker.Initialize())
            {
                //log
                return false;
            }

            return true;
        }

        public bool Start(string url)
        {
            Worker.Start(url);

            return true;
        }

        public async Task<bool> StartAsync(string url)
        {
            await Worker.StartAsync(url);

            return true;
        }
    }
}
