//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// GZip压缩算法的压缩机
    /// </summary>
    [IntelligentCoder.AsyncMethodPoster(Flags = IntelligentCoder.MemberFlags.Public)]
    public  sealed partial class GZipDataCompressor : IDataCompressor
    {
        byte[] IDataCompressor.Compress(ArraySegment<byte> data)
        {
            return GZip.Compress(data.Array, data.Offset, data.Count);
        }

        byte[] IDataCompressor.Decompress(ArraySegment<byte> data)
        {
            return GZip.Decompress(data.Array, data.Offset, data.Count);
        }
    }
}