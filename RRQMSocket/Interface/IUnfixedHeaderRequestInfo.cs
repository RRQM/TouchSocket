//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 用户自定义不固定包头请求
    /// </summary>
    public interface IUnfixedHeaderRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 数据体长度
        /// </summary>
        int BodyLength { get; }

        /// <summary>
        /// 当收到数据，由框架封送数据，您需要在此函数中，解析自己的数据包头。
        /// <para>如果满足包头的解析，请返回True，并且递增整个包头的长度到<see cref="ByteBlock.Pos"/>，然后赋值<see cref="BodyLength"/></para>
        /// <para>如果返回false，意味着缓存剩余数据，此时如果仅仅是因为长度不足，则不必修改其他。</para>
        /// <para>但是如果是因为数据错误，则需要修改<see cref="ByteBlock.Pos"/>到正确位置，如果都不正确，则设置<see cref="ByteBlock.Pos"/>等于<see cref="ByteBlock.Len"/></para>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <returns>是否满足解析包头</returns>
        bool OnParsingHeader(ByteBlock byteBlock, int length);

        /// <summary>
        /// 当收到数据，由框架封送有效载荷数据。
        /// <para>如果返回false，意味着放弃本次解析的所有数据，包括已经解析完成的Header</para>
        /// </summary>
        /// <param name="body">载荷数据</param>
        /// <returns>是否成功有效</returns>
        bool OnParsingBody(byte[] body);
    }
}
