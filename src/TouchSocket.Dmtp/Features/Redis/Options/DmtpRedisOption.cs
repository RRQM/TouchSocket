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

namespace TouchSocket.Dmtp.Redis;

/// <summary>
/// Redis配置选项
/// </summary>
public class DmtpRedisOption : DmtpFeatureOption
{
    public DmtpRedisOption()
    {
        this.StartProtocol = 25;
    }
    /// <summary>
    /// 元素序列化和反序列化转换器
    /// </summary>
    public BytesSerializerConverter Converter { get; set; } = new BytesSerializerConverter();

    /// <summary>
    /// 实际储存缓存
    /// </summary>
    public ICache<string, ReadOnlyMemory<byte>> Cache { get; set; } = new MemoryCache<string, ReadOnlyMemory<byte>>();


    /// <summary>
    /// 配置元素的序列化和反序列化转换器
    /// </summary>
    /// <param name="action">配置操作</param>
    public void ConfigureConverter(Action<BytesSerializerConverter> action)
    {
        action.Invoke(this.Converter);
    }
}