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

using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp.Redis
{
    /// <summary>
    /// RedisFeature
    /// </summary>
    public class RedisFeature : PluginBase, IDmtpHandshakingPlugin, IDmtpReceivedPlugin, IDmtpFeature
    {
        /// <summary>
        /// RedisFeature
        /// </summary>
        public RedisFeature()
        {
            this.SetProtocolFlags(25);

            this.Converter = new BytesSerializerConverter();
            this.Converter.Add(new JsonBytesToClassSerializerFormatter<object>());
        }

        /// <summary>
        /// 定义元素的序列化和反序列化。
        /// <para>注意：Byte[]类型不用考虑。内部单独会做处理。</para>
        /// </summary>
        public BytesSerializerConverter Converter { get; private set; }

        /// <summary>
        /// 实际储存缓存。
        /// </summary>
        public ICache<string, byte[]> ICache { get; set; } = new MemoryCache<string, byte[]>();

        /// <inheritdoc/>
        public ushort ReserveProtocolSize => 5;

        /// <inheritdoc/>
        public ushort StartProtocol { get; set; }

        /// <inheritdoc/>
        public Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var dmtpRedisActor = new DmtpRedisActor(client.DmtpActor)
            {
                ICache = this.ICache,
                Converter = this.Converter
            };

            dmtpRedisActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpRedisActor(dmtpRedisActor);

            return e.InvokeNext();
        }

        /// <inheritdoc/>
        public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetDmtpRedisActor() is DmtpRedisActor redisClient)
            {
                if (await redisClient.InputReceivedData(e.DmtpMessage).ConfigureAwait(false))
                {
                    e.Handled = true;
                    return;
                }
            }
            await e.InvokeNext().ConfigureAwait(false);
        }

        /// <summary>
        /// 设置实际储存缓存。默认使用<see cref="MemoryCache{TKey, TValue}"/>
        /// </summary>
        /// <param name="cache"></param>
        public RedisFeature SetCache(ICache<string, byte[]> cache)
        {
            this.ICache = cache;
            return this;
        }

        /// <summary>
        /// 定义元素的序列化和反序列化。
        /// <para>注意：Byte[]类型不用考虑。内部单独会做处理。</para>
        /// </summary>
        /// <param name="action"></param>
        public RedisFeature ConfigureConverter(Action<BytesSerializerConverter> action)
        {
            action.Invoke(this.Converter);
            return this;
        }

        /// <summary>
        /// 设置<see cref="RedisFeature"/>的起始协议。
        /// <para>
        /// 默认起始为：25，保留5个协议长度。
        /// </para>
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public RedisFeature SetProtocolFlags(ushort start)
        {
            this.StartProtocol = start;
            return this;
        }
    }
}