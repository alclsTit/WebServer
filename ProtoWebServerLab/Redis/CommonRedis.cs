using System.ComponentModel.Design.Serialization;
using System.Net;
using System.Numerics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Extensions;
using ProtoWebServerLab.Config;
using ProtoWebServerLab.Common;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using StackExchange.Redis.Extensions.Newtonsoft;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Runtime.InteropServices;

namespace ProtoWebServerLab.Redis
{
    #region redis_hash
    public static class RedisHashIndex
    {
        public static readonly int HASH_COUNT = 10;
        public static string GetHashIndexString(long unique_id) => GetHashIndex(unique_id).ToString();
        public static int GetHashIndex(long unique_id) => (int)(unique_id % HASH_COUNT);
    }
    #endregion

    #region redis_interface
    public interface IRedisCache
    {
        public LoginRedisCache LoginRedisCache { get; }

        public LobbyRedisCache LobbyRedisCache { get; }
    }

    public interface IClusterRedisCache
    {
        public LoginClusterRedisCache LoginClusterRedisCache { get; }
        public LobbyClusterRedisCache LobbyClusterRedisCache { get; }
    }
    #endregion

    #region abstract_class
    public abstract class BaseRedisCache
    {
        protected readonly RedisConfig Config;
        private readonly NewtonsoftSerializer m_serializer = new NewtonsoftSerializer();
        protected ConnectionMultiplexer Redis { get; private set; }

