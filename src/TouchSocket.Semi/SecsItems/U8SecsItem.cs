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
/// 表示 SECS-II U8（UInt64）格式数据项。
/// </summary>
public class U8SecsItem : SecsItem<ulong>
{
    /// <summary>
    /// 初始化 <see cref="U8SecsItem"/> 的新实例（用于反序列化）。
    /// </summary>
    public U8SecsItem() { }

    /// <summary>
    /// 初始化 <see cref="U8SecsItem"/> 的新实例，并设置初始值。
    /// </summary>
    /// <param name="values">64 位无符号整数值数组。</param>
    public U8SecsItem(ReadOnlyMemory<ulong> values) : base(values) { }

    /// <inheritdoc/>
    public override SecsFormat SecsFormat => SecsFormat.U8;
}
