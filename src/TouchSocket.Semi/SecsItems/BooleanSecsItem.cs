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

namespace TouchSocket.Semi;

/// <summary>
/// 表示 SECS-II Boolean 格式数据项。
/// </summary>
public class BooleanSecsItem : SecsItem<byte>
{
    /// <summary>
    /// 初始化 <see cref="BooleanSecsItem"/> 的新实例（用于反序列化）。
    /// </summary>
    public BooleanSecsItem() { }

    /// <summary>
    /// 初始化 <see cref="BooleanSecsItem"/> 的新实例，并设置初始值。
    /// </summary>
    /// <param name="values">布尔值数组（以 <see langword="byte"/> 表示）。</param>
    public BooleanSecsItem(ReadOnlyMemory<byte> values) : base(values) { }

    /// <inheritdoc/>
    public override SecsFormat SecsFormat => SecsFormat.Boolean;
}
