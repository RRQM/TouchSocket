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
/// 定义了快速二进制转换器的接口，用于将对象转换为字节块，反之亦然。
/// </summary>
public interface IFastBinaryConverter
{
    /// <summary>
    /// 从字节块中读取对象。
    /// </summary>
    /// <param name="reader">包含对象数据的字节块。</param>
    /// <param name="type">要读取的对象的类型。</param>
    /// <typeparam name="TReader">字节块的类型，实现了IByteBlock接口。</typeparam>
    /// <returns>从字节块中读取的对象实例。</returns>
    object Read<TReader>(ref TReader reader, Type type) where TReader : IBytesReader;

    /// <summary>
    /// 将对象写入字节块。
    /// </summary>
    /// <param name="writer">将要包含对象数据的字节块。</param>
    /// <param name="obj">要写入的对象实例。</param>
    /// <typeparam name="TWriter">字节块的类型，实现了IByteBlock接口。</typeparam>
    void Write<TWriter>(ref TWriter writer, in object obj) where TWriter : IBytesWriter;
}

/// <summary>
/// 提供了一个抽象类，实现了IFastBinaryConverter接口，用于快速二进制转换。
/// </summary>
/// <typeparam name="T">具体实现类的类型参数。</typeparam>
public abstract class FastBinaryConverter<T> : IFastBinaryConverter
{
    /// <summary>
    /// 通过此实现从字节块中读取对象。
    /// </summary>
    /// <param name="reader">包含对象数据的字节块。</param>
    /// <param name="type">要读取的对象的类型。</param>
    /// <typeparam name="TReader">字节块的类型，实现了IByteBlock接口。</typeparam>
    /// <returns>从字节块中读取的对象实例。</returns>
    object IFastBinaryConverter.Read<TReader>(ref TReader reader, Type type)
    {
        return this.Read(ref reader, type);
    }

    /// <summary>
    /// 通过此实现将对象写入字节块。
    /// </summary>
    /// <param name="writer">将要包含对象数据的字节块。</param>
    /// <param name="obj">要写入的对象实例。</param>
    /// <typeparam name="TWriter">字节块的类型，实现了IByteBlock接口。</typeparam>
    void IFastBinaryConverter.Write<TWriter>(ref TWriter writer, in object obj)
    {
        this.Write(ref writer, (T)obj);
    }

    /// <summary>
    /// 从字节块中读取对象。必须由具体实现类实现。
    /// </summary>
    /// <param name="reader">包含对象数据的字节块。</param>
    /// <param name="type">要读取的对象的类型。</param>
    /// <typeparam name="TReader">字节块的类型，实现了IByteBlock接口。</typeparam>
    /// <returns>从字节块中读取的对象实例。</returns>
    protected abstract T Read<TReader>(ref TReader reader, Type type) where TReader : IBytesReader;

    /// <summary>
    /// 将对象写入字节块。必须由具体实现类实现。
    /// </summary>
    /// <param name="writer">将要包含对象数据的字节块。</param>
    /// <param name="obj">要写入的对象实例。</param>
    /// <typeparam name="TWriter">字节块的类型，实现了IByteBlock接口。</typeparam>
    protected abstract void Write<TWriter>(ref TWriter writer, in T obj) where TWriter : IBytesWriter;
}