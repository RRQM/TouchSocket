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

using System;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocket数据帧扩展类
/// </summary>
public static class WebSocketDataFrameExtension
{
    public static readonly ReadOnlyMemory<byte> DefaultMaskingKey = "RRQM".ToUtf8Bytes();

    /// <summary>
    /// 构建请求数据（含Make）
    /// </summary>
    /// <param name="dataFrame">数据帧对象，用于封装请求数据</param>
    /// <param name="byteBlock">字节块对象，用于存储构建的请求数据</param>
    /// <typeparam name="TByteBlock">泛型参数，指定字节块的类型，必须实现IByteBlock接口</typeparam>
    /// <remarks>
    /// 此方法通过设置数据帧的Mask属性为<see langword="true"/>，并确保数据帧具有MaskingKey，
    /// 然后调用dataFrame的Build方法来构建请求数据，并将结果存储在byteBlock中。
    /// 如果MaskingKey未设置，则使用"RRQM"作为默认值。
    /// </remarks>
    public static void BuildRequest<TByteBlock>(this WSDataFrame dataFrame, ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        // 设置数据帧的Mask属性为<see langword="true"/>，表示数据在传输前会被掩码处理
        dataFrame.Mask = true;
        // 检查MaskingKey是否已设置，如果没有设置，则使用默认值"RRQM"
        if (dataFrame.MaskingKey.IsEmpty)
        {
            dataFrame.SetMask(DefaultMaskingKey);
        }
        // 调用Build方法构建请求数据，并将结果存储在byteBlock中
        dataFrame.Build(ref byteBlock);
    }

    /// <summary>
    /// 构建请求数据（含Make）
    /// </summary>
    /// <param name="dataFrame">要构建的数据帧</param>
    /// <returns>构建完成的字节数组</returns>
    public static byte[] BuildRequestToBytes(this WSDataFrame dataFrame)
    {
        // 设置数据帧的Mask属性为<see langword="true"/>，确保数据帧被掩码处理
        dataFrame.Mask = true;
        // 如果数据帧的MaskingKey属性为空，则设置MaskingKey
        if (dataFrame.MaskingKey.IsEmpty)
        {
            dataFrame.SetMask(DefaultMaskingKey);
        }
        // 创建一个ValueByteBlock对象，用于存储构建过程中的字节数据
        var byteBlock = new ValueByteBlock(dataFrame.MaxLength);
        try
        {
            // 调用数据帧的Build方法，将数据帧构建到byteBlock中
            dataFrame.Build(ref byteBlock);
            // 将byteBlock转换为字节数组
            var data = byteBlock.ToArray();
            // 返回构建完成的字节数组
            return data;
        }
        finally
        {
            // 释放byteBlock占用的资源
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 构建响应数据（无Make）
    /// </summary>
    /// <param name="dataFrame">待构建的WS数据帧</param>
    /// <param name="byteBlock">字节块，用于存储构建后的数据</param>
    /// <remarks>
    /// 该方法直接调用WS数据帧的Build方法来构建数据，
    /// 并将构建结果存储在字节块中，而不是创建一个新的对象。
    /// 这样可以提高性能，减少内存分配。
    /// </remarks>
    public static void BuildResponse<TByteBlock>(this WSDataFrame dataFrame, ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        dataFrame.Build(ref byteBlock);
    }

    /// <summary>
    /// 构建响应数据（无Make）
    /// </summary>
    /// <param name="dataFrame">要转换为字节数组的数据帧</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] BuildResponseToBytes(this WSDataFrame dataFrame)
    {
        // 创建一个值字块，大小为数据帧的最大长度，用于存储即将构建的字节数据
        var byteBlock = new ValueByteBlock(dataFrame.MaxLength);
        try
        {
            // 调用数据帧的Build方法，将数据帧的内容构建到byteBlock中
            dataFrame.Build(ref byteBlock);
            // 将值字块转换为字节数组
            var data = byteBlock.ToArray();
            // 返回构建好的字节数组
            return data;
        }
        finally
        {
            // 确保在离开方法前释放值字块的资源
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 当数据类型为<see cref="WSDataType.Text"/>时，将数据帧转换为文本消息。
    /// </summary>
    /// <param name="dataFrame">要转换的数据帧。</param>
    /// <param name="encoding">使用的编码方式。如果未指定（默认值），将使用UTF8编码。</param>
    /// <returns>转换后的文本消息。</returns>
    public static string ToText(this WSDataFrame dataFrame, Encoding encoding = default)
    {
        // 根据指定的编码方式或默认的UTF8编码，将数据帧的负载数据转换为字符串
        return dataFrame.PayloadData.Span.ToString(encoding == default ? Encoding.UTF8 : encoding);
    }
}