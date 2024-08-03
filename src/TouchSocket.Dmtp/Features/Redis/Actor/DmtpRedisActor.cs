//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Resources;

namespace TouchSocket.Dmtp.Redis
{
    /// <summary>
    /// DmtpRedisActor
    /// </summary>
    public class DmtpRedisActor : IDmtpRedisActor
    {
        /// <summary>
        /// DmtpRedisActor
        /// </summary>
        /// <param name="dmtpActor"></param>
        public DmtpRedisActor(IDmtpActor dmtpActor)
        {
            this.DmtpActor = dmtpActor;
        }

        /// <inheritdoc/>
        public BytesSerializerConverter Converter { get; set; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get; }

        /// <inheritdoc/>
        public ICache<string, byte[]> ICache { get; set; }

        /// <inheritdoc/>
        public int Timeout { get; set; } = 30 * 1000;

        /// <inheritdoc/>
        public async Task<bool> AddAsync<TValue>(string key, TValue value, int duration = 60000)
        {
            var cache = new CacheEntry<string, byte[]>(key)
            {
                Duration = TimeSpan.FromSeconds(duration)
            };
            if (!(value is byte[]))
            {
                cache.Value = this.Converter.Serialize(null, value);
            }
            return await this.AddCacheAsync(cache).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> AddCacheAsync(ICacheEntry<string, byte[]> entity)
        {
            return !await this.ContainsCacheAsync(entity.Key).ConfigureAwait(false) && await this.SetCacheAsync(entity).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ClearCacheAsync()
        {
            var package = new RedisRequestWaitPackage
            {
                packageType = RedisPackageType.Clear
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);
            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    var block = byteBlock;
                    package.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Request, byteBlock.Memory).ConfigureAwait(false);
                }
                switch (await waitData.WaitAsync(this.Timeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            if (waitData.WaitResult.Status == 1)
                            {
                                return;
                            }
                            else
                            {
                                throw new Exception(waitData.WaitResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ContainsCacheAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage
            {
                key = key,
                packageType = RedisPackageType.Contains
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);
            try
            {
                using (var byteBlock = new ByteBlock())
                {
                    var block = byteBlock;
                    package.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Request, byteBlock.Memory).ConfigureAwait(false);
                }
                switch (await waitData.WaitAsync(this.Timeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1
                                ? true
                                : waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message);
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public async Task<TValue> GetAsync<TValue>(string key)
        {
            var cache = await this.GetCacheAsync(key).ConfigureAwait(false);

            if (cache != null)
            {
                if (cache.Value is null)
                {
                    return default;
                }
                if (cache.Value is TValue value1)
                {
                    return value1;
                }
                var value = (TValue)this.Converter.Deserialize(null, cache.Value, typeof(TValue));
                return value;
            }
            return default;
        }

        /// <inheritdoc/>
        public async Task<ICacheEntry<string, byte[]>> GetCacheAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage()
            {
                key = key,
                packageType = RedisPackageType.Get
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    var block = byteBlock;
                    package.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Request, byteBlock.Memory).ConfigureAwait(false);
                }
                switch (await waitData.WaitAsync(this.Timeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var responsePackage = (RedisResponseWaitPackage)waitData.WaitResult;
                            return responsePackage.Status == 1
                                ? new CacheEntry<string, byte[]>(key)
                                {
                                    Value = responsePackage.value
                                }
                                : responsePackage.Status == byte.MaxValue ? new CacheEntry<string, byte[]>(key) : (ICacheEntry<string, byte[]>)default;
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <summary>
        /// 处理收到的消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> InputReceivedData(DmtpMessage message)
        {
            if (message.ProtocolFlags == this.m_redis_Request)
            {
                var waitResult = new RedisResponseWaitPackage();
                try
                {
                    var package = new RedisRequestWaitPackage();
                    var block = message.BodyByteBlock;
                    package.Unpackage(ref block);
                    waitResult.Sign = package.Sign;

                    switch (package.packageType)
                    {
                        case RedisPackageType.Set:
                            {
                                var success = this.ICache.SetCache(new CacheEntry<string, byte[]>(package.key)
                                {
                                    Duration = package.timeSpan.Value,
                                    Value = package.value
                                });
                                waitResult.Status = success ? (byte)1 : byte.MaxValue;
                                break;
                            }
                        case RedisPackageType.Get:
                            {
                                var cache = this.ICache.GetCache(package.key);
                                if (cache != null)
                                {
                                    waitResult.Status = 1;
                                    waitResult.value = cache.Value;
                                }
                                else
                                {
                                    waitResult.Status = byte.MaxValue;
                                }
                            }
                            break;

                        case RedisPackageType.Contains:
                            {
                                waitResult.Status = this.ICache.ContainsCache(package.key) ? (byte)1 : byte.MaxValue;
                            }
                            break;

                        case RedisPackageType.Remove:
                            {
                                waitResult.Status = this.ICache.RemoveCache(package.key) ? (byte)1 : byte.MaxValue;
                            }
                            break;

                        case RedisPackageType.Clear:
                            {
                                this.ICache.ClearCache();
                                waitResult.Status = 1;
                            }
                            break;

                        default:
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    waitResult.Status = 2;
                    waitResult.Message = ex.Message;
                }

                using (var byteBlock = new ByteBlock())
                {
                    var block = byteBlock;
                    waitResult.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Response, byteBlock.Memory).ConfigureAwait(false);
                }
                return true;
            }
            else if (message.ProtocolFlags == this.m_redis_Response)
            {
                var waitResult = new RedisResponseWaitPackage();
                var block = message.BodyByteBlock;
                waitResult.Unpackage(ref block);
                this.DmtpActor.WaitHandlePool.SetRun(waitResult);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveCacheAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"“{nameof(key)}”不能为 null 或空。", nameof(key));
            }

            var package = new RedisRequestWaitPackage
            {
                key = key,
                packageType = RedisPackageType.Remove
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    var block = byteBlock;
                    package.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Request, byteBlock.Memory).ConfigureAwait(false);
                }
                switch (await waitData.WaitAsync(this.Timeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1
                                ? true
                                : waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message);
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(Resources.TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled: return false;
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(Resources.TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetAsync<TValue>(string key, TValue value, int duration = 60000)
        {
            var cache = new CacheEntry<string, byte[]>(key)
            {
                Duration = TimeSpan.FromSeconds(duration),
                Value = value is byte[] bytes ? bytes : this.Converter.Serialize(null, value)
            };
            return await this.SetCacheAsync(cache).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> SetCacheAsync(ICacheEntry<string, byte[]> cache)
        {
            if (string.IsNullOrEmpty(cache.Key))
            {
                throw new ArgumentException($"“{nameof(cache.Key)}”不能为 null 或空。", nameof(cache.Key));
            }

            if (cache is null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            var package = new RedisRequestWaitPackage
            {
                key = cache.Key,
                timeSpan = cache.Duration,
                value = cache.Value,
                packageType = RedisPackageType.Set
            };

            var waitData = this.DmtpActor.WaitHandlePool.GetWaitDataAsync(package);
            try
            {
                using (var byteBlock = new ByteBlock((package.value == null ? 0 : package.value.Length) + 1024))
                {
                    var block = byteBlock;
                    package.Package(ref block);
                    await this.DmtpActor.SendAsync(this.m_redis_Request, byteBlock.Memory).ConfigureAwait(false);
                }
                switch (await waitData.WaitAsync(this.Timeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status == 1 || (waitData.WaitResult.Status == byte.MaxValue ? false : throw new Exception(waitData.WaitResult.Message));
                        }
                    case WaitDataStatus.Overtime: throw new TimeoutException(Resources.TouchSocketDmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled: return false;
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new TimeoutException(TouchSocketDmtpStatus.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.DmtpActor.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <summary>
        /// 设置处理协议标识的起始标识。
        /// </summary>
        /// <param name="start"></param>
        public void SetProtocolFlags(ushort start)
        {
            this.m_redis_Request = start++;
            this.m_redis_Response = start++;
        }

        #region 字段

        private ushort m_redis_Request;
        private ushort m_redis_Response;

        #endregion 字段
    }
}