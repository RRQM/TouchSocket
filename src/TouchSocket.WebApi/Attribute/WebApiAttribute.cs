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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi;

/// <summary>
/// 该自定义属性用于标记 Web API 方法。
/// 继承自 <see cref="RpcAttribute"/>，用于实现远程过程调用的功能。
/// 通过该属性，可以更便捷地将方法暴露为 Web API 服务。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[DynamicMethod]
public sealed class WebApiAttribute : RpcAttribute
{
    /// <summary>
    /// 构造函数，用于初始化WebApiAttribute对象并设置HTTP方法类型。
    /// </summary>
    /// <param name="method">指定HTTP请求的方法类型，如GET、POST等。</param>
    [Obsolete("由于构造函数直接设置参数在源生成时效果不一致，所以取消该方式，如果想要设置参数，请使用属性直接设置，例如：MethodInvoke=true", true)]
    public WebApiAttribute(HttpMethodType method) : this()
    {
        // 设置HTTP方法类型
        this.Method = method;
    }

    /// <summary>
    /// 使用Get函数的WebApi特性
    /// </summary>
    public WebApiAttribute()
    {
        this.Namespaces.Add("using TouchSocket.Http;");
        this.Namespaces.Add("using TouchSocket.WebApi;");
    }

    /// <summary>
    /// 请求函数类型。
    /// </summary>
    public HttpMethodType Method { get; set; }

    /// <inheritdoc/>
    public override Type[] GetGenericConstraintTypes()
    {
        return new Type[] { typeof(IWebApiClientBase) };
    }

    /// <inheritdoc/>
    protected override string GetExtensionInstanceMethod(RpcMethod rpcMethod, List<string> parametersStr, RpcParameter[] parameters, bool isAsync)
    {
        if (rpcMethod.GetAttribute<WebApiAttribute>() is not WebApiAttribute webApiAttribute)
        {
            return string.Empty;
        }
        var webApiParameterInfos = parameters.Select(p => new WebApiParameterInfo(p));
        var returnTypeString = rpcMethod.HasReturn ? string.Format("typeof({0})", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)) : "null";

        var codeString = new StringBuilder();

        codeString.AppendLine("var _request=new WebApiRequest();");
        codeString.AppendLine($"_request.Method = HttpMethodType.{webApiAttribute.Method};");
        codeString.AppendLine($"_request.Headers = {this.GetFromHeaderString(rpcMethod, webApiParameterInfos)};");
        codeString.AppendLine($"_request.Querys = {this.GetFromQueryString(webApiParameterInfos)};");
        codeString.AppendLine($"_request.Forms = {this.GetFromFormString(webApiParameterInfos)};");

        var url = this.GetRouteUrls(rpcMethod).First();
        string bodyName = default;

        foreach (var webApiParameterInfo in webApiParameterInfos)
        {
            if (webApiParameterInfo.Parameter.Type.IsPrimitive())
            {
                if (webApiParameterInfo.IsFromBody)
                {
                    bodyName = webApiParameterInfo.Parameter.Name;
                }
            }
            else
            {
                bodyName = webApiParameterInfo.Parameter.Name;
            }
        }

        if (bodyName != null)
        {
            codeString.AppendLine($"_request.Body = {bodyName};");
        }

