namespace ProtoWebServerLab.Config
{
    public class Address
    {
        public string ip { get; set; } = string.Empty;
        public int port { get; set; }
        public string url { get; set; } = string.Empty;
        public int db_index { get; set; }
    }

    // single_connect redis
    public class ListRedis
    {
        // redis db name
        public string name { get; set; } = string.Empty;
        // redis 
        public List<RedisConfig> list { get; set; } = new List<RedisConfig>();
    }

    // multi_connect redis
    public class ListClusterRedis
    {
        // redis db name
        public string name { get; set; } = string.Empty;
        // redis
        public List<ClusterRedisConfig> list { get; set; } = new List<ClusterRedisConfig>();
    }

    public class ConfigEtc
    {
        public List<ListRedis> list_redis { get; set; } = new List<ListRedis>();
        public List<ListClusterRedis> list_cluster_redis { get; set; } = new List<ListClusterRedis>();
    }

}
