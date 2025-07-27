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
    public override void PackageBody<TWriter>(ref TWriter writer)
    {
        base.PackageBody(ref writer);

        if (this.m_parameters != null && this.m_parameters.Length > 0)
        {
            WriterExtension.WriteValue<TWriter,byte>(ref writer,(byte)this.m_parameters.Length);
            foreach (var item in this.m_parameters)
            {
                this.m_selector.SerializeParameter(ref writer, this.SerializationType, item);
            }
        }
        else
        {
            WriterExtension.WriteValue<TWriter,byte>(ref writer,0);
        }
    }

    /// <inheritdoc/>
    public override void PackageRouter<TWriter>(ref TWriter writer)
    {
        base.PackageRouter(ref writer);
        WriterExtension.WriteValue<TWriter,byte>(ref writer,(byte)this.m_serializationType);
        WriterExtension.WriteString(ref writer,this.InvokeKey, FixedHeaderType.Byte);
        WriterExtension.WriteValue<TWriter,byte>(ref writer,(byte)this.m_feedback);
        if (this.Metadata is null)
        {
            WriterExtension.WriteNull(ref writer);
        }
        else
        {
             WriterExtension.WriteNotNull(ref writer);
            this.Metadata.Package(ref writer);
        }
    }

    /// <inheritdoc/>
    public override void UnpackageBody<TReader>(ref TReader reader)
    {
        base.UnpackageBody(ref reader);
        var countPar = ReaderExtension.ReadValue<TReader,byte>(ref reader);
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
                ps[i] = this.m_selector.DeserializeParameter(ref reader, this.SerializationType, parameter.Type);

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
    public override void UnpackageRouter<TReader>(ref TReader reader)
    {
        base.UnpackageRouter(ref reader);
        this.m_serializationType = (SerializationType)ReaderExtension.ReadValue<TReader,byte>(ref reader);
        this.m_invokeKey = ReaderExtension.ReadString(ref reader,FixedHeaderType.Byte);
        this.m_feedback = (FeedbackType)ReaderExtension.ReadValue<TReader,byte>(ref reader);
        if (!ReaderExtension.ReadIsNull(ref reader))
        {
            var package = new Metadata();
            package.Unpackage(ref reader);
            this.m_metadata = package;
        }
    }
}