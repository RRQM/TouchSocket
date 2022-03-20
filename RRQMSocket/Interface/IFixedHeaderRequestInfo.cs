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
namespace RRQMSocket
{
    /// <summary>
    /// 用户自定义固定包头请求
    /// </summary>
    public interface IFixedHeaderRequestInfo : IRequestInfo
    {
        /// <summary>
        /// 数据体长度
        /// </summary>
        int BodyLength { get; }

        /// <summary>
        /// 当收到数据，由框架封送固定协议头。
        /// <para>您需要在此函数中，解析自己的固定包头，并且对<see cref="BodyLength"/>赋值后续数据的长度，然后返回True。</para>
        /// <para>如果返回false，则意味着放弃本次解析</para>
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        bool OnParsingHeader(byte[] header);

        /// <summary>
        /// 当收到数据，由框架封送有效载荷数据。
        /// <para>如果返回false，意味着放弃本次解析的所有数据，包括已经解析完成的Header</para>
        /// </summary>
        /// <param name="body">载荷数据</param>
        /// <returns>是否成功有效</returns>
        bool OnParsingBody(byte[] body);
    }
}