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

public partial class MainWindow : System.Windows.Window
{
    private const int ServerPort = 9789;

    private TcpDmtpService? m_service;
    private TcpDmtpClient? m_captureClient;        // 场景三：采集端 TcpDmtpClient
    private RequestWindow? m_viewerWindow;
    private CaptureClientWindow? m_captureWindow;  // 场景二：采集端窗口
    private PixStreamDisplay? m_serverDisplay;     // 场景二：服务器端显示器
    private ScreenCaptureService? m_captureService;
    private readonly PixStreamProvider m_provider = new();
    private int m_captureFrameCount;
    private DateTime m_lastCaptureFpsTime = DateTime.Now;

    public MainWindow()
    {
        this.InitializeComponent();
    }

    protected override async void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        m_captureService?.Stop();
        m_captureService?.Dispose();
        m_viewerWindow?.Close();
        m_captureWindow?.Close();
        if (m_captureClient != null)
        {
            await m_captureClient.CloseAsync("closed");
            m_captureClient.Dispose();
        }
        if (m_service != null)
        {
            await m_service.StopAsync();
            m_service.Dispose();
        }
    }

    // =====================================================================
    // 启动按钮：根据所选场景启动不同拓扑
    // =====================================================================

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        btnStart.IsEnabled = false;
        rbScenario1.IsEnabled = false;
        rbScenario2.IsEnabled = false;
        rbScenario3.IsEnabled = false;
        try { Enterprise.ForTest(); } catch { }

        try
        {
            if (rbScenario1.IsChecked == true)
                await StartScenario1Async();
            else if (rbScenario2.IsChecked == true)
                await StartScenario2Async();
            else
                await StartScenario3Async();
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"启动失败: {ex.Message}";
            btnStart.IsEnabled = true;
            rbScenario1.IsEnabled = true;
            rbScenario2.IsEnabled = true;
            rbScenario3.IsEnabled = true;
        }
    }

    // =====================================================================
    // 场景一：服务器采集，客户端直连显示
    // =====================================================================

    private async Task StartScenario1Async()
    {
        #region PixStream场景一服务器配置
        // 服务器持有 Provider，作为图像采集端；客户端直连拉取，无需路由
        m_service = new TcpDmtpService();
        await m_service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(ServerPort)
            .SetDmtpOption(o => o.VerifyToken = "pixstream")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpPixStream(o => o.Provider = m_provider);

                #region PixStream审批会话创建插件
                // 审批来自客户端的会话创建请求
                a.Add(typeof(IDmtpPixStreamCreatingPlugin), async (TcpDmtpSessionClient client, DmtpPixStreamCreatingEventArgs ev) =>
                {
                    ev.IsPermitOperation = true;
                    await ev.InvokeNext();
                });
                #endregion
            }));
        await m_service.StartAsync();
        #endregion

        txtStatus.Text = $"场景一已启动（端口 {ServerPort}），服务器采集中";
        StartScreenCapture();
        btnOpenView.Visibility = Visibility.Visible;
        btnOpenView.IsEnabled = true;
    }

    // =====================================================================
    // 场景二：客户端采集，服务器主动拉取
    // =====================================================================

    private async Task StartScenario2Async()
    {
        #region PixStream场景二服务器配置
        // 服务器不设 Provider，作为接收显示端；客户端连接后服务器主动发起会话拉取
        m_service = new TcpDmtpService();
        await m_service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(ServerPort)
            .SetDmtpOption(o => o.VerifyToken = "pixstream")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpPixStream();

                // 客户端连接后，服务器主动向其发起像素流会话
                a.Add(typeof(IDmtpConnectedPlugin), async (TcpDmtpSessionClient sessionClient, DmtpVerifyEventArgs e) =>
                {
                    await e.InvokeNext();
                    _ = Task.Run(() => PullFromClientAsync(sessionClient));
                });
            }));
        await m_service.StartAsync();
        #endregion

        txtStatus.Text = $"场景二已启动（端口 {ServerPort}），等待采集端连接…";

        // 展开服务器显示区域，扩大窗口
        m_serverDisplay = new PixStreamDisplay(this.imgDisplay2, this.txtFrameInfo2, this.Dispatcher);
        gridServerDisplay.Visibility = Visibility.Visible;
        this.Height = 750;
        this.Width = 900;

        // 打开采集端窗口，自动连接到服务器并开始采集
        m_captureWindow = new CaptureClientWindow(ServerPort);
        m_captureWindow.Show();
    }

    private async Task PullFromClientAsync(TcpDmtpSessionClient sessionClient)
    {
        try
        {
            await Task.Delay(300);

            #region PixStream服务器向客户端发起会话
            // 服务器通过 sessionClient 向采集端客户端发起像素流会话
            var session = await sessionClient.GetDmtpPixStreamActor().CreatePixStreamSessionAsync(new PixStreamRequest
            {
                Quality = 80,
                EnableDeltaTransfer = true,
            });
            session.FrameRate = 30;
            session.Bind(m_serverDisplay!);
            session.Start();
            #endregion

            this.Dispatcher.Invoke(() => txtStatus.Text = $"正在接收来自 {sessionClient.Id} 的像素流");
        }
        catch (Exception ex)
        {
            this.Dispatcher.Invoke(() => txtFrameInfo2.Text = $"拉取失败: {ex.Message}");
        }
    }

    // =====================================================================
    // 场景三：两客户端间路由传输（服务器仅路由）
    // =====================================================================

    private async Task StartScenario3Async()
    {
        #region PixStream路由服务端配置
        // 服务器作为纯路由节点：不持有 Provider，仅转发客户端间的像素流报文
        m_service = new TcpDmtpService();
        await m_service.SetupAsync(new TouchSocketConfig()
            .SetListenIPHosts(ServerPort)
            .SetDmtpOption(o => o.VerifyToken = "pixstream")
            .ConfigureContainer(a =>
            {
                a.AddDmtpRouteService(id =>
                {
                    m_service.TryGetClient<TcpDmtpSessionClient>(id, out var client);
                    return client?.DmtpActor;
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpPixStream();

                a.Add(typeof(IDmtpPixStreamCreatingPlugin), async (TcpDmtpSessionClient client, DmtpPixStreamCreatingEventArgs ev) =>
                {
                    ev.IsPermitOperation = true;
                    await ev.InvokeNext();
                });

                a.Add(typeof(IDmtpRoutingPlugin), async (TcpDmtpSessionClient client, PackageRouterEventArgs ev) =>
                {
                    ev.IsPermitOperation = true;
                    await ev.InvokeNext();
                });
            }));
        await m_service.StartAsync();
        #endregion

        #region PixStream采集端客户端配置
        // 采集端：以普通 TcpDmtpClient 连接路由服务器，携带 Provider
        m_captureClient = new TcpDmtpClient();
        await m_captureClient.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost($"127.0.0.1:{ServerPort}")
            .SetDmtpOption(o => o.VerifyToken = "pixstream")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpPixStream(o => o.Provider = m_provider);

                // 采集端需要同意接收端（通过路由）对其发起的会话创建请求
                a.Add(typeof(IDmtpPixStreamCreatingPlugin), async (TcpDmtpClient client, DmtpPixStreamCreatingEventArgs ev) =>
                {
                    ev.IsPermitOperation = true;
                    await ev.InvokeNext();
                });
            }));
        await m_captureClient.ConnectAsync();
        #endregion

        var captureId = m_captureClient.DmtpActor.Id;
        txtCaptureClientId.Text = captureId;
        panelCaptureId.Visibility = Visibility.Visible;
        txtStatus.Text = $"场景三已启动（端口 {ServerPort}），采集端 ID: {captureId}";

        StartScreenCapture();
        btnOpenView.Visibility = Visibility.Visible;
        btnOpenView.IsEnabled = true;
    }

    // =====================================================================
    // 屏幕采集（场景一和场景三使用）
    // =====================================================================

    private void StartScreenCapture()
    {
        var (w, h) = ScreenCaptureService.GetPhysicalScreenSize();
        m_captureService = new ScreenCaptureService();
        m_captureService.Fps = 100;
        m_captureService.FrameCaptured = this.OnFrameCaptured;
        m_captureService.Start();
        txtCaptureStatus.Text = $"采集 {w}×{h}，启动中…";
    }

    private void OnFrameCaptured(SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgr24> image)
    {
        try
        {
            #region PixStream采集端推送帧
            // 将新捕获的图像帧推送给 PixStreamProvider，供接收端拉取
            m_provider.UpdateFrame(image);
            #endregion

            m_captureFrameCount++;
            var now = DateTime.Now;
            var elapsed = (now - m_lastCaptureFpsTime).TotalSeconds;
            if (elapsed >= 1.0)
            {
                var fpsText = $"{m_captureFrameCount / elapsed:F1} fps";
                this.Dispatcher.Invoke(() => txtCaptureStatus.Text = fpsText);
                m_captureFrameCount = 0;
                m_lastCaptureFpsTime = now;
            }
        }
        catch (Exception ex)
        {
            this.Dispatcher.Invoke(() => txtCaptureStatus.Text = $"采集错误: {ex.Message}");
            m_captureService?.Stop();
        }
    }

    // =====================================================================
    // 打开显示窗口（场景一和场景三）
    // =====================================================================

    private void BtnOpenView_Click(object sender, RoutedEventArgs e)
    {
        if (m_viewerWindow is { IsLoaded: true })
        {
            m_viewerWindow.Activate();
            return;
        }

        // 场景一：直连（captureClientId=null），场景三：路由（带 ID）
        var captureId = rbScenario3.IsChecked == true ? txtCaptureClientId.Text : null;
        m_viewerWindow = new RequestWindow(ServerPort, captureId);
        m_viewerWindow.Show();
    }
}
