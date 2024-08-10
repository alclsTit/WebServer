using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using ProtoWebServerLab.Redis;

namespace ProtoWebServerLab.Controllers
{
    [Route("api/Login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IClusterRedisCache ClusterRedis;

        public LoginController(IClusterRedisCache redis)
        {
            ClusterRedis = redis;
        }

        [HttpGet]
        public async Task LoginAsyncTest()
        {
            await Task.Delay(1000);
            await Console.Out.WriteLineAsync($"LoginAsyncTest Method Callled!!!");
        }

        [HttpPost("{access_token}")]
        public async Task LoginAsync([FromBody] long access_token)
        {
            //await ClusterRedis.LoginClusterRedisCache.SetHashDataAsync();

            await Console.Out.WriteLineAsync($"LoginAsync Func Called - data = {access_token}");
            await Task.Delay(100);
        }
    }
}
