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
using RRQMCore.Run;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC传输类
    /// </summary>
    public class RpcContext : WaitResult
    {
        internal int MethodToken;
        internal string ID;
        internal byte Feedback;
        internal byte[] ReturnParameterBytes;
        internal List<byte[]> ParametersBytes;

        internal void Serialize(ByteBlock byteBlock)
        {
            byteBlock.Write(this.Sign);
            byteBlock.Write(this.Status);
            byteBlock.Write(this.Feedback);
            byteBlock.Write(this.MethodToken);
            byteBlock.Write(this.ID);
            byteBlock.Write(this.Message);
            byteBlock.WriteBytesPackage(this.ReturnParameterBytes);

            if (this.ParametersBytes != null && this.ParametersBytes.Count > 0)
            {
                byteBlock.Write((byte)this.ParametersBytes.Count);
                foreach (byte[] item in this.ParametersBytes)
                {
                    byteBlock.WriteBytesPackage(item);
                }
            }
            else
            {
                byteBlock.Write((byte)0);
            }
        }

        internal static RpcContext Deserialize(ByteBlock byteBlock)
        {
            RpcContext context = new RpcContext();
            context.Sign = byteBlock.ReadInt32();
            context.Status = byteBlock.ReadByte();
            context.Feedback = byteBlock.ReadByte();
            context.MethodToken = byteBlock.ReadInt32();
            context.ID = byteBlock.ReadString();
            context.Message = byteBlock.ReadString();
            context.ReturnParameterBytes = byteBlock.ReadBytesPackage();

            context.ParametersBytes = new List<byte[]>();
            byte countPar = byteBlock.ReadByte();

            for (int i = 0; i < countPar; i++)
            {
                context.ParametersBytes.Add(byteBlock.ReadBytesPackage());
            }
            return context;
        }
    }
}