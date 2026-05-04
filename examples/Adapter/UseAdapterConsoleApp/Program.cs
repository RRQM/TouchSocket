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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UseAdapterConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Demo_ConfigAdapter();
        await Demo_UdpConfig();
        Demo_AdapterOption();
    }

    private static async Task Demo_ConfigAdapter()
    {
        #region 适配器在Config中使用
        var client = new TcpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .SetSingleStreamDataHandlingAdapter(() => new FixedHeaderPackageAdapter())
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        #endregion
    }

    private static async Task Demo_UdpConfig()
    {
        #region 适配器Udp中使用
        var udpSession = new UdpSession();
        await udpSession.SetupAsync(new TouchSocketConfig()
            .SetBindIPHost(new IPHost("127.0.0.1:7790"))
            .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            }));
        await udpSession.StartAsync();
        #endregion
    }

    private static void Demo_AdapterOption()
    {
        #region 适配器通过AdapterOption配置参数
        var config = new TouchSocketConfig()
            .SetAdapterOption(option =>
            {
                option.MaxPackageSize = 1024 * 1024;
                option.CacheTimeoutEnable = true;
                option.CacheTimeout = TimeSpan.FromSeconds(5);
            })
            .SetSingleStreamDataHandlingAdapter(() => new FixedHeaderPackageAdapter());
        #endregion
    }
}

#region 适配器限制使用
/// <summary>
/// 自定义 TcpSessionClient，在 OnTcpConnecting 内部固定设置适配器，
/// 外部无法通过 protected SetAdapter 修改，从而实现适配器锁定。
/// </summary>
public class MyTcpSessionClient : TcpSessionClient
{
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        // 仅在内部设置适配器，外部无法访问 protected SetAdapter
        this.SetAdapter(new FixedHeaderPackageAdapter());
        await base.OnTcpConnecting(e);
    }
}
#endregion
