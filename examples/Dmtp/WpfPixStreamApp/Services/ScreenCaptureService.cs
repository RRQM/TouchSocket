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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace WpfApp1.Services;

/// <summary>
/// 屏幕捕获服务，支持指定区域捕获及内置帧率控制的后台采集线程
/// </summary>
public sealed partial class ScreenCaptureService : IDisposable
{
    private const int SRCCOPY = 0x00CC0020;
    private const int DESKTOPHORZRES = 118;
    private const int DESKTOPVERTRES = 117;

    private readonly Image<Bgr24> m_image;
    private readonly int m_screenHeight;
    private readonly int m_screenWidth;
    private readonly int m_offsetX;
    private readonly int m_offsetY;
    private readonly byte[] m_pixels;

    // 持久化 GDI 资源，避免每帧重复创建/释放
    private readonly IntPtr m_hDesk;
    private readonly IntPtr m_hSrce;
    private readonly IntPtr m_hDest;
    private readonly IntPtr m_hBmp;
    private readonly IntPtr m_hOldBmp;
    private BITMAPINFOHEADER m_bmi;

    private bool m_disposed;
    private Thread? m_captureThread;
    private CancellationTokenSource? m_cts;
    private int m_fps = 30;

    /// <summary>
    /// 初始化 <see cref="ScreenCaptureService"/> 类的新实例，捕获整个屏幕
    /// </summary>
    public ScreenCaptureService() : this(0, 0, 0, 0)
    {
    }

    /// <summary>
    /// 初始化 <see cref="ScreenCaptureService"/> 类的新实例，捕获指定区域
    /// </summary>
    /// <param name="offsetX">捕获区域左上角 X 坐标</param>
    /// <param name="offsetY">捕获区域左上角 Y 坐标</param>
    /// <param name="width">捕获区域宽度，0 表示从偏移到屏幕右边</param>
    /// <param name="height">捕获区域高度，0 表示从偏移到屏幕底边</param>
    public ScreenCaptureService(int offsetX, int offsetY, int width, int height)
    {
        var (physW, physH) = GetPhysicalScreenSize();
        this.m_offsetX = offsetX;
        this.m_offsetY = offsetY;
        this.m_screenWidth = width > 0 ? width : physW - offsetX;
        this.m_screenHeight = height > 0 ? height : physH - offsetY;

        this.m_pixels = new byte[this.m_screenWidth * this.m_screenHeight * 3];
        this.m_image = new Image<Bgr24>(this.m_screenWidth, this.m_screenHeight);

        this.m_hDesk = GetDesktopWindow();
        this.m_hSrce = GetDC(this.m_hDesk);
        this.m_hDest = CreateCompatibleDC(this.m_hSrce);
        this.m_hBmp = CreateCompatibleBitmap(this.m_hSrce, this.m_screenWidth, this.m_screenHeight);
        this.m_hOldBmp = SelectObject(this.m_hDest, this.m_hBmp);

        this.m_bmi = new BITMAPINFOHEADER
        {
            biSize = Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth = this.m_screenWidth,
            biHeight = -this.m_screenHeight,
            biPlanes = 1,
            biBitCount = 24,
            biCompression = 0
        };
    }

    /// <summary>
    /// 获取或设置采集帧率
    /// </summary>
    public int Fps
    {
        get => this.m_fps;
        set => this.m_fps = value > 0 ? value : 1;
    }

    /// <summary>
    /// 捕获到新帧时触发，回调参数为内部复用的 <see cref="Image{Bgr24}"/> 对象，不可在回调外持有引用
    /// </summary>
    public Action<Image<Bgr24>>? FrameCaptured { get; set; }

    /// <summary>
    /// 获取捕获区域高度
    /// </summary>
    public int ScreenHeight => this.m_screenHeight;

    /// <summary>
    /// 获取捕获区域宽度
    /// </summary>
    public int ScreenWidth => this.m_screenWidth;

    /// <summary>
    /// 获取主屏幕的物理像素尺寸，不受 DPI 缩放影响
    /// </summary>
    public static (int Width, int Height) GetPhysicalScreenSize()
    {
        var hDesk = GetDesktopWindow();
        var hSrce = GetDC(hDesk);
        try
        {
            return (GetDeviceCaps(hSrce, DESKTOPHORZRES), GetDeviceCaps(hSrce, DESKTOPVERTRES));
        }
        finally
        {
            ReleaseDC(hDesk, hSrce);
        }
    }

    /// <summary>
    /// 启动后台采集线程
    /// </summary>
    public void Start()
    {
        if (this.m_captureThread != null && this.m_captureThread.IsAlive)
        {
            return;
        }

        this.m_cts = new CancellationTokenSource();
        this.m_captureThread = new Thread(this.CaptureLoop)
        {
            IsBackground = true,
            Name = "ScreenCaptureThread"
        };
        this.m_captureThread.Start();
    }

    /// <summary>
    /// 停止后台采集线程
    /// </summary>
    public void Stop()
    {
        this.m_cts?.Cancel();
    }

    private Image<Bgr24> CaptureScreen()
    {
        BitBlt(this.m_hDest, 0, 0, this.m_screenWidth, this.m_screenHeight, this.m_hSrce, this.m_offsetX, this.m_offsetY, SRCCOPY);
        GetDIBits(this.m_hDest, this.m_hBmp, 0, (uint)this.m_screenHeight, this.m_pixels, ref this.m_bmi, 0);

        this.m_image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < this.m_screenHeight; y++)
            {
                var rowSpan = accessor.GetRowSpan(y);
                var srcOffset = y * this.m_screenWidth * 3;
                for (var x = 0; x < this.m_screenWidth; x++)
                {
                    var pixelOffset = srcOffset + x * 3;
                    rowSpan[x] = new Bgr24(this.m_pixels[pixelOffset + 2], this.m_pixels[pixelOffset + 1], this.m_pixels[pixelOffset]);
                }
            }
        });

        return this.m_image;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.m_disposed)
        {
            this.Stop();
            SelectObject(this.m_hDest, this.m_hOldBmp);
            DeleteObject(this.m_hBmp);
            DeleteDC(this.m_hDest);
            ReleaseDC(this.m_hDesk, this.m_hSrce);
            this.m_image.Dispose();
            this.m_disposed = true;
        }
    }

    private void CaptureLoop(object? state)
    {
        if (this.m_cts is null)
        {
            return;
        }
        while (!this.m_cts.IsCancellationRequested)
        {
            var start = Environment.TickCount64;
            var image = this.CaptureScreen();
            this.FrameCaptured?.Invoke(image);
            var elapsed = (int)(Environment.TickCount64 - start);
            var delay = 1000 / this.m_fps - elapsed;
            if (delay > 0)
            {
                Thread.Sleep(delay);
            }
        }
    }

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

    [LibraryImport("gdi32.dll")]
    private static partial IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [LibraryImport("gdi32.dll")]
    private static partial IntPtr CreateCompatibleDC(IntPtr hDC);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(IntPtr hDC);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr hObject);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetDesktopWindow();

    [LibraryImport("gdi32.dll")]
    private static partial int GetDeviceCaps(IntPtr hDC, int nIndex);

    [LibraryImport("gdi32.dll")]
    private static partial int GetDIBits(IntPtr hdc, IntPtr hbmp, uint start, uint cLines, byte[] lpvBits, ref BITMAPINFOHEADER lpbmi, uint usage);

    [LibraryImport("user32.dll")]
    private static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [LibraryImport("gdi32.dll")]
    private static partial IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }
}