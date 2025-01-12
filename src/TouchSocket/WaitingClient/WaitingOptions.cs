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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 等待设置
    /// </summary>
    public class WaitingOptions
    {
        /// <summary>
        /// 筛选函数
        /// </summary>
        public Func<ResponsedData, bool> FilterFunc
        {
            set
            {
                if (value == default)
                {
                    this.FilterFuncAsync = default;
                }
                else
                {
                    Task<bool> FilterFuncValue(ResponsedData data)
                    {
                        var task = Task.FromResult(value.Invoke(data));
                        task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return task;
                    }

                    this.FilterFuncAsync = FilterFuncValue;
                }
            }
        }

        /// <summary>
        /// 异步筛选函数
        /// </summary>
        public Func<ResponsedData, Task<bool>> FilterFuncAsync { get; set; }

        /// <summary>
        /// 远程地址(仅在Udp模式下生效)
        /// </summary>
        public IPHost RemoteIPHost { get; set; }
    }
}