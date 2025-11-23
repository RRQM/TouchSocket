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

using System.Threading.RateLimiting;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.RateLimiting;
using TouchSocket.Sockets;

namespace RpcRateLimitingConsoleApp;

internal class CustomRateLimiterExample
{
    public static void ConfigureCustomRateLimiter()
    {
        var service = new TcpDmtpService();

        #region Rpc限流自定义分区键添加策略
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();

                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<MyRpcServer>();//注册服务
                   });

                   a.AddRateLimiter(p =>
                   {
                       p.AddPolicy("FixedWindow", new RpcFixedWindowLimiter(new System.Threading.RateLimiting.FixedWindowRateLimiterOptions()
                       {
                           PermitLimit = 10,
                           Window = TimeSpan.FromSeconds(10)
                       }));
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Rpc";//连接验证口令。
               });
        #endregion

        service.SetupAsync(config);
        service.StartAsync();
    }
}
