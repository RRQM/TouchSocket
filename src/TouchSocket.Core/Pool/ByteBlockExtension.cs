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

namespace TouchSocket.Core
{
    /// <summary>
    /// 提供字节块扩展方法的静态类。
    /// </summary>
    public static class ByteBlockExtension
    {
        /// <summary>
        /// 将字节块转换为字节块流。
        /// </summary>
        /// <param name="byteBlock">要转换的字节块。</param>
        /// <param name="releaseTogether">是否在释放字节块时一起释放关联的资源，默认为true。</param>
        /// <returns>一个新的字节块流对象。</returns>
        public static ByteBlockStream AsStream(this ByteBlock byteBlock, bool releaseTogether = true)
        {
            return new ByteBlockStream(byteBlock, releaseTogether);
        }

        /// <summary>
        /// 将值类型的字节块转换为普通的字节块。
        /// </summary>
        /// <param name="valueByteBlock">要转换的值类型字节块。</param>
        /// <returns>一个新的字节块对象。</returns>
        public static ByteBlock AsByteBlock(this in ValueByteBlock valueByteBlock)
        {
            return new ByteBlock(valueByteBlock);
        }
    }
}
