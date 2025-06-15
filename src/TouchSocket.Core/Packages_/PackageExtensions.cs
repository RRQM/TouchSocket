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
/// PackageExtensions
/// </summary>
public static class PackageExtensions
{
    /// <summary>
    /// 将指定类型的实例序列化为字节流
    /// </summary>
    /// <typeparam name="TPackage">实现IPackage接口的类型</typeparam>
    /// <param name="package">要序列化的实例</param>
    /// <returns>序列化后的字节流</returns>
    public static byte[] PackageAsBytes<TPackage>(this TPackage package) where TPackage : IPackage
    {
        // 创建一个字节块对象，用于存储序列化的字节数据
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            // 调用IPackage接口的Package方法，将实例序列化到字节块中
            package.Package(ref byteBlock);
            // 将字节块转换为字节数组并返回
            return byteBlock.ToArray();
        }
        finally
        {
            // 释放字节块资源
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 将指定类型的对象打包为字节块。
    /// </summary>
    /// <param name="package">要打包的对象，必须实现IPackage接口。</param>
    /// <param name="byteBlock">用于存储打包后数据的字节块。</param>
    /// <typeparam name="TPackage">要打包的对象的类型，必须实现IPackage接口。</typeparam>
    public static void Package<TPackage>(this TPackage package, ByteBlock byteBlock) where TPackage : IPackage
    {
        package.Package(ref byteBlock);
    }

    /// <summary>
    /// 从字节块中解包出指定类型的对象。
    /// </summary>
    /// <param name="package">要解包的对象，必须实现IPackage接口。</param>
    /// <param name="byteBlock">包含要解包数据的字节块。</param>
    /// <typeparam name="TPackage">要解包对象的类型，必须实现IPackage接口。</typeparam>
    public static void Unpackage<TPackage>(this TPackage package, ByteBlock byteBlock) where TPackage : IPackage
    {
        package.Unpackage(ref byteBlock);
    }
}