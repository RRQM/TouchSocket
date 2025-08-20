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
using System.Linq;
using System.Reflection;
using System.Text;

namespace TouchSocket.Rpc;

internal static class RpcUtils
{
    public const string GeneratorRpcProxyAttributeTypeName = "TouchSocket.Rpc.GeneratorRpcProxyAttribute";
    public const string RpcAttributeTypeName = "TouchSocket.Rpc.RpcAttribute";
    public const string FromServicesAttributeTypeName = "TouchSocket.Rpc.FromServicesAttribute";
    public const string ICallContextTypeName = "TouchSocket.Rpc.ICallContext";
    public const string FromServicesAttributeName = "TouchSocket.Rpc.FromServicesAttribute";

    public static StringBuilder CreateStringBuilder()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("//----------------------------------------------------------------------------");
        stringBuilder.AppendLine("//此代码由工具直接生成，非必要请不要修改此处代码");
        stringBuilder.AppendLine($"//生成工具：{assembly.GetName().Name} 版本：{assembly.GetName().Version}");
        stringBuilder.AppendLine($"//作者：若汝棋茗");
        stringBuilder.AppendLine($"//网址：https://touchsocket.net/");
        stringBuilder.AppendLine("//----------------------------------------------------------------------------");
        stringBuilder.AppendLine("#pragma warning disable");
        stringBuilder.AppendLine();

        return stringBuilder;
    }
    public static string GetRealTypeString(IParameterSymbol parameterSymbol)
    {
        switch (parameterSymbol.RefKind)
        {
            case RefKind.Ref:
                return parameterSymbol.Type.ToDisplayString().Replace("ref", string.Empty);

            case RefKind.Out:
                return parameterSymbol.Type.ToDisplayString().Replace("out", string.Empty);

            case RefKind.None:
            case RefKind.In:
            default:
                return parameterSymbol.Type.ToDisplayString();
        }
    }
    public static bool IsFromServices(IParameterSymbol parameter)
    {
        return parameter.HasAttribute(FromServicesAttributeTypeName, out _);
        //if (namedArguments.TryGetValue("MethodFlags", out var typedConstant))
        //{
        //    return typedConstant.Value is int value && this.HasFlags(value, 2);
        //}
        //else if (this.m_rpcApiNamedArguments.TryGetValue("MethodFlags", out typedConstant))
        //{
        //    return typedConstant.Value is int value && this.HasFlags(value, 2);
        //}
        //return false;
    }


    public static bool IsCallContext(IParameterSymbol parameter)
    {
        return parameter.Type.IsInheritFrom(ICallContextTypeName);
        //if (namedArguments.TryGetValue("MethodFlags", out var typedConstant))
        //{
        //    return typedConstant.Value is int value && this.HasFlags(value, 2);
        //}
        //else if (this.m_rpcApiNamedArguments.TryGetValue("MethodFlags", out typedConstant))
        //{
        //    return typedConstant.Value is int value && this.HasFlags(value, 2);
        //}
        //return false;
    }

    /// <summary>
    /// 是否为Rpc接口
    /// </summary>
    /// <param name="interface"></param>
    /// <returns></returns>
    public static bool IsRpcApiInterface(INamedTypeSymbol @interface)
    {
        //if (GeneratorRpcProxyAttribute is null)
        //{
        //    return false;
        //}
        //Debugger.Launch();
        return @interface.GetAttributes().FirstOrDefault(a =>
        {
            if (a.AttributeClass.ToDisplayString() != GeneratorRpcProxyAttributeTypeName)
            {
                return false;
            }
            //var s = GeneratorRpcProxyAttribute.ContainingAssembly.Name;
            //if (s != "TouchSocketPro")
            //{
            //    return false;
            //}
            //string key =  BitConverter.ToString(generatorRpcProxyAttribute.ContainingAssembly.Identity.PublicKeyToken.ToArray()).Replace("-", "")
            //.ToLower();
            //if (key != "efaad12a6cf1b696")
            //{
            //    return false;
            //}

            return true;
        }) is not null;
    }

    public static INamedTypeSymbol GetGeneratorRpcProxyAttribute(Compilation compilation)
    {
        return compilation.GetTypeByMetadataName(GeneratorRpcProxyAttributeTypeName);
    }

    public static AttributeData GetRpcAttribute(IMethodSymbol method, string rpcAttName)
    {
        foreach (var item in method.GetAttributes())
        {
            if (item.AttributeClass.IsInheritFrom(rpcAttName))
            {
                return item;
            }
        }

        return default;
    }
    public static AttributeData GetRpcAttribute(IMethodSymbol method)
    {
        return GetRpcAttribute(method, RpcAttributeTypeName);
    }
}