        if (isAsync)
        {
            if (rpcMethod.HasReturn)
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) await client.InvokeAsync(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) await client.InvokeAsync(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
            }
            else
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return client.InvokeAsync(\"{url}\", default, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return client.InvokeAsync(\"{url}\", default, invokeOption, _request);");
                }
            }
        }
        else
        {
            if (rpcMethod.HasReturn)
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) client.Invoke(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) client.Invoke(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
            }
            else
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"client.Invoke(\"{url}\", default, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"client.Invoke(\"{url}\", default, invokeOption, _request);");
                }
            }
        }

        return codeString.ToString();
    }

    private string GetParameterToString(RpcParameter parameter)
    {
        if (parameter.ParameterInfo.ParameterType.IsValueType)
        {
            return $"{parameter.Name}.ToString()";
        }

        return $"{parameter.Name}?.ToString()";
    }

    private string GetFromHeaderString(RpcMethod rpcMethod, IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        var parameterInfos = webApiParameterInfos.Where(a => a.IsFromHeader);
        var list = parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromHeaderName}\",{this.GetParameterToString(a.Parameter)})").ToList();

        if (rpcMethod.HasReturn && rpcMethod.ReturnType == typeof(string))
        {
            list.Add($"new KeyValuePair<string, string>(\"Accept\",\"text/plain\")");
        }

        if (list.Count > 0)
        {
            var codeString = new StringBuilder();
            codeString.Append("new KeyValuePair<string, string>[] {");
            codeString.Append(string.Join(",", list));
            codeString.Append("}");
            return codeString.ToString();
        }
        return "null";
    }

    private string GetFromFormString(IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        var parameterInfos = webApiParameterInfos.Where(a => a.IsFromForm);
        if (!parameterInfos.Any())
        {
            return "null";
        }

        var codeString = new StringBuilder();
        codeString.Append("new KeyValuePair<string, string>[] {");
        codeString.Append(string.Join(",", parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromFormName}\",{this.GetParameterToString(a.Parameter)})")));
        codeString.Append("}");

        return codeString.ToString();
    }

    private string GetFromQueryString(IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        var parameterInfos = webApiParameterInfos.Where(a =>
        {
            if (a.Parameter.Type.IsPrimitive())
            {
                if (a.IsFromBody)
                {
                    return false;
                }
                else if (a.IsFromHeader)
                {
                    return false;
                }
                else if (a.IsFromForm)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        });
        if (!parameterInfos.Any())
        {
            return "null";
        }

        var codeString = new StringBuilder();
        codeString.Append("new KeyValuePair<string, string>[] {");
        codeString.Append(string.Join(",", parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromQueryName ?? a.Parameter.Name}\",{this.GetParameterToString(a.Parameter)})")));
        codeString.Append("}");

        return codeString.ToString();
    }

    /// <inheritdoc/>
    protected override string GetInstanceMethod(RpcMethod rpcMethod, List<string> parametersStr, RpcParameter[] parameters, bool isAsync)
    {
        if (rpcMethod.GetAttribute<WebApiAttribute>() is not WebApiAttribute webApiAttribute)
        {
            return string.Empty;
        }
        var webApiParameterInfos = parameters.Select(p => new WebApiParameterInfo(p));
        var returnTypeString = rpcMethod.HasReturn ? string.Format("typeof({0})", this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)) : "null";

        var codeString = new StringBuilder();
        codeString.AppendLine("if(this.Client==null)");
        codeString.AppendLine("{");
        codeString.AppendLine("throw new RpcException(\"IRpcClient为空，请先初始化或者进行赋值\");");
        codeString.AppendLine("}");

        codeString.AppendLine("var _request=new WebApiRequest();");
        codeString.AppendLine($"_request.Method = HttpMethodType.{webApiAttribute.Method};");
        codeString.AppendLine($"_request.Headers = {this.GetFromHeaderString(rpcMethod, webApiParameterInfos)};");
        codeString.AppendLine($"_request.Querys = {this.GetFromQueryString(webApiParameterInfos)};");
        codeString.AppendLine($"_request.Forms = {this.GetFromFormString(webApiParameterInfos)};");

        var url = this.GetRouteUrls(rpcMethod).First();
        string bodyName = default;

        foreach (var webApiParameterInfo in webApiParameterInfos)
        {
            if (webApiParameterInfo.Parameter.Type.IsPrimitive())
            {
                if (webApiParameterInfo.IsFromBody)
                {
                    bodyName = webApiParameterInfo.Parameter.Name;
                }
            }
            else
            {
                bodyName = webApiParameterInfo.Parameter.Name;
            }
        }

        if (bodyName != null)
        {
            codeString.AppendLine($"_request.Body = {bodyName};");
        }

        if (isAsync)
        {
            if (rpcMethod.HasReturn)
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) await this.Client.InvokeAsync(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) await this.Client.InvokeAsync(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
            }
            else
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return this.Client.InvokeAsync(\"{url}\", default, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return this.Client.InvokeAsync(\"{url}\", default, invokeOption, _request);");
                }
            }
        }
        else
        {
            if (rpcMethod.HasReturn)
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) this.Client.Invoke(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"return ({this.GetProxyParameterName(rpcMethod.Info.ReturnParameter)}) this.Client.Invoke(\"{url}\", {returnTypeString}, invokeOption, _request);");
                }
            }
            else
            {
                if (bodyName == null)
                {
                    codeString.AppendLine($"this.Client.Invoke(\"{url}\", default, invokeOption, _request);");
                }
                else
                {
                    codeString.AppendLine($"this.Client.Invoke(\"{url}\", default, invokeOption, _request);");
                }
            }
        }

        return codeString.ToString();
    }

    /// <summary>
    /// 获取路由路径。
    /// <para>路由路径的第一个值会被当做调用值。</para>
    /// </summary>
    /// <param name="rpcMethod"></param>
    /// <returns></returns>
    public string[] GetRouteUrls(RpcMethod rpcMethod)
    {
        if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute webApiAttribute)
        {
            var urls = new List<string>();
            //如果方法有特性，则会覆盖类的特性
            var attrs = rpcMethod.Info.GetCustomAttributes(typeof(RouterAttribute), false);
            if (attrs.Length > 0)
            {
                foreach (var item in attrs.Cast<RouterAttribute>())
                {
                    var url = item.RouteTemple.ToLower()
                        .Replace("[api]", rpcMethod.ServerFromType.Name)
                        .Replace("[action]", webApiAttribute.GetMethodName(rpcMethod, false)).ToLower();

                    if (!urls.Contains(url))
                    {
                        urls.Add(url);
                    }
                }
            }
            else
            {
                attrs = rpcMethod.ServerFromType.GetCustomAttributes(typeof(RouterAttribute), false);
                if (attrs.Length > 0)
                {
                    foreach (var item in attrs.Cast<RouterAttribute>())
                    {
                        var url = item.RouteTemple.ToLower()
                            .Replace("[api]", rpcMethod.ServerFromType.Name)
                            .Replace("[action]", webApiAttribute.GetMethodName(rpcMethod, false)).ToLower();

                        if (!urls.Contains(url))
                        {
                            urls.Add(url);
                        }
                    }
                }
                else
                {
                    urls.Add($"/{rpcMethod.Info.DeclaringType.Name}/{rpcMethod.Name}".ToLower());
                }
            }

            return urls.ToArray();
        }
        return default;
    }

    public string[] GetRegexRouteUrls(RpcMethod rpcMethod)
    {
        if (rpcMethod.GetAttribute<WebApiAttribute>() is WebApiAttribute webApiAttribute)
        {
            var urls = new List<string>();
            //如果方法有特性，则会覆盖类的特性
            var attrs = rpcMethod.Info.GetCustomAttributes(typeof(RegexRouterAttribute), false);
            if (attrs.Length > 0)
            {
                foreach (var item in attrs.Cast<RegexRouterAttribute>())
                {
                    var url = item.RegexTemple.ToLower();

                    if (!urls.Contains(url))
                    {
                        urls.Add(url);
                    }
                }
            }
            else
            {
                attrs = rpcMethod.ServerFromType.GetCustomAttributes(typeof(RegexRouterAttribute), false);
                if (attrs.Length > 0)
                {
                    foreach (var item in attrs.Cast<RegexRouterAttribute>())
                    {
                        var url = item.RegexTemple.ToLower();

                        if (!urls.Contains(url))
                        {
                            urls.Add(url);
                        }
                    }
                }
            }

            return urls.ToArray();
        }
        return default;
    }
}