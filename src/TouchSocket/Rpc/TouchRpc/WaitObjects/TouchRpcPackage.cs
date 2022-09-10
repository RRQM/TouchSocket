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
using System.Collections.Generic;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Run;
using TouchSocket.Core.Serialization;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// Rpc传输类
    /// </summary>
    public sealed class TouchRpcPackage : WaitResult
    {
        internal string id;
        internal string methodName;
        internal List<byte[]> parametersBytes;
        internal byte[] returnParameterBytes;
        internal int timeout;
        private byte feedback;
        private byte serializationType;
        internal bool isByRef;

        /// <summary>
        /// 反馈类型
        /// </summary>
        public byte Feedback => this.feedback;

        /// <summary>
        /// 调用ID
        /// </summary>
        public string ID => this.id;

        /// <summary>
        /// 调用超时设置
        /// </summary>
        public int Timeout => this.timeout;


        /// <summary>
        /// 函数名
        /// </summary>
        public string MethodName => this.methodName;

        /// <summary>
        /// 参数数据
        /// </summary>
        public List<byte[]> ParametersBytes => this.parametersBytes;

        /// <summary>
        /// 反回参数数据
        /// </summary>
        public byte[] ReturnParameterBytes => this.returnParameterBytes;

        /// <summary>
        /// 序列化类型
        /// </summary>
        public SerializationType SerializationType => (SerializationType)this.serializationType;

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static TouchRpcPackage Deserialize(ByteBlock byteBlock)
        {
            TouchRpcPackage context = new TouchRpcPackage();
            context.timeout = byteBlock.ReadInt32();
            context.Sign = byteBlock.ReadInt64();
            context.Status = (byte)byteBlock.ReadByte();
            context.feedback = (byte)byteBlock.ReadByte();
            context.serializationType = (byte)byteBlock.ReadByte();
            context.isByRef = byteBlock.ReadBoolean();
            context.methodName = byteBlock.ReadString();
            context.id = byteBlock.ReadString();
            context.Message = byteBlock.ReadString();
            context.returnParameterBytes = byteBlock.ReadBytesPackage();

            byte countPar = (byte)byteBlock.ReadByte();
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
            byteBlock.Write(this.Sign);
            byteBlock.Write(this.Status);
            byteBlock.Write(this.feedback);
            byteBlock.Write(this.serializationType);
            byteBlock.Write(this.isByRef);
            byteBlock.Write(this.methodName);
            byteBlock.Write(this.id);
            byteBlock.Write(this.Message);
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
                        throw new RpcException($"返回状态异常，信息：{this.Message}");
                    }
                case 1:
                    {
                        return;//正常。
                    }
                case 2:
                    {
                        throw new RpcException($"未找到该公共方法，或该方法未标记{nameof(RpcAttribute)}");
                    }
                case 3:
                    {
                        throw new RpcException("该方法已被禁用");
                    }
                case 4:
                    {
                        throw new RpcException($"服务器已阻止本次行为，信息：{this.Message}");
                    }
                case 5:
                    {
                        throw new RpcInvokeException($"函数执行异常，详细信息：{this.Message}");
                    }
                case 6:
                    {
                        throw new RpcException($"函数异常，信息：{this.Message}");
                    }
                case 7:
                    {
                        throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(this.ID));
                    }
                default:
                    throw new RpcException($"未知状态定义，信息：{this.Message}");
            }
        }
    }
}