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

namespace TouchSocket.Http;

/// <summary>
/// HTTP/2 连接设置，封装本端或远端 SETTINGS 帧内容，见 RFC 7540 §6.5
/// </summary>
internal sealed class Http2PeerSettings
{
    /// <summary>默认初始窗口大小（65535 字节）</summary>
    public const uint DefaultInitialWindowSize = 65535;

    /// <summary>默认最大帧大小（16384 字节）</summary>
    public const uint DefaultMaxFrameSize = 16384;

    /// <summary>最大允许的窗口大小（2^31 - 1）</summary>
    public const uint MaxWindowSize = 0x7FFFFFFF;

    /// <summary>最小允许的帧大小（16384 字节）</summary>
    public const uint MinMaxFrameSize = 16384;

    /// <summary>最大允许的帧大小（16777215 字节）</summary>
    public const uint MaxMaxFrameSize = 0xFFFFFF;

    /// <summary>头部压缩表大小，默认 4096</summary>
    public uint HeaderTableSize { get; set; } = 4096;

    /// <summary>是否允许服务器推送，默认 true</summary>
    public bool EnablePush { get; set; } = true;

    /// <summary>最大并发流数，默认无限制（uint.MaxValue）</summary>
    public uint MaxConcurrentStreams { get; set; } = uint.MaxValue;

    /// <summary>初始流窗口大小，默认 65535</summary>
    public uint InitialWindowSize { get; set; } = DefaultInitialWindowSize;

    /// <summary>最大帧负载大小，默认 16384</summary>
    public uint MaxFrameSize { get; set; } = DefaultMaxFrameSize;

    /// <summary>最大头部列表大小，默认无限制（uint.MaxValue）</summary>
    public uint MaxHeaderListSize { get; set; } = uint.MaxValue;

    /// <summary>
    /// 应用单条 SETTINGS 参数
    /// </summary>
    /// <param name="parameter">参数标识</param>
    /// <param name="value">参数值</param>
    /// <exception cref="Http2ConnectionException">参数值非法时抛出</exception>
    public void ApplySetting(Http2SettingsParameter parameter, uint value)
    {
        switch (parameter)
        {
            case Http2SettingsParameter.HeaderTableSize:
                this.HeaderTableSize = value;
                break;
            case Http2SettingsParameter.EnablePush:
                if (value > 1)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, "SETTINGS_ENABLE_PUSH 值必须为 0 或 1");
                }
                this.EnablePush = value == 1;
                break;
            case Http2SettingsParameter.MaxConcurrentStreams:
                this.MaxConcurrentStreams = value;
                break;
            case Http2SettingsParameter.InitialWindowSize:
                if (value > MaxWindowSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.FlowControlError, $"SETTINGS_INITIAL_WINDOW_SIZE 值 {value} 超过最大值 {MaxWindowSize}");
                }
                this.InitialWindowSize = value;
                break;
            case Http2SettingsParameter.MaxFrameSize:
                if (value < MinMaxFrameSize || value > MaxMaxFrameSize)
                {
                    throw new Http2ConnectionException(Http2ErrorCode.ProtocolError, $"SETTINGS_MAX_FRAME_SIZE 值 {value} 超出合法范围 [{MinMaxFrameSize}, {MaxMaxFrameSize}]");
                }
                this.MaxFrameSize = value;
                break;
            case Http2SettingsParameter.MaxHeaderListSize:
                this.MaxHeaderListSize = value;
                break;
                // 忽略未知参数（RFC 7540 §6.5）
        }
    }
}
