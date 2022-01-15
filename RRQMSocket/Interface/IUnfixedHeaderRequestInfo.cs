//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <returns>是否满足解析包头</returns>
        bool OnParsingHeader(ByteBlock byteBlock,int length);

        /// <summary>
        /// 当收到数据，由框架封送有效载荷数据。
        /// </summary>
        /// <param name="body">载荷数据</param>
        /// <returns>是否成功有效</returns>
        DataResult OnParsingBody(byte[] body);
    }
}
