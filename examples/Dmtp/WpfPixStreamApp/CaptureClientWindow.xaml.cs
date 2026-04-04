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

/// <summary>
/// 场景二：采集端窗口。以 TcpDmtpClient 连接服务器，持有 Provider，供服务器主动拉取帧。
/// </summary>
public partial class CaptureClientWindow : System.Windows.Window
{
    private readonly int m_serverPort;
    private TcpDmtpClient? m_client;
    private ScreenCaptureService? m_captureService;
    private readonly PixStreamProvider m_provider = new();
    private int m_captureFrameCount;
    private DateTime m_lastCaptureFpsTime = DateTime.Now;

    public CaptureClientWindow(int serverPort)
    {
        this.InitializeComponent();
        this.m_serverPort = serverPort;
    }

    private async void Window_Closed(object sender, EventArgs e)
    {
        m_captureService?.Stop();
        m_captureService?.Dispose();
        if (m_client != null)
        {
            await m_client.CloseAsync("closed");
            m_client.Dispose();
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            #region PixStream客户端采集端配置
            // 采集端：以普通 TcpDmtpClient 连接服务器，持有 Provider；服务器在连接后主动向其发起会话拉取帧
            m_client = new TcpDmtpClient();
            await m_client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost($"127.0.0.1:{m_serverPort}")
                .SetDmtpOption(o => o.VerifyToken = "pixstream")
                .ConfigurePlugins(a =>
                {
                    // 客户端持有 Provider，服务器将主动向其发起像素流会话
                    a.UseDmtpPixStream(o => o.Provider = m_provider);

                    // 允许服务器（会话请求方）创建会话
                    a.Add(typeof(IDmtpPixStreamCreatingPlugin), async (TcpDmtpClient client, DmtpPixStreamCreatingEventArgs ev) =>
                    {
                        ev.IsPermitOperation = true;
                        await ev.InvokeNext();
                    });
                }));
            await m_client.ConnectAsync();
            #endregion

            txtStatus.Text = $"已连接，ID={m_client.DmtpActor.Id}";
            StartScreenCapture();
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"连接失败: {ex.Message}";
        }
    }

    private void StartScreenCapture()
    {
        var (w, h) = ScreenCaptureService.GetPhysicalScreenSize();
        m_captureService = new ScreenCaptureService();
        m_captureService.Fps = 100;
        m_captureService.FrameCaptured = OnFrameCaptured;
        m_captureService.Start();
        txtCaptureRate.Text = $"采集 {w}×{h}，启动中…";
    }

    private void OnFrameCaptured(SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgr24> image)
    {
        try
        {
            m_provider.UpdateFrame(image);

            m_captureFrameCount++;
            var elapsed = (DateTime.Now - m_lastCaptureFpsTime).TotalSeconds;
            if (elapsed >= 1.0)
            {
                var fps = m_captureFrameCount / elapsed;
                this.Dispatcher.Invoke(() => txtCaptureRate.Text = $"{fps:F1} fps");
                m_captureFrameCount = 0;
                m_lastCaptureFpsTime = DateTime.Now;
            }
        }
        catch
        {
            m_captureService?.Stop();
        }
    }
}
