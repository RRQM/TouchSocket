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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Run;
using System;
using System.Text;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 事件容器
    /// </summary>
    [EnterpriseEdition]
    internal class EventContext : WaitResult
    {
        internal string EventName { get; set; }

        internal void Serialize(ByteBlock byteBlock)
        {
            byteBlock.Write(BitConverter.GetBytes(this.Sign));
            byteBlock.Write(this.Status);

            if (!string.IsNullOrEmpty(EventName))
            {
                byte[] idBytes = Encoding.UTF8.GetBytes(this.EventName);
                byteBlock.Write((byte)idBytes.Length);
                byteBlock.Write(idBytes);
            }
            else
            {
                byteBlock.Write((byte)0);
            }
            if (this.Message != null)
            {
                byte[] mesBytes = Encoding.UTF8.GetBytes(this.Message);
                byteBlock.Write((byte)mesBytes.Length);
                byteBlock.Write(mesBytes);
            }
            else
            {
                byteBlock.Write((byte)0);
            }
        }

        internal static EventContext Deserialize(byte[] buffer, int offset)
        {
            EventContext context = new EventContext();
            context.Sign = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            context.Status = buffer[offset];
            offset += 1;
            int lenEventName = buffer[offset];
            offset += 1;
            context.EventName = Encoding.UTF8.GetString(buffer, offset, lenEventName);
            offset += lenEventName;
            int lenMes = buffer[offset];
            offset += 1;
            context.Message = Encoding.UTF8.GetString(buffer, offset, lenMes);
            return context;
        }
    }
}