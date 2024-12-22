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

namespace TouchSocket.Core
{
    /// <summary>
    /// 提供用于处理TPackage类型的快速二进制转换器。
    /// </summary>
    /// <typeparam name="TPackage">实现了IPackage接口的类型。</typeparam>
    public sealed class PackageFastBinaryConverter<TPackage> : IFastBinaryConverter where TPackage : IPackage, new()
    {
        /// <summary>
        /// 从字节块中读取数据并转换为TPackage类型的对象。
        /// </summary>
        /// <typeparam name="TByteBlock">实现了IByteBlock接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块的引用。</param>
        /// <param name="type">要转换的类型。</param>
        /// <returns>转换后的TPackage类型的对象。</returns>
        object IFastBinaryConverter.Read<TByteBlock>(ref TByteBlock byteBlock, Type type)
        {
            var ipackage = new TPackage();
            ipackage.Unpackage(ref byteBlock);
            return ipackage;
        }

        /// <summary>
        /// 将TPackage类型的对象写入字节块中。
        /// </summary>
        /// <typeparam name="TByteBlock">实现了IByteBlock接口的字节块类型。</typeparam>
        /// <param name="byteBlock">字节块的引用。</param>
        /// <param name="obj">要写入的对象。</param>
        void IFastBinaryConverter.Write<TByteBlock>(ref TByteBlock byteBlock, in object obj)
        {
            var ipackage = (TPackage)obj;
            ipackage.Package(ref byteBlock);
        }
    }
}