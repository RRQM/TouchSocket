using System;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.Rpc
{
    internal class DmtpRpcResponsePackage : WaitRouterPackage
    {
        public DmtpRpcResponsePackage()
        {

        }

        public void LoadInfo(Type retuenType, ISerializationSelector selector, SerializationType serializationType)
        {
            this.m_returnType = retuenType;
            this.m_selector = selector;
            this.m_serializationType = serializationType;
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

        private ISerializationSelector m_selector;

        private SerializationType m_serializationType;
        private object m_returnParameter;
        private Type m_returnType;

        /// <summary>
        /// 返回参数数据
        /// </summary>
        public object ReturnParameter { get => this.m_returnParameter; }

        /// <inheritdoc/>
        protected override bool IncludedRouter => true;
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
