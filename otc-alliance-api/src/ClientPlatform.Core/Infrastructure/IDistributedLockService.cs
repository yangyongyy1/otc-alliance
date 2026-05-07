using System;
using System.Threading.Tasks;

namespace ClientPlatform.Infrastructure
{
    /// <summary>
    /// 分布式锁服务接口
    /// </summary>
    public interface IDistributedLockService
    {
        /// <summary>
        /// 获取分布式锁
        /// </summary>
        /// <param name="key">锁的唯一标识</param>
        /// <param name="expiry">锁的过期时间（防止死锁）</param>
        /// <returns>锁对象，Dispose 时自动释放锁；如果获取失败返回 null</returns>
        Task<IDisposable> AcquireLockAsync(string key, TimeSpan expiry);
    }
}
