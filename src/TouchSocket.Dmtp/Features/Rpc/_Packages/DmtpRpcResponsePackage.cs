//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.Rpc
{
    internal class DmtpRpcResponsePackage : WaitRouterPackage
    {
        private object m_returnParameter;

        private Type m_returnType;

        private ISerializationSelector m_selector;

        private SerializationType m_serializationType;

        public DmtpRpcResponsePackage()
        {
        }

        public DmtpRpcResponsePackage(DmtpRpcRequestPackage requestPackage, ISerializationSelector selector, object returnParameter)
        {
            this.TargetId = requestPackage.SourceId;
            this.SourceId = requestPackage.TargetId;
            this.Sign = requestPackage.Sign;
            this.m_returnParameter = returnParameter;
            this.Status = (byte)TouchSocketDmtpStatus.Success;
            this.m_selector = selector;
            this.m_serializationType = requestPackage.SerializationType;
        }

        public DmtpRpcResponsePackage(DmtpRpcRequestPackage requestPackage, ISerializationSelector selector, TouchSocketDmtpStatus status, string message)
        {
            this.TargetId = requestPackage.SourceId;
            this.SourceId = requestPackage.TargetId;
            this.Sign = requestPackage.Sign;
            this.Status = (byte)status;
            this.Message = message;
            this.m_selector = selector;
            this.m_serializationType = requestPackage.SerializationType;
        }

        /// <summary>
        /// 返回参数数据
        /// </summary>
        public object ReturnParameter { get => this.m_returnParameter; }

        /// <inheritdoc/>
        protected override bool IncludedRouter => true;

        public void LoadInfo(Type retuenType, ISerializationSelector selector, SerializationType serializationType)
        {
            this.m_returnType = retuenType;
            this.m_selector = selector;
            this.m_serializationType = serializationType;
        }

        /// <inheritdoc/>
        public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.PackageBody(ref byteBlock);

            this.m_selector.SerializeParameter(ref byteBlock, this.m_serializationType, this.ReturnParameter);
        }

        /// <inheritdoc/>
        public override void PackageRouter<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.PackageRouter(ref byteBlock);
            byteBlock.WriteByte((byte)this.m_serializationType);
        }

        /// <inheritdoc/>
        public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.UnpackageBody(ref byteBlock);
            if (this.m_returnType != null)
            {
                this.m_returnParameter = this.m_selector.DeserializeParameter(ref byteBlock, this.m_serializationType, this.m_returnType);
            }
        }

        /// <inheritdoc/>
        public override void UnpackageRouter<TByteBlock>(ref TByteBlock byteBlock)
        {
            base.UnpackageRouter(ref byteBlock);
            this.m_serializationType = (SerializationType)byteBlock.ReadByte();
        }

        internal void ThrowStatus()
        {
            if (this.Status == 1)
            {
                return;
            }
            switch (this.Status.ToStatus())
            {
                case TouchSocketDmtpStatus.ClientNotFind:
                    {
                        throw new ClientNotFindException(TouchSocketDmtpStatus.ClientNotFind.GetDescription(this.TargetId));
                    }
                case TouchSocketDmtpStatus.UnknownError:
                case TouchSocketDmtpStatus.RpcMethodNotFind:
                case TouchSocketDmtpStatus.RpcMethodDisable:
                case TouchSocketDmtpStatus.RemoteRefuse:
                case TouchSocketDmtpStatus.RpcInvokeException:
                case TouchSocketDmtpStatus.RoutingNotAllowed:
                case TouchSocketDmtpStatus.Exception:
                default:
                    {
                        throw new RpcException(this.Status.ToStatus().GetDescription(this.Message));
                    }
            }
        }
    }
}