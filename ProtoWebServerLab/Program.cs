using ProtoWebServerLab;
using ProtoWebServerLab.Module;
using Serilog;

{
    var module = new ServiceModule(args);
    if (false == module.Initialize())
        return;

    await module.StartAsync("http://localhost:5273");
}