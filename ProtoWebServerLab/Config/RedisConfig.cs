namespace ProtoWebServerLab.Config
{
    public class RedisConfig 
    {
        public readonly string name;
        public readonly Address address;
        public readonly string password;
        public readonly bool ssl;
        public readonly int keep_alive_sec;
        public readonly int connect_timeout_sec;
        public readonly int reconnect_timeout_sec;
        public readonly int sync_timeout_sec;

        public RedisConfig(string name, Address address, string password, bool ssl,
                           int keep_alive_sec, int connect_timeout_sec, int reconnect_timeout_sec,
                           int sync_timeout_sec)
        {
            this.name = name;
            this.address = address;
            this.password = password;
            this.ssl = ssl;
            this.keep_alive_sec = keep_alive_sec;
            this.connect_timeout_sec = connect_timeout_sec;
            this.reconnect_timeout_sec = reconnect_timeout_sec;
            this.sync_timeout_sec = sync_timeout_sec;
        }
    }

    public class ClusterRedisConfig
    {
        public readonly string name;
        public readonly List<Address> list_address;
        public readonly string password;
        public readonly bool ssl;
        public readonly int keep_alive_sec;
        public readonly int connect_timeout_sec;
        public readonly int reconnect_timeout_sec;
        public readonly int sync_timeout_sec;

        public ClusterRedisConfig(string name, List<Address> list_address, string password, bool ssl,
                                  int keep_alive_sec, int connect_timeout_sec, int reconnect_timeout_sec,
                                  int sync_timeout_sec)
        {
            this.name = name;
            this.list_address = list_address;
            this.password = password;
            this.ssl = ssl;
            this.keep_alive_sec = keep_alive_sec;
            this.connect_timeout_sec = connect_timeout_sec;
            this.reconnect_timeout_sec = reconnect_timeout_sec;
            this.sync_timeout_sec = sync_timeout_sec;
        }
    }
}
