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

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace TouchSocket;

internal sealed class XmlRpcClientCodeBuilder : RpcClientCodeBuilder
{
    public const string XmlRpcAttribute = "TouchSocket.XmlRpc.XmlRpcAttribute";

    public XmlRpcClientCodeBuilder(INamedTypeSymbol rpcApi) : base(rpcApi, XmlRpcAttribute)
    {
    }

    protected override string RpcAttributeName => "XmlRpc";

    protected override string GetInheritedClassName(INamedTypeSymbol rpcApi)
    {
        return new XmlRpcClientCodeBuilder(rpcApi).GetClassName();
    }

    protected override string[] GetGenericConstraintTypes(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
    {
        var strings = new List<string>();
        strings.AddRange(base.GetGenericConstraintTypes(method, namedArguments));

        strings.Add("TouchSocket.XmlRpc.IXmlRpcClient");
        return strings.ToArray();
    }
}