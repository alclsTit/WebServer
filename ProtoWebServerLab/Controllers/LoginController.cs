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

        [HttpPost("{access_token}")]
        public async Task LoginAsync(long access_token)
        {
            await ClusterRedis.LoginClusterRedisCache.SetHashDataAsync();

            await Console.Out.WriteLineAsync($"LoginAsync Func Called");
            await Task.Delay(100);
        }
    }
}
