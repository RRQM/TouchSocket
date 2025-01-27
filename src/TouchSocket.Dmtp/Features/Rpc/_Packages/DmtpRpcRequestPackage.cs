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
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

internal class DmtpRpcRequestPackage : WaitRouterPackage, IDmtpRpcRequestPackage
{
    private DmtpRpcCallContext m_callContext;
    private FeedbackType m_feedback;

    private string m_invokeKey;
    private Metadata m_metadata;
    private object[] m_parameters;
    private readonly Type m_returnType;
    private RpcMethod m_rpcMethod;
    private ISerializationSelector m_selector;

    private SerializationType m_serializationType;
    public DmtpRpcRequestPackage()
    {

    }

    public DmtpRpcRequestPackage(string invokeKey, IInvokeOption option, object[] parameters, Type returnType, ISerializationSelector selector)
    {
        this.m_invokeKey = invokeKey;

        if (option is DmtpInvokeOption dmtpInvokeOption)
        {
            this.m_feedback = dmtpInvokeOption.FeedbackType;
            this.m_serializationType = dmtpInvokeOption.SerializationType;
            this.m_metadata = dmtpInvokeOption.Metadata;
        }
        else if (option is InvokeOption invokeOption)
        {
            this.m_feedback = invokeOption.FeedbackType;
            this.m_serializationType = SerializationType.FastBinary;
        }
        this.m_parameters = parameters;
        this.m_returnType = returnType;
        this.m_selector = selector;
    }

    public DmtpRpcCallContext CallContext => this.m_callContext;

    /// <summary>
    /// 反馈类型
    /// </summary>
    public FeedbackType Feedback => this.m_feedback;

    /// <summary>
    /// 函数名
    /// </summary>
    public string InvokeKey => this.m_invokeKey;

    /// <summary>
    /// 元数据
    /// </summary>
    public Metadata Metadata => this.m_metadata;

    public object[] Parameters => this.m_parameters;

    public Type ReturnType => this.m_returnType;

    public RpcMethod RpcMethod => this.m_rpcMethod;

    public RpcParameter[] RpcParameters => this.RpcMethod.Parameters;

    /// <summary>
    /// 序列化类型
    /// </summary>
    public SerializationType SerializationType => this.m_serializationType;

    /// <inheritdoc/>
    protected override bool IncludedRouter => true;

    public void LoadInfo(RpcMethod rpcMethod)
    {
        this.m_rpcMethod = rpcMethod;
    }

    public void LoadInfo(DmtpRpcCallContext callContext, ISerializationSelector selector)
    {
        this.m_callContext = callContext;
        this.m_rpcMethod = callContext.RpcMethod;
        this.m_selector = selector;
    }
    /// <inheritdoc/>
    public override void PackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.PackageBody(ref byteBlock);

        if (this.m_parameters != null && this.m_parameters.Length > 0)
        {
            byteBlock.WriteByte((byte)this.m_parameters.Length);
            foreach (var item in this.m_parameters)
            {
                this.m_selector.SerializeParameter(ref byteBlock, this.SerializationType, item);
            }
        }
        else
        {
            byteBlock.WriteByte(0);
        }
    }

    /// <inheritdoc/>
    public override void PackageRouter<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.PackageRouter(ref byteBlock);
        byteBlock.WriteByte((byte)this.m_serializationType);
        byteBlock.WriteString(this.InvokeKey, FixedHeaderType.Byte);
        byteBlock.WriteByte((byte)this.m_feedback);
        byteBlock.WritePackage(this.Metadata);
    }

    /// <inheritdoc/>
    public override void UnpackageBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.UnpackageBody(ref byteBlock);
        var countPar = byteBlock.ReadByte();
        var ps = new object[this.RpcParameters.Length];

        var index = 0;
        for (var i = 0; i < ps.Length; i++)
        {
            var parameter = this.RpcParameters[i];
            if (parameter.IsCallContext)
            {
                ps[i] = this.m_callContext;
            }
            else if (parameter.IsFromServices)
            {
                ps[i] = this.m_callContext.Resolver.Resolve(parameter.Type);
            }
            else if (index < countPar)
            {
                ps[i] = this.m_selector.DeserializeParameter(ref byteBlock, this.SerializationType, parameter.Type);

                index++;
            }
            else if (parameter.ParameterInfo.HasDefaultValue)
            {
                ps[i] = parameter.ParameterInfo.DefaultValue;
            }
            else
            {
                ps[i] = parameter.Type.GetDefault();
            }
        }

        this.m_parameters = ps;
    }

    /// <inheritdoc/>
    public override void UnpackageRouter<TByteBlock>(ref TByteBlock byteBlock)
    {
        base.UnpackageRouter(ref byteBlock);
        this.m_serializationType = (SerializationType)byteBlock.ReadByte();
        this.m_invokeKey = byteBlock.ReadString(FixedHeaderType.Byte);
        this.m_feedback = (FeedbackType)byteBlock.ReadByte();
        if (!byteBlock.ReadIsNull())
        {
            var package = new Metadata();
            package.Unpackage(ref byteBlock);
            this.m_metadata = package;
        }
    }
}