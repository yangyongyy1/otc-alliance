using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Castle.Core.Logging;
using StackExchange.Redis;

namespace ClientPlatform.Infrastructure
{
    /// <summary>
    /// 基于 Redis 的分布式锁实现
    /// </summary>
    public class RedisDistributedLockService : IDistributedLockService, ITransientDependency
    {
        private readonly IDatabase _redis;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        public RedisDistributedLockService(IConnectionMultiplexer redisConnection)
        {
            _redis = redisConnection.GetDatabase();
        }

        /// <summary>
        /// 获取分布式锁
        /// </summary>
        public async Task<IDisposable> AcquireLockAsync(string key, TimeSpan expiry)
        {
            var lockValue = Guid.NewGuid().ToString(); // 唯一标识，防止误释放其他实例的锁
            var lockKey = $"DistributedLock:{key}";

            try
            {
                // SET NX EX：仅当键不存在时设置，并设置过期时间
                var acquired = await _redis.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);

                if (!acquired)
                {
                    Logger.Debug($"获取分布式锁失败===Key={key}");
                    return null; // 锁已被其他实例持有
                }

                Logger.Debug($"成功获取分布式锁===Key={key}===Value={lockValue}===Expiry={expiry.TotalSeconds}s");
                return new RedisLock(_redis, lockKey, lockValue, Logger);
            }
            catch (Exception ex)
            {
                Logger.Error($"获取分布式锁异常===Key={key}===Error={ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Redis 锁对象（自动释放）
        /// </summary>
        private class RedisLock : IDisposable
        {
            private readonly IDatabase _redis;
            private readonly string _key;
            private readonly string _value;
            private readonly ILogger _logger;

            public RedisLock(IDatabase redis, string key, string value, ILogger logger)
            {
                _redis = redis;
                _key = key;
                _value = value;
                _logger = logger;
            }

            public void Dispose()
            {
                try
                {
                    // Lua 脚本：仅当值匹配时才删除（防止误删其他实例的锁）
                    var script = @"
                        if redis.call('get', KEYS[1]) == ARGV[1] then
                            return redis.call('del', KEYS[1])
                        else
                            return 0
                        end";

                    _redis.ScriptEvaluate(script, new RedisKey[] { _key }, new RedisValue[] { _value });
                    _logger.Debug($"成功释放分布式锁===Key={_key}");
                }
                catch (Exception ex)
                {
                    _logger.Warn($"释放分布式锁失败===Key={_key}===Error={ex.Message}", ex);
                }
            }
        }
    }
}
