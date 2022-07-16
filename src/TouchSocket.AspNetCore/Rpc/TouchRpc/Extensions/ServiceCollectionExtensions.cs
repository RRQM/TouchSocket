//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core.Config;
using TouchSocket.Rpc.TouchRpc.AspNetCore;
/* 项目“TouchSocketPro.AspNetCore (netcoreapp3.1)”的未合并的更改
在此之前:
using TouchSocket.Core.Config;
在此之后:
using TouchSocket.Sockets;
*/


namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加WSTouchRpc服务。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IWSTouchRpcService AddWSTouchRpc(this IServiceCollection service, TouchSocketConfig config)
        {
            WSTouchRpcService rpcService = new WSTouchRpcService(config);
            service.AddSingleton<IWSTouchRpcService>(rpcService);
            return rpcService;
        }
    }
}