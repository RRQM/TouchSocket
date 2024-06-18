////------------------------------------------------------------------------------
////  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  API首页：https://touchsocket.net/
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------

//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.Xml.Linq;
//using TouchSocket.Core;
//using TouchSocket.Resources;
//using TouchSocket.Rpc;
//using TouchSocket.Sockets;

//namespace TouchSocket.Dmtp.Rpc
//{
//    /// <summary>
//    /// Rpc传输类
//    /// </summary>
//    public sealed class DmtpRpcPackage : WaitRouterPackage
//    {
//        private FeedbackType m_feedback;

//        private SerializationSelector m_selector;

//        private SerializationType m_serializationType;

//        public DmtpRpcCallContext CallContext { get; set; }

//        /// <summary>
//        /// 反馈类型
//        /// </summary>
//        public FeedbackType Feedback => m_feedback;

//        /// <summary>
//        /// 函数名
//        /// </summary>
//        public string InvokeKey { get; internal set; }

//        /// <summary>
//        /// 参数是否包含引用类型
//        /// </summary>
//        public bool IsByRef { get; internal set; }

//        public bool IsRequest { get; set; }

//        /// <summary>
//        /// 元数据
//        /// </summary>
//        public Metadata Metadata { get; internal set; }

//        /// <summary>
//        /// 参数数据
//        /// </summary>
//        public object[] Parameters { get; set; }

//        public IResolver Resolver { get; set; }

//        /// <summary>
//        /// 返回参数数据
//        /// </summary>
//        public object ReturnParameter { get; set; }

//        public Type ReturnType { get; set; }
//        public RpcMethod RpcMethod { get; set; }
//        public RpcParameter[] RpcParameters { get; set; }
//        public SerializationSelector Selector { get => this.m_selector; set => this.m_selector = value; }

//        /// <summary>
//        /// 序列化类型
//        /// </summary>
//        public SerializationType SerializationType => m_serializationType;

//        /// <inheritdoc/>
//        protected override bool IncludedRouter => true;
//        /// <inheritdoc/>
//        public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
//        {
//            base.PackageBody(ref byteBlock);

//            if (this.IsRequest)
//            {
//                if (this.Parameters != null && this.Parameters.Length > 0)
//                {
//                    byteBlock.Write((byte)this.Parameters.Length);
//                    foreach (var item in this.Parameters)
//                    {
//                        this.m_selector.SerializeParameter(ref byteBlock, this.SerializationType, item);
//                    }
//                }
//                else
//                {
//                    byteBlock.Write((byte)0);
//                }
//            }
//            else
//            {
//                this.m_selector.SerializeParameter(ref byteBlock, this.m_serializationType, this.ReturnParameter);
//            }
//        }

//        /// <inheritdoc/>
//        public override void PackageRouter<TByteBlock>(ref TByteBlock byteBlock)
//        {
//            base.PackageRouter(ref byteBlock);
//            byteBlock.Write((byte)this.m_serializationType);
//            byteBlock.Write(this.InvokeKey, FixedHeaderType.Byte);
//            byteBlock.Write((byte)this.m_feedback);
//            byteBlock.Write(this.IsByRef);
//            byteBlock.WritePackage(this.Metadata);
//        }

//        /// <inheritdoc/>
//        public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
//        {
//            base.UnpackageBody(ref byteBlock);
//            if (this.IsRequest)
//            {
//                if (this.ReturnType != null)
//                {
//                    this.ReturnParameter = this.m_selector.DeserializeParameter(ref byteBlock, this.m_serializationType, this.ReturnType);
//                }
//            }
//            else
//            {
//                var countPar = (byte)byteBlock.ReadByte();
//                var ps = new object[RpcParameters.Length];

//                var index = 0;
//                for (var i = 0; i < ps.Length; i++)
//                {
//                    var parameter = RpcParameters[i];
//                    if (parameter.IsCallContext)
//                    {
//                        ps[i] = this.CallContext;
//                    }
//                    else if (parameter.IsFromServices)
//                    {
//                        ps[i] = this.Resolver.Resolve(parameter.Type);
//                    }
//                    else if (index < countPar)
//                    {
//                        ps[i] = this.m_selector.DeserializeParameter(ref byteBlock, this.SerializationType, parameter.Type);

//                        index++;
//                    }
//                    else if (parameter.ParameterInfo.HasDefaultValue)
//                    {
//                        ps[i] = parameter.ParameterInfo.DefaultValue;
//                    }
//                    else
//                    {
//                        ps[i] = parameter.Type.GetDefault();
//                    }
//                }

//                this.Parameters = ps;
//            }
//        }

//        /// <inheritdoc/>
//        public override void UnpackageRouter<TByteBlock>(ref TByteBlock byteBlock)
//        {
//            base.UnpackageRouter(ref byteBlock);
//            this.m_serializationType = (SerializationType)byteBlock.ReadByte();
//            this.InvokeKey = byteBlock.ReadString(FixedHeaderType.Byte);
//            this.m_feedback = (FeedbackType)byteBlock.ReadByte();
//            this.IsByRef = byteBlock.ReadBoolean();
//            if (!byteBlock.ReadIsNull())
//            {
//                var package = new Metadata();
//                package.Unpackage(ref byteBlock);
//                this.Metadata = package;
//            }
//        }

//        internal void LoadInvokeOption(IInvokeOption option)
//        {
//            if (option is DmtpInvokeOption dmtpInvokeOption)
//            {
//                this.m_feedback = dmtpInvokeOption.FeedbackType;
//                this.m_serializationType = dmtpInvokeOption.SerializationType;
//                this.Metadata = dmtpInvokeOption.Metadata;
//            }
//            else if (option is InvokeOption invokeOption)
//            {
//                this.m_feedback = invokeOption.FeedbackType;
//            }
//        }

//        internal void ThrowStatus()
//        {
//            if (this.Status == 1)
//            {
//                return;
//            }
//            switch (this.Status.ToStatus())
//            {
//                case TouchSocketDmtpStatus.ClientNotFind:
//                    {
//                        throw new ClientNotFindException(TouchSocketDmtpStatus.ClientNotFind.GetDescription(this.TargetId));
//                    }
//                case TouchSocketDmtpStatus.UnknownError:
//                case TouchSocketDmtpStatus.RpcMethodNotFind:
//                case TouchSocketDmtpStatus.RpcMethodDisable:
//                case TouchSocketDmtpStatus.RemoteRefuse:
//                case TouchSocketDmtpStatus.RpcInvokeException:
//                case TouchSocketDmtpStatus.RoutingNotAllowed:
//                case TouchSocketDmtpStatus.Exception:
//                default:
//                    {
//                        throw new RpcException(this.Status.ToStatus().GetDescription(this.Message));
//                    }
//            }
//        }
//    }
//}