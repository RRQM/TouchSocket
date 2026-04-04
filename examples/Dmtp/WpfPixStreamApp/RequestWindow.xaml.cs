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

using System.Windows;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.PixStream;
using TouchSocket.Sockets;
using WpfApp1.Services;

namespace WpfApp1;

public partial class RequestWindow : System.Windows.Window
{
    private readonly int m_serverPort;
    private readonly string? m_captureClientId;  // null = 直连（场景一），非空 = 路由（场景三）
    private TcpDmtpClient? m_client;
    private PixStreamDisplay? m_displayBitmap;
    private IPixStreamSession? m_session;

    public RequestWindow(int serverPort, string? captureClientId)
    {
        this.InitializeComponent();
        this.m_serverPort = serverPort;
        this.m_captureClientId = captureClientId;
        this.Title = captureClientId == null
            ? "接收方 - 直连显示（场景一）"
            : "接收方 - 路由显示（场景三）";
    }

    private async void Window_Closed(object sender, EventArgs e)
    {
        if (this.m_session != null)
        {
            await this.m_session.CloseAsync("");
        }
        if (this.m_client != null)
        {
            await this.m_client.CloseAsync("closed");
            this.m_client.Dispose();
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            #region PixStream接收端客户端配置
            this.m_client = new TcpDmtpClient();
            await this.m_client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost($"127.0.0.1:{this.m_serverPort}")
                .SetDmtpOption(options =>
                {
                    options.VerifyToken = "pixstream";
                })
                .ConfigurePlugins(a =>
                {
                    // 客户端只需注册功能，无需配置Provider
                    a.UseDmtpPixStream();
                }));
            await this.m_client.ConnectAsync();
            #endregion
            this.txtStatus.Text = $"已连接为请求方 (ID: {this.m_client.DmtpActor.Id})";

            this.m_displayBitmap = new PixStreamDisplay(this.imgDisplay, this.txtFrameInfo, this.Dispatcher);

            if (this.m_captureClientId == null)
            {
                #region PixStream客户端直连创建会话
                // 场景一：直连服务器，服务器本身就是采集端，无需指定 targetId
                this.m_session = await this.m_client!.GetDmtpPixStreamActor().CreatePixStreamSessionAsync(
                    new PixStreamRequest
                    {
                        Quality = 90,
                        EnableDeltaTransfer = true,
                    });
                #endregion
            }
            else
            {
                #region PixStream接收端路由创建会话
                // 场景三：通过路由服务器寻址到采集端，传入采集端的 DMTP ID
                this.m_session = await this.m_client!.GetDmtpPixStreamActor().CreatePixStreamSessionAsync(
                    this.m_captureClientId,     // 采集端的DMTP ID
                    new PixStreamRequest
                    {
                        Quality = 90,
                        EnableDeltaTransfer = true,
                    });
                #endregion
            }

            // 设置帧率（每秒请求次数）
            this.m_session.FrameRate = 1000;
            // 绑定帧接收回调到显示器
            this.m_session.Bind(this.m_displayBitmap);
            // 启动定时拉取
            this.m_session.Start();
        }
        catch (Exception ex)
        {
            this.txtStatus.Text = $"连接失败: {ex.Message}";
        }
    }
}