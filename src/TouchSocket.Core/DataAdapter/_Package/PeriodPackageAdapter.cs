//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 周期包适配
    /// </summary>
    public class PeriodPackageAdapter : SingleStreamDataHandlingAdapter
    {
        private readonly ConcurrentQueue<byte[]> m_bytes = new ConcurrentQueue<byte[]>();
        private long m_count;

        /// <inheritdoc/>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            this.m_bytes.Enqueue(byteBlock.ToArray());
            Interlocked.Increment(ref this.m_count);
            Task.Run(this.DelayGo);
        }

        private async Task DelayGo()
        {
            await Task.Delay(this.CacheTimeout);
            if (Interlocked.Decrement(ref this.m_count) == 0)
            {
                using (var byteBlock = new ByteBlock())
                {
                    while (this.m_bytes.TryDequeue(out var bytes))
                    {
                        byteBlock.Write(bytes);
                    }

                    byteBlock.SeekToStart();

                    try
                    {
                        this.GoReceived(byteBlock, default);
                    }
                    catch (Exception ex)
                    {
                        this.OnError(ex, ex.Message, true, true);
                    }
                }
            }
        }
    }
}