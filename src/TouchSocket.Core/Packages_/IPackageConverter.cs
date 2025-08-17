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

namespace TouchSocket.Core;
/// <summary>
/// 定义用于将数据包类型 <typeparamref name="T"/> 与字节流进行转换的接口。
/// </summary>
/// <typeparam name="T">要转换的数据包类型。</typeparam>
public interface IPackageConverter<T>
{
    /// <summary>
    /// 将 <typeparamref name="T"/> 类型的数据包写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须实现 <see cref="IBytesWriter"/> 接口。</typeparam>
    /// <param name="writer">字节写入器的引用。</param>
    /// <param name="value">要写入的数据包。</param>
    void Package<TWriter>(ref TWriter writer, T value)
        where TWriter : IBytesWriter;

    /// <summary>
    /// 从字节读取器中读取并还原为 <typeparamref name="T"/> 类型的数据包。
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型，必须实现 <see cref="IBytesReader"/> 接口。</typeparam>
    /// <param name="reader">字节读取器的引用。</param>
    /// <returns>还原后的数据包。</returns>
    T Unpackage<TReader>(ref TReader reader)
        where TReader : IBytesReader;
}
