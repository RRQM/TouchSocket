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

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TouchSocket.Dmtp.PixStream;

namespace WpfApp1.Services;

/// <summary>
/// PixStream 帧显示器，负责将接收到的 <see cref="PixFrame"/> 渲染到指定的 <see cref="Image"/> 控件。
/// </summary>
public sealed class PixStreamDisplay
{
    private readonly Dispatcher m_dispatcher;
    private readonly Image m_imageControl;
    private readonly TextBlock m_infoText;
    private readonly PerformanceMonitor m_perfMonitor = new();
    private WriteableBitmap? m_bitmap;
    private bool m_isProcessing;

    /// <summary>
    /// 初始化 <see cref="PixStreamDisplay"/> 类的新实例。
    /// </summary>
    /// <param name="imageControl">用于显示画面的 <see cref="Image"/> 控件。</param>
    /// <param name="infoText">用于显示性能信息的 <see cref="TextBlock"/> 控件。</param>
    /// <param name="dispatcher">UI 线程的 <see cref="Dispatcher"/>。</param>
    public PixStreamDisplay(Image imageControl, TextBlock infoText, Dispatcher dispatcher)
    {
        this.m_imageControl = imageControl;
        this.m_infoText = infoText;
        this.m_dispatcher = dispatcher;
    }

    internal async Task OnFrameReceivedAsync(PixFrame frame)
    {
        if (this.m_isProcessing)
        {
            return;
        }

        this.m_isProcessing = true;
        try
        {
            var frameWidth = frame.Width;
            var frameHeight = frame.Height;

            this.m_perfMonitor.AddBytes(frame.TransferredBytes);
            this.m_perfMonitor.IncrementFrame();
            var perfUpdated = this.m_perfMonitor.Update();
            var frameType = frame.FrameType;
            var transferredBytes = frame.TransferredBytes;

            await this.m_dispatcher.InvokeAsync(() =>
            {
                if (this.m_bitmap is null
                    || this.m_bitmap.PixelWidth != frameWidth
                    || this.m_bitmap.PixelHeight != frameHeight)
                {
                    this.m_bitmap = new WriteableBitmap(frameWidth, frameHeight, 96, 96, PixelFormats.Bgr24, null);
                    this.m_imageControl.Source = this.m_bitmap;
                }

                this.m_bitmap.Lock();
                try
                {
                    #region PixStream接收帧并写入WPF位图
                    unsafe
                    {
                        var pDst = (byte*)this.m_bitmap.BackBuffer;
                        var stride = this.m_bitmap.BackBufferStride;

                        // 遍历收到的每个像素块，将其BGR24数据写入对应的位图区域
                        foreach (var block in frame.Blocks)
                        {
                            var bgr24 = block.Bgr24Data.Span;
                            var rowBytes = block.Width * 3;
                            for (var by = 0; by < block.Height; by++)
                            {
                                bgr24.Slice(by * rowBytes, rowBytes)
                                     .CopyTo(new Span<byte>(pDst + (block.Y + by) * stride + block.X * 3, rowBytes));
                            }
                        }
                    }
                    #endregion

                    Debug.WriteLine($"Received frame: {frameType}, {frameWidth}x{frameHeight}, {transferredBytes} bytes");
                    this.m_bitmap.AddDirtyRect(new Int32Rect(0, 0, frameWidth, frameHeight));
                }
                finally
                {
                    this.m_bitmap.Unlock();
                }

                if (perfUpdated)
                {
                    this.m_infoText.Text =
                        $"帧类型: {frameType} | 尺寸: {frameWidth}×{frameHeight} | " +
                        $"{this.m_perfMonitor.CurrentFps:F1} fps | 带宽: {this.m_perfMonitor.CurrentBandwidth:F2} MB/s";
                }
            });
        }
        catch (Exception ex)
        {
            await this.m_dispatcher.InvokeAsync(() =>
            {
                this.m_infoText.Text = $"接收错误: {ex.Message}";
            });
        }
        finally
        {
            this.m_isProcessing = false;
        }
    }
}
