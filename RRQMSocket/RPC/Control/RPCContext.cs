//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC传输类
    /// </summary>
    [Serializable]
    public class RPCContext : WaitResult
    {
        internal string Method;
        internal byte Feedback;
        internal byte[] ReturnParameterBytes;
        internal object Flag;
        internal List<byte[]> ParametersBytes;

        internal void Serialize(ByteBlock byteBlock)
        {
            byteBlock.Write(BitConverter.GetBytes(this.Sign));
            byteBlock.Write(this.Status);
            byteBlock.Write(this.Feedback);

            if (this.Message != null)
            {
                byte[] mesBytes = Encoding.UTF8.GetBytes(this.Message);
                byteBlock.Write((byte)mesBytes.Length);
                byteBlock.Write(mesBytes);
            }
            else
            {
                byteBlock.Write(0);
            }

            if (this.Method != null)
            {
                byte[] meBytes = Encoding.UTF8.GetBytes(this.Method);
                byteBlock.Write((byte)meBytes.Length);
                byteBlock.Write(meBytes);
            }
            else
            {
                byteBlock.Write(0);
            }

            if (this.ReturnParameterBytes != null)
            {
                byteBlock.Write(BitConverter.GetBytes(this.ReturnParameterBytes.Length));
                byteBlock.Write(this.ReturnParameterBytes);
            }
            else
            {
                byteBlock.Write(BitConverter.GetBytes(0));
            }

            if (this.ParametersBytes != null)
            {
                byteBlock.Write((byte)this.ParametersBytes.Count);
                foreach (byte[] item in this.ParametersBytes)
                {
                    if (item != null)
                    {
                        byteBlock.Write(BitConverter.GetBytes(item.Length));
                        byteBlock.Write(item);
                    }
                    else
                    {
                        byteBlock.Write(BitConverter.GetBytes(0));
                    }
                }

            }
            else
            {
                byteBlock.Write(0);
            }

        }

        internal static RPCContext Deserialize(byte[] buffer, int offset)
        {
            RPCContext context = new RPCContext();
            context.Sign = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            context.Status = buffer[offset];
            offset += 1;
            context.Feedback = buffer[offset];
            offset += 1;
            int lenMes = buffer[offset];
            offset += 1;
            context.Message = Encoding.UTF8.GetString(buffer, offset, lenMes);
            offset += lenMes;
            int lenMet = buffer[offset];
            offset += 1;
            context.Method = Encoding.UTF8.GetString(buffer, offset, lenMet);
            offset += lenMet;
            int lenRet = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            if (lenRet > 0)
            {
                context.ReturnParameterBytes = new byte[lenRet];
                Array.Copy(buffer, offset, context.ReturnParameterBytes, 0, lenRet);
            }
            offset += lenRet;
            context.ParametersBytes = new List<byte[]>();
            int countPar = buffer[offset];
            offset += 1;
            for (int i = 0; i < countPar; i++)
            {
                int lenPar = BitConverter.ToInt32(buffer, offset);
                offset += 4;
                if (lenPar > 0)
                {
                    byte[] datas = new byte[lenPar];
                    Array.Copy(buffer, offset, datas, 0, lenPar);
                    offset += lenPar;
                    context.ParametersBytes.Add(datas);
                }
                else
                {
                    context.ParametersBytes.Add(null);
                }
            }
            return context;
        }
    }
}