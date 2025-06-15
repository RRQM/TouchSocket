// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

#if AsyncLocal
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Rpc;

[DebuggerDisplay("CallContext = {CallContext}")]
internal class RpcCallContextAccessor : IRpcCallContextAccessor
{
    private static readonly AsyncLocal<RpcCallContextHolder> s_rpcCallContextCurrent = new AsyncLocal<RpcCallContextHolder>();

    /// <inheritdoc/>
    public ICallContext CallContext
    {
        get
        {
            return s_rpcCallContextCurrent.Value?.Context;
        }
        set
        {
            var holder = s_rpcCallContextCurrent.Value;
            if (holder != null)
            {
                holder.Context = null;
            }

            if (value != null)
            {
                s_rpcCallContextCurrent.Value = new RpcCallContextHolder { Context = value };
            }
        }
    }

    private sealed class RpcCallContextHolder
    {
        public ICallContext Context;
    }
}

#endif