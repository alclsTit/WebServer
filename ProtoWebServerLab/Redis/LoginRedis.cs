using ProtoWebServerLab.Config;

namespace ProtoWebServerLab.Redis
{
    public class LoginRedisCache : BaseRedisCache
    {
        public LoginRedisCache(ILogger<BaseRedisCache> logger, RedisConfig config) 
            : base(logger, config) 
        {
        }
    }

    public class LoginClusterRedisCache : BaseClusterRedisCache
    {
        public LoginClusterRedisCache(ILogger<BaseClusterRedisCache> logger, ClusterRedisConfig config)
            : base(logger, config)
        {
        }
    }
}
