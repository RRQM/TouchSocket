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
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Run;
using RRQMCore.Serialization;
using System.Collections.Generic;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC传输类
    /// </summary>
    public sealed class RpcContext : WaitResult, IRpcContext
    {
        internal string id;
        internal int methodToken;
        internal string methodName;
        internal List<byte[]> parametersBytes;
        internal byte[] returnParameterBytes;
        internal int timeout;
        private byte feedback;
        private byte invokeType;
        private byte serializationType;

        /// <summary>
        /// 反馈类型
        /// </summary>
        public byte Feedback
        {
            get { return this.feedback; }
        }

        /// <summary>
        /// 调用ID
        /// </summary>
        public string ID
        {
            get { return this.id; }
        }

        /// <summary>
        /// 调用超时设置
        /// </summary>
        public int Timeout
        {
            get { return this.timeout; }
        }


        /// <summary>
        /// 调用类型
        /// </summary>
        public InvokeType InvokeType
        {
            get { return (InvokeType)this.invokeType; }
        }

        /// <summary>
        /// 函数键
        /// </summary>
        public int MethodToken
        {
            get { return this.methodToken; }
        }

        /// <summary>
        /// 函数名
        /// </summary>
        public string MethodName
        {
            get { return this.methodName; }
        }

        /// <summary>
        /// 参数数据
        /// </summary>
        public List<byte[]> ParametersBytes
        {
            get { return this.parametersBytes; }
        }

        /// <summary>
        /// 反回参数数据
        /// </summary>
        public byte[] ReturnParameterBytes
        {
            get { return this.returnParameterBytes; }
        }

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType
        {
            get { return (SerializationType)this.serializationType; }
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static RpcContext Deserialize(ByteBlock byteBlock)
        {
            RpcContext context = new RpcContext();
            context.timeout = byteBlock.ReadInt32();
            context.sign = byteBlock.ReadInt32();
            context.status = byteBlock.ReadByte();
            context.invokeType = byteBlock.ReadByte();
            context.feedback = byteBlock.ReadByte();
            context.serializationType = byteBlock.ReadByte();
            context.methodToken = byteBlock.ReadInt32();
            context.methodName = byteBlock.ReadString();
            context.id = byteBlock.ReadString();
            context.message = byteBlock.ReadString();
            context.returnParameterBytes = byteBlock.ReadBytesPackage();

            byte countPar = byteBlock.ReadByte();
            context.parametersBytes = new List<byte[]>();
            for (int i = 0; i < countPar; i++)
            {
                context.parametersBytes.Add(byteBlock.ReadBytesPackage());
            }
            return context;
        }

        internal void LoadInvokeOption(IInvokeOption option)
        {
            InvokeOption invokeOption = (InvokeOption)option;
            this.invokeType = (byte)invokeOption.InvokeType;
            this.feedback = (byte)invokeOption.FeedbackType;
            this.serializationType = (byte)invokeOption.SerializationType;
            this.timeout = option.Timeout;
        }

        /// <summary>
        /// 编包
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Serialize(ByteBlock byteBlock)
        {
            byteBlock.Write(this.timeout);
            byteBlock.Write(this.sign);
            byteBlock.Write(this.status);
            byteBlock.Write(this.invokeType);
            byteBlock.Write(this.feedback);
            byteBlock.Write(this.serializationType);
            byteBlock.Write(this.methodToken);
            byteBlock.Write(this.methodName);
            byteBlock.Write(this.id);
            byteBlock.Write(this.message);
            byteBlock.WriteBytesPackage(this.returnParameterBytes);

            if (this.parametersBytes != null && this.parametersBytes.Count > 0)
            {
                byteBlock.Write((byte)this.parametersBytes.Count);
                foreach (byte[] item in this.parametersBytes)
                {
                    byteBlock.WriteBytesPackage(item);
                }
            }
            else
            {
                byteBlock.Write((byte)0);
            }
        }


        internal void ThrowStatus()
        {
            switch (this.Status)
            {
                case 0:
                    {
                        throw new RRQMRPCException($"返回状态异常，信息：{this.Message}");
                    }
                case 1:
                    {
                        return;//正常。
                    }
                case 2:
                    {
                        throw new RRQMRPCInvokeException($"未找到该公共方法，或该方法未标记{nameof(RPCAttribute)}");
                    }
                case 3:
                    {
                        throw new RRQMRPCException("该方法已被禁用");
                    }
                case 4:
                    {
                        throw new RRQMAbandonRPCException($"服务器已阻止本次行为，信息：{this.Message}");
                    }
                case 5:
                    {
                        throw new RRQMRPCInvokeException($"函数执行异常，详细信息：{this.Message}");
                    }
                case 6:
                    {
                        throw new RRQMRPCException($"函数异常，信息：{this.Message}");
                    }
                case 7:
                    {
                        throw new ClientNotFindException(ResType.ClientNotFind.GetResString(this.ID));
                    }
                default:
                    throw new RRQMRPCException($"未知状态定义，信息：{this.Message}");
            }
        }
    }
}