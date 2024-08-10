using ProtoWebServerLab;
using ProtoWebServerLab.Module;
using Serilog;

{
    var worker = new ServiceWorker(args);
    if (false == worker.Initialize())
        return;

    await worker.StartAsync("http://0.0.0.0:8080");
}