        public BaseRedisCache(ILogger<BaseRedisCache> logger, RedisConfig config) 
        {
            try
            {     
                Config = config;

                var redis_config = new StackExchange.Redis.ConfigurationOptions();
                var redis_address = config.address.ip;
                var redis_port = config.address.port;

                if (true == string.IsNullOrEmpty(redis_address) || 0 >= redis_port)
                    return;

                redis_config.EndPoints.Add(redis_address, redis_port);
                redis_config.AllowAdmin = true;
                redis_config.ConnectTimeout = config.connect_timeout_sec * 1000;
                redis_config.ReconnectRetryPolicy = new LinearRetry(config.reconnect_timeout_sec * 1000);
                redis_config.ClientName = config.name;
                redis_config.Password = config.password;
                redis_config.DefaultDatabase = config.address.db_index;
                redis_config.Ssl = config.ssl;
                redis_config.KeepAlive = config.keep_alive_sec;
                redis_config.SyncTimeout = config.sync_timeout_sec * 1000;

                Redis = ConnectionMultiplexer.Connect(redis_config);

                if (false == Redis.IsConnected)
                {
                    Serilog.Log.Error($"Error in BaseRedisCache() - RedisObject Created Fail");
                    Redis.Dispose();
                    return;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Exception in BaseRedisCache() - {ex.Message} - {ex.StackTrace}");
                return;
            }
        }

        public IServer GetServer(System.Net.EndPoint endpoint, object? asyncState = null)
        {
            return Redis.GetServer(endpoint, asyncState);
        }

        public IServer GetServer(IPAddress host, int port)
        {
            return Redis.GetServer(host, port);
        }

        public IServer GetServer(string host, int port, object? asyncState = null)
        {
            return Redis.GetServer(host, port, asyncState);
        }

        public IDatabase GetDatabase()
        {
            return Redis.GetDatabase();
        }

        public ISubscriber GetSubscriber()
        {
            // 모든 구독은 전역. 인스턴스의 생명주기에 따라서 생성되거나 파괴되지않는다
            // ISubscriber.Publish로 채널에 게시, ISubscriber.Subscribe로 채널 메시지 큐의 콜백 대리자 실행
            return Redis.GetSubscriber();
        }

        public System.Net.EndPoint[] GetEndpoints(bool configure_only = false)
        {
            return Redis.GetEndPoints(configure_only);
        }

        public bool KeyDelete(string key, CommandFlags flag = CommandFlags.None)
        {
            return GetDatabase().KeyDelete(key, flag);
        }

        public async Task<bool> KeyDeleteAsync(string key, CommandFlags flag = CommandFlags.None)
        {
            return await GetDatabase().KeyDeleteAsync(key, flag);
        }

        public bool SetData<T>(string key, T value, int expire_sec) where T : class
        {
            return GetDatabase().StringSet(key, m_serializer.Serialize(value), TimeSpan.FromSeconds(expire_sec));
        }

        public async Task<bool> SetDataAsync<T>(string key, T value, int expire_sec) where T : class
        {
            return await GetDatabase().StringSetAsync(key, m_serializer.Serialize(value), TimeSpan.FromSeconds(expire_sec));
        }

        public bool SetHashData<T>(string key, string hash_field_key, T hash_field_value) where T : class
        {
            return GetDatabase().HashSet(key, hash_field_key, m_serializer.Serialize(hash_field_value));
        }

        public async Task<bool> SetHashDataAsync<T>(string key, string hash_field_key, T hash_field_value) where T : class
        {
            return await GetDatabase().HashSetAsync(key, hash_field_key, m_serializer.Serialize(hash_field_value));
        }

        public T? GetHashData<T>(string key, string hash_field_key)
        {
            RedisValue hash_field_value = GetDatabase().HashGet(key, hash_field_key);
            if (false == hash_field_value.HasValue)
                return default;

            return m_serializer.Deserialize<T>(serializedObject: hash_field_value);
        }

        public async ValueTask<T?> GetHashDataAsync<T>(string key, string hash_field_key)
        {
            RedisValue hash_field_value = await GetDatabase().HashGetAsync(key, hash_field_key);
            if (false == hash_field_value.HasValue)
                return default;

            return m_serializer.Deserialize<T>(serializedObject: hash_field_value);
        }

        public bool FlushAll()
        {
            try
            {
                var list_endpoint = Redis.GetEndPoints(true);
                foreach (var item in list_endpoint)
                {
                    var server = GetServer(item);
                    server.FlushAllDatabases();
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Exception in BaseRedisCache.FlushAll() - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public async ValueTask<bool> FlushAllAsync()
        {
            try
            {
                var list_endpoint = Redis.GetEndPoints(true);
                foreach (var item in list_endpoint)
                {
                    var server = GetServer(item);
                    await server.FlushAllDatabasesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Exception in BaseRedisCache.FlushAllAsync() - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public virtual void Dispose()
        {
            Redis.Dispose();
        }
    }

    public abstract class BaseClusterRedisCache 
    {
        protected readonly MultiRedisConfig Config;     
        private readonly NewtonsoftSerializer m_serializer = new NewtonsoftSerializer();
        protected Dictionary<int, ConnectionMultiplexer> list_redis { get; private set; } = new Dictionary<int, ConnectionMultiplexer>();

        public BaseClusterRedisCache(ILogger<BaseClusterRedisCache> logger, MultiRedisConfig config)
        {
            try
            {
                Config = config;

                foreach (var item in config.list_address)
                {
                    var redis_config = new StackExchange.Redis.ConfigurationOptions();
                    if (true == string.IsNullOrEmpty(item.ip) || 0 >= item.port)
                        continue;

                    redis_config.EndPoints.Add(item.ip, item.port);
                    redis_config.AllowAdmin = true;
                    redis_config.ConnectRetry = config.connect_timeout_sec * 1000;
                    redis_config.ReconnectRetryPolicy = new LinearRetry(config.reconnect_timeout_sec * 1000);
                    redis_config.ClientName = config.name;
                    redis_config.Password = config.password;
                    redis_config.DefaultDatabase = item.db_index;
                    redis_config.Ssl = config.ssl;
                    redis_config.KeepAlive = config.keep_alive_sec;
                    redis_config.SyncTimeout = config.sync_timeout_sec* 1000;

                    var local_redis = ConnectionMultiplexer.Connect(redis_config);

                    if (false == local_redis.IsConnected)
                    {
                        logger.LogError($"Error in BaseClusterRedisCache() - [DB_Index: {item.db_index}] RedisObject Created Fail");
                        local_redis.Dispose();
                        continue;
                    }

                    list_redis.Add(item.db_index, local_redis);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception in BaseClusterRedisCache() - {ex.Message} - {ex.StackTrace}");
                return;
            }
        }

        public IServer? GetServerByHostInfo(string address, int port, object? asyncState = null)
        {
            foreach(var local_redis in list_redis)
            {
                var local_server = GetServerByDBIndex(local_redis.Key);
                if (null != local_server)
                {
                    var endpoint = (IPEndPoint)local_server.EndPoint;
                    if (IPAddress.Parse(address) == endpoint.Address && port == endpoint.Port)
                        return local_server;
                }
            }

            return null;

            //var endpoints = (IPEndPoint[])GetEndpoints();
            //IPEndPoint? endpoint = endpoints.Where(e => e.Address == IPAddress.Parse(address) && e.Port == port).FirstOrDefault();

            //return null != endpoint ? GetServer(endpoint, asyncState) : null;
        }

        public IEnumerable<IServer>? GetServerListByDbIndex(int db_index)
        {
            if (false == list_redis.TryGetValue(db_index, out var local_redis))
                yield break;

            var endpoints = (IPEndPoint[])local_redis.GetEndPoints();
            foreach(var item in endpoints)
                yield return local_redis.GetServer(item.Address, item.Port);
        }

        public IServer? GetServerByDBIndex(int db_index)
        {
            if (false == list_redis.TryGetValue(db_index, out var local_redis))
                return null;

            var endpoint = (IPEndPoint?)local_redis.GetEndPoints().FirstOrDefault();  
            return null != endpoint ? local_redis.GetServer(endpoint.Address, endpoint.Port) : null;
        }


        public IDatabase? GetDatabaseByHashIndex(long unique_id)
        {
            var index = RedisHashIndex.GetHashIndex(unique_id);
            if (false == list_redis.TryGetValue(index, out var local_redis))
                return null;

            return local_redis.GetDatabase();
        }

        public ISubscriber? GetSubscriberByHashIndex(long unique_id) 
        {
            var index = RedisHashIndex.GetHashIndex(unique_id);
            if (false == list_redis.TryGetValue(index, out var local_redis))
                return null;

            return local_redis.GetSubscriber();
        }
        #region CommandFunction
        public bool KeyDelete(long unique_id, string key)
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return db.KeyDelete(key);
        }

        public async ValueTask<bool> KeyDeleteAsync(long unique_id, string key)
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return await db.KeyDeleteAsync(key);
        }

        public bool SetData<T>(long unique_id, string key, T value, int expire_sec) where T : class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return db.StringSet(key, m_serializer.Serialize(value), TimeSpan.FromSeconds(expire_sec));
        }

        public async ValueTask<bool> SetDataAsync<T>(long unique_id, string key, T value, int expire_sec) where T: class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return await db.StringSetAsync(key, m_serializer.Serialize(value), TimeSpan.FromSeconds(expire_sec));
        }

        public bool SetHashData<T>(long unique_id, string key, string hash_field_key, T value) where T : class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return db.HashSet(key, hash_field_key, m_serializer.Serialize(value));
        }

        public async ValueTask<bool> SetHashDataAsync<T>(long unique_id, string key, string hash_field_key, T value) where T : class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return false;

            return await db.HashSetAsync(key, hash_field_key, m_serializer.Serialize(value));
        }

        public T? GetHashData<T>(long unique_id, string key, string hash_field_key) where T : class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return default;

            RedisValue hash_field_value = db.HashGet(key, hash_field_key);
            if (false == hash_field_value.HasValue)
                return default;

            return m_serializer.Deserialize<T>(serializedObject: hash_field_value);
        }

        public async ValueTask<T?> GetHashDataAsync<T>(long unique_id, string key, string hash_field_key) where T : class
        {
            var db = GetDatabaseByHashIndex(unique_id);
            if (null == db)
                return default;

            RedisValue hash_field_value = await db.HashGetAsync(key, hash_field_key);
            if (false == hash_field_value.HasValue)
                return default;

            return m_serializer.Deserialize<T>(serializedObject: hash_field_value);
        }

        public bool FlushAll()
        {
            try
            {
                foreach(var item in list_redis)
                {
                    var servers = item.Value.GetServers();
                    foreach (var server in servers)
                        server.FlushAllDatabases();
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Exception in BaseClusterRedisCache.FlushAll() - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }

        public async ValueTask<bool> FlushAllAsync()
        {
            try
            {
                foreach(var item in list_redis)
                {
                    var servers = item.Value.GetServers();
                    foreach (var server in servers)
                        await server.FlushAllDatabasesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Exception in BaseClusterRedisCache.FlushAllAsync() - {ex.Message} - {ex.StackTrace}");
                return false;
            }
        }
        #endregion

        public virtual void Dispose()
        {
            foreach(var item in list_redis)
            {
                item.Value.Dispose();
            }
        }
    }
    #endregion

    #region concrete_class
    public class RedisChache : IRedisCache
    {
        public LoginRedisCache LoginRedisCache { get; private set; }

        public LobbyRedisCache LobbyRedisCache { get; private set; }

        public RedisChache(ILogger<BaseRedisCache> logger)
        {
            // service에 대한 DI를 singleton으로 진행하여 등록하기 때문에 ClusterRedis를 상속한 객체는 프로그램 생명주기에서 계속 재활용되어 사용된다
            var config = ConfigLoader.LoadJson<RedisConfig>("config_etc", ConfigLoader.eFileExtensionType.json);
            if (null == config)
            {
                logger.LogError($"Error in RedisChache() - RedisConfig Created Fail");
                return;
            }

            LoginRedisCache = new LoginRedisCache(logger, config);
            LobbyRedisCache = new LobbyRedisCache(logger, config);
        }
    }

    public class ClusterRedisCache : IClusterRedisCache
    {
        public LoginClusterRedisCache LoginClusterRedisCache { get; private set; }
        public LobbyClusterRedisCache LobbyClusterRedisCache { get; private set; }

        public ClusterRedisCache(ILogger<BaseClusterRedisCache> logger)
        {
            // service에 대한 DI를 singleton으로 진행하여 등록하기 때문에 ClusterRedis를 상속한 객체는 프로그램 생명주기에서 계속 재활용되어 사용된다
            var config = ConfigLoader.LoadJson<MultiRedisConfig>("config_etc", ConfigLoader.eFileExtensionType.json);
            if (null == config)
            {
                logger.LogError($"Error in ClusterRedisCache() - RedisConfig Created Fail");
                return;
            }

            LoginClusterRedisCache = new LoginClusterRedisCache(logger, config);
            LobbyClusterRedisCache = new LobbyClusterRedisCache(logger, config);
        }
    }
    #endregion
}
