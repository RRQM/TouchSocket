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
using TouchSocket.Core;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// 序列化选择器接口，用于定义如何根据不同的序列化类型来序列化和反序列化参数。
/// </summary>
public interface ISerializationSelector
{
    /// <summary>
    /// 反序列化字节块中的参数。
    /// </summary>
    /// <typeparam name="TReader">字节块的类型，必须实现IByteBlock接口。</typeparam>
    /// <param name="reader">包含序列化参数的字节块。</param>
    /// <param name="serializationType">指定的序列化类型。</param>
    /// <param name="parameterType">预期反序列化参数的类型。</param>
    /// <returns>反序列化后的参数对象。</returns>
    object DeserializeParameter<TReader>(ref TReader reader, SerializationType serializationType, Type parameterType) where TReader : IBytesReader

        ;

    /// <summary>
    /// 序列化参数并将其添加到字节块中。
    /// </summary>
    /// <typeparam name="TWriter">字节块的类型，必须实现IByteBlock接口。</typeparam>
    /// <param name="wirter">将要包含序列化参数的字节块。</param>
    /// <param name="serializationType">要使用的序列化类型。</param>
    /// <param name="parameter">要序列化的参数对象。</param>
    void SerializeParameter<TWriter>(ref TWriter wirter, SerializationType serializationType, in object parameter) where TWriter : IBytesWriter

        ;
}