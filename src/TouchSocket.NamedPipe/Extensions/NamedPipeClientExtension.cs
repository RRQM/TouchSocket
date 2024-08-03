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

using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// NamedPipeClientExtension
    /// </summary>
    public static class NamedPipeClientExtension
    {
        #region 连接

        /// <summary>
        /// 异步连接到指定的命名管道
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="pipeName">管道名称</param>
        /// <param name="millisecondsTimeout">超时设定</param>
        /// <returns></returns>
        public static async Task<TClient> ConnectAsync<TClient>(this TClient client, string pipeName, int millisecondsTimeout = 5000) where TClient : INamedPipeClient
        {
            TouchSocketConfig config;
            if (client.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetPipeName(pipeName);
                await client.SetupAsync(config).ConfigureAwait(false);
            }
            else
            {
                config = client.Config;
                config.SetPipeName(pipeName);
            }
            await client.ConnectAsync(millisecondsTimeout, CancellationToken.None).ConfigureAwait(false);
            return client;
        }

        #endregion 连接
    }
}