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
/// 提供扩展方法以方便地构建字节块。
/// </summary>
public static class ByteBlockBuilderExtension
{
    /// <summary>
    /// 扩展方法，用于构建给定的字节块。
    /// 该方法通过引用传递字节块，以利用ref参数提高性能，避免不必要的复制。
    /// </summary>
    /// <param name="builder">实现IByteBlockBuilder接口的构建器对象。</param>
    /// <param name="byteBlock">要构建的字节块对象。</param>
    public static void Build(this IByteBlockBuilder builder, ByteBlock byteBlock)
    {
        builder.Build(ref byteBlock);
    }
}