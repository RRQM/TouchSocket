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

//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using TouchSocket.Core;

//namespace TouchSocket.Dmtp;

//public sealed class DmtpAdapterSlim : DataHandlingAdapterSlim<DmtpMessage>
//{
//    protected override bool ParseRequestCore<TReader>(ref TReader reader, out DmtpMessage request)
//    {
//        if (reader.BytesRemaining < 8)
//        {
//            request = default;
//            return  false;
//        }
//        var header = reader.GetSpan(8);
//        var offset = 0;
//        if (header[offset++] != DmtpMessage.Head[0] || header[offset++] != DmtpMessage.Head[1])
//        {
//            throw new Exception("这可能不是Dmtp协议数据");
//        }
//        var protocolFlags = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(offset));
//        offset += 2;
//        var m_bodyLength = TouchSocketBitConverter.BigEndian.To<int>(header.Slice(offset));

//        if (reader.BytesRemaining < m_bodyLength + 8)
//        {
//            request = default;
//            return false;
//        }

//        reader.Advance(8);
//        var bodyMemory = reader.GetMemory(m_bodyLength);
//        reader.Advance(m_bodyLength);

//        request = new DmtpMessage(protocolFlags, bodyMemory);
//        return true;
//    }
//}