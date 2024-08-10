using ProtoWebServerLab.Config;

namespace ProtoWebServerLab.Redis
{
    public class LobbyRedisCache : BaseRedisCache
    {
        public LobbyRedisCache(ILogger<BaseRedisCache> logger, RedisConfig config) 
            : base(logger, config) 
        { 
        }
    }

    public class LobbyClusterRedisCache : BaseClusterRedisCache
    {
        public LobbyClusterRedisCache(ILogger<BaseClusterRedisCache> logger, ClusterRedisConfig config) 
            : base(logger, config) 
        {
        }
    }
}
