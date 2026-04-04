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

namespace WpfApp1.Services;

/// <summary>
/// 性能监控器
/// </summary>
public sealed class PerformanceMonitor
{
    private long m_bytesTransferred;
    private int m_frameCount;
    private DateTime m_lastUpdate;

    /// <summary>
    /// 初始化 <see cref="PerformanceMonitor"/> 类的新实例
    /// </summary>
    public PerformanceMonitor()
    {
        this.Reset();
    }

    /// <summary>
    /// 获取当前带宽（MB/s）
    /// </summary>
    public double CurrentBandwidth { get; private set; }

    /// <summary>
    /// 获取当前 FPS
    /// </summary>
    public double CurrentFps { get; private set; }

    /// <summary>
    /// 增加传输字节数
    /// </summary>
    /// <param name="bytes">字节数</param>
    public void AddBytes(long bytes)
    {
        this.m_bytesTransferred += bytes;
    }

    /// <summary>
    /// 增加帧计数
    /// </summary>
    public void IncrementFrame()
    {
        this.m_frameCount++;
    }

    /// <summary>
    /// 重置统计数据
    /// </summary>
    public void Reset()
    {
        this.m_frameCount = 0;
        this.m_bytesTransferred = 0;
        this.CurrentFps = 0;
        this.CurrentBandwidth = 0;
        this.m_lastUpdate = DateTime.Now;
    }

    /// <summary>
    /// 更新性能统计
    /// </summary>
    /// <returns>如果已更新则返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public bool Update()
    {
        var now = DateTime.Now;
        var elapsed = now - this.m_lastUpdate;

        if (elapsed.TotalSeconds >= 1.0)
        {
            this.CurrentFps = this.m_frameCount / elapsed.TotalSeconds;
            this.CurrentBandwidth = this.m_bytesTransferred / elapsed.TotalSeconds / (1024 * 1024);
            this.m_frameCount = 0;
            this.m_bytesTransferred = 0;
            this.m_lastUpdate = now;
            return true;
        }

        return false;
    }
}