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

namespace TouchSocket.Core;

/// <summary>
/// 定义了字节块构建器的接口，用于从内存池中构建和管理字节块。
/// </summary>
public interface IBytesBuilder
{
    /// <summary>
    /// 构建数据时，指示内存池的申请长度。
    /// <para>
    /// 建议：该值可以尽可能的设置大一些，这样可以避免内存池扩容。
    /// </para>
    /// </summary>
    int MaxLength { get; }

    /// <summary>
    /// 构建对象到<see cref="ByteBlock"/>
    /// </summary>
    /// <param name="writer">要构建的字节块对象引用。</param>
    void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
#if AllowsRefStruct
,allows ref struct
#endif
        ;
}