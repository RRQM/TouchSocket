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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TouchSocket.Rpc;

namespace TouchSocket;

internal sealed class WebApiClientCodeBuilder : RpcClientCodeBuilder
{
    public const string FromBodyAttribute = "TouchSocket.WebApi.FromBodyAttribute";
    public const string FromFormAttribute = "TouchSocket.WebApi.FromFormAttribute";
    public const string FromHeaderAttribute = "TouchSocket.WebApi.FromHeaderAttribute";
    public const string FromQueryAttribute = "TouchSocket.WebApi.FromQueryAttribute";
    public const string RouterAttribute = "TouchSocket.WebApi.RouterAttribute";
    public const string WebApiAttribute = "TouchSocket.WebApi.WebApiAttribute";

    public WebApiClientCodeBuilder(INamedTypeSymbol rpcApi) : base(rpcApi, WebApiAttribute)
    {
    }

    protected override string RpcAttributeName => "WebApi";

    public override IEnumerable<string> Usings
    {
        get
        {
            yield return "using System;";
            yield return "using System.Diagnostics;";
            yield return "using TouchSocket.Core;";
            yield return "using TouchSocket.Sockets;";
            yield return "using TouchSocket.Rpc;";
            yield return "using System.Threading.Tasks;";
            yield return "using TouchSocket.Http;";
            yield return "using TouchSocket.WebApi;";
            yield return "using System.Collections.Generic;";
        }
    }

    protected override string[] GetGenericConstraintTypes(IMethodSymbol method, Dictionary<string, TypedConstant> namedArguments)
    {
        var strings = new List<string>();
        strings.AddRange(base.GetGenericConstraintTypes(method, namedArguments));

        strings.Add("TouchSocket.WebApi.IWebApiClientBase");
        return strings.ToArray();
    }

    public static IEnumerable<AttributeData> GetRouterAttributes(ISymbol symbol)
    {
        return symbol.GetAttributes().Where(a => a.AttributeClass.EqualsWithFullName(RouterAttribute));
    }

    protected override string BuildMethod(IMethodSymbol method)
    {
        //Debugger.Launch();
        var attributeData = RpcUtils.GetRpcAttribute(method, WebApiAttribute);
        if (attributeData is null)
        {
            return string.Empty;
        }

        //Debugger.Launch();

        var namedArguments = attributeData.NamedArguments.ToDictionary(a => a.Key, a => a.Value);

        var invokeKey = this.GetRouteUrls(method, attributeData, namedArguments).First();
        var methodName = this.GetMethodName(method, namedArguments);
        var genericConstraintTypes = this.GetGenericConstraintTypes(method, namedArguments);
        //var isIncludeCallContext = this.IsIncludeCallContext(method);
        var allowSync = this.AllowSync(GeneratorFlag.ExtensionSync, method, namedArguments);
        var allowAsync = this.AllowAsync(GeneratorFlag.ExtensionAsync, method, namedArguments);
        var returnType = this.GetReturnType(method);
        var taskType = this.GetTaskType(method);
        var webApiMethod = this.GetMethodType(namedArguments);

        var parameters = method.Parameters.Where(a => (!RpcUtils.IsCallContext(a)) && (!RpcUtils.IsFromServices(a))).ToImmutableArray();

        //生成开始
        var codeBuilder = new StringBuilder();

        if (allowSync)
        {
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");
            codeBuilder.Append($"public static {returnType} {methodName}");
            codeBuilder.Append("<TClient>(");//方法参数

            codeBuilder.Append($"this TClient client");

            codeBuilder.Append(",");
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

            codeBuilder.AppendLine("{");//方法开始

            var webApiParameterInfos = parameters.Select(p => new WebApiParameterInfo(p));

            codeBuilder.AppendLine("var _request=new WebApiRequest();");
            codeBuilder.AppendLine($"_request.Method = (HttpMethodType){webApiMethod};");
            codeBuilder.AppendLine($"_request.Headers = {this.GetFromHeaderString(method, webApiParameterInfos)};");
            codeBuilder.AppendLine($"_request.Querys = {this.GetFromQueryString(webApiParameterInfos)};");
            codeBuilder.AppendLine($"_request.Forms = {this.GetFromFormString(webApiParameterInfos)};");

            string bodyName = default;

            //Debugger.Launch();

            foreach (var webApiParameterInfo in webApiParameterInfos)
            {
                if (webApiParameterInfo.Parameter.Type.IsPrimitiveAndString())
                {
                    //Debugger.Launch();
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
                codeBuilder.AppendLine($"_request.Body = {bodyName};");
            }

            if (method.HasReturn())
            {
                codeBuilder.AppendLine($"return ({returnType}) client.Invoke(\"{invokeKey}\",typeof({returnType}),invokeOption,_request);");
            }
            else
            {
                codeBuilder.AppendLine($"client.Invoke(\"{invokeKey}\",default,invokeOption,_request);");
            }
            codeBuilder.AppendLine("}");
        }

        if (allowAsync)
        {
            //以下生成异步
            codeBuilder.AppendLine("///<summary>");
            codeBuilder.AppendLine($"///{this.GetDescription(method)}");
            codeBuilder.AppendLine("///</summary>");
            if (method.HasReturn())
            {
                codeBuilder.Append($"public static async Task<{returnType}> {methodName}Async");
            }
            else
            {
                codeBuilder.Append($"public static Task {methodName}Async");
            }

            codeBuilder.Append("<TClient>(");//方法参数

            codeBuilder.Append($"this TClient client");

            codeBuilder.Append(",");
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    codeBuilder.Append(",");
                }
                codeBuilder.Append(this.GetMethodParameterString(parameters[i]));
            }
            if (parameters.Length > 0)
            {
                codeBuilder.Append(",");
            }
            codeBuilder.Append("InvokeOption invokeOption = default");
            codeBuilder.AppendLine($") where TClient:{string.Join(",", genericConstraintTypes)}");

            codeBuilder.AppendLine("{");//方法开始
            var webApiParameterInfos = parameters.Select(p => new WebApiParameterInfo(p));

            codeBuilder.AppendLine("var _request=new WebApiRequest();");
            codeBuilder.AppendLine($"_request.Method = (HttpMethodType){webApiMethod};");
            codeBuilder.AppendLine($"_request.Headers = {this.GetFromHeaderString(method, webApiParameterInfos)};");
            codeBuilder.AppendLine($"_request.Querys = {this.GetFromQueryString(webApiParameterInfos)};");
            codeBuilder.AppendLine($"_request.Forms = {this.GetFromFormString(webApiParameterInfos)};");

            string bodyName = default;

            foreach (var webApiParameterInfo in webApiParameterInfos)
            {
                if (webApiParameterInfo.Parameter.Type.IsPrimitiveAndString())
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
                codeBuilder.AppendLine($"_request.Body = {bodyName};");
            }

            if (method.HasReturn())
            {
                codeBuilder.AppendLine($"return ({returnType}) await client.InvokeAsync(\"{invokeKey}\",typeof({returnType}),invokeOption,_request);");
            }
            else
            {
                codeBuilder.AppendLine($"return client.InvokeAsync(\"{invokeKey}\",default,invokeOption,_request);");
            }
            codeBuilder.AppendLine("}");
        }

        return codeBuilder.ToString();
    }

    protected override string GetInheritedClassName(INamedTypeSymbol rpcApi)
    {
        return new WebApiClientCodeBuilder(rpcApi).GetClassName();
    }

    protected override string ReplacePattern(string key, TypedConstant typedConstant)
    {
        if (key == "Method")
        {
            var value = Convert.ToInt32(typedConstant.Value);

            return ((HttpMethodType)value).ToString();
        }
        return base.ReplacePattern(key, typedConstant);
    }

    private string GetFromFormString(IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        var parameterInfos = webApiParameterInfos.Where(a => a.IsFromForm);
        if (!parameterInfos.Any())
        {
            return "null";
        }

        var codeBuilder = new StringBuilder();
        codeBuilder.Append("new KeyValuePair<string, string>[] {");
        codeBuilder.Append(string.Join(",", parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromFormName}\",{this.GetParameterToString(a.Parameter)})")));
        codeBuilder.Append("}");

        return codeBuilder.ToString();
    }

    private string GetFromHeaderString(IMethodSymbol rpcMethod, IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        //Debugger.Launch();
        var parameterInfos = webApiParameterInfos.Where(a => a.IsFromHeader);
        var list = parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromHeaderName}\",{this.GetParameterToString(a.Parameter)})").ToList();

        if (rpcMethod.HasReturn() && this.GetReturnType(rpcMethod) == "string")
        {
            list.Add($"new KeyValuePair<string, string>(\"Accept\",\"text/plain\")");
        }

        if (list.Count > 0)
        {
            var codeBuilder = new StringBuilder();
            codeBuilder.Append("new KeyValuePair<string, string>[] {");
            codeBuilder.Append(string.Join(",", list));
            codeBuilder.Append("}");
            return codeBuilder.ToString();
        }
        return "null";
    }

    private string GetFromQueryString(IEnumerable<WebApiParameterInfo> webApiParameterInfos)
    {
        var parameterInfos = webApiParameterInfos.Where(a =>
        {
            if (a.Parameter.Type.IsPrimitiveAndString())
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

        var codeBuilder = new StringBuilder();
        codeBuilder.Append("new KeyValuePair<string, string>[] {");
        codeBuilder.Append(string.Join(",", parameterInfos.Select(a => $"new KeyValuePair<string, string>(\"{a.FromQueryName ?? a.Parameter.Name}\",{this.GetParameterToString(a.Parameter)})")));
        codeBuilder.Append("}");

        return codeBuilder.ToString();
    }

    private string GetMethodType(Dictionary<string, TypedConstant> namedArguments)
    {
        if (namedArguments.TryGetValue("Method", out var typedConstant))
        {
            return typedConstant.Value.ToString();
        }

        return "0";
    }

    private string GetParameterToString(IParameterSymbol parameter)
    {
        if (parameter.Type.IsValueType)
        {
            return $"{parameter.Name}.ToString()";
        }

        return $"{parameter.Name}?.ToString()";
    }

    private string GetRouteUrl(IMethodSymbol rpcMethod, AttributeData routerAttribute, Dictionary<string, TypedConstant> namedArguments)
    {
        var typedConstant = routerAttribute.ConstructorArguments.First();
        var routeTemple = typedConstant.Value.ToString();
        if (!routeTemple.StartsWith("/"))
        {
            routeTemple = routeTemple.Insert(0, "/");
        }

        var url = routeTemple.ToLower()
                        .Replace("[api]", rpcMethod.ContainingType.Name)
                        .Replace("[action]", this.GetMethodName(rpcMethod, namedArguments)).ToLower();

        return url;
    }

    private string[] GetRouteUrls(IMethodSymbol rpcMethod, AttributeData webApiAttribute, Dictionary<string, TypedConstant> namedArguments)
    {
        var urls = new List<string>();

        var attrs = GetRouterAttributes(rpcMethod);
        if (attrs.Any())
        {
            foreach (var item in attrs)
            {
                var url = this.GetRouteUrl(rpcMethod, item, namedArguments);

                if (!urls.Contains(url))
                {
                    urls.Add(url);
                }
            }
        }
        else
        {
            attrs = GetRouterAttributes(rpcMethod.ContainingType);
            if (attrs.Any())
            {
                foreach (var item in attrs)
                {
                    var url = this.GetRouteUrl(rpcMethod, item, namedArguments);
                    if (!urls.Contains(url))
                    {
                        urls.Add(url);
                    }
                }
            }
            else
            {
                urls.Add($"/{rpcMethod.ContainingType.Name}/{rpcMethod.Name}".ToLower());
            }
        }

        return urls.ToArray();
    }

    #region Class

    public enum HttpMethodType
    {
        Get = 0,
        Post = 1,
        Put = 2,
        Delete = 3
    }

    private class WebApiParameterInfo
    {
        public WebApiParameterInfo(IParameterSymbol parameter)
        {
            this.IsFromBody = parameter.HasAttribute(FromBodyAttribute);

            if (parameter.HasAttribute(FromQueryAttribute, out var fromQueryAttribute))
            {
                this.IsFromQuery = true;
                this.FromQueryName = GetName(fromQueryAttribute) ?? parameter.Name;
            }
            if (parameter.HasAttribute(FromFormAttribute, out var fromFormAttribute))
            {
                this.IsFromForm = true;
                this.FromFormName = GetName(fromFormAttribute) ?? parameter.Name;
            }

            if (parameter.HasAttribute(FromHeaderAttribute, out var fromHeaderAttribute))
            {
                this.IsFromHeader = true;
                this.FromHeaderName = GetName(fromHeaderAttribute) ?? parameter.Name;
            }

            this.Parameter = parameter;
        }

        public string FromFormName { get; }

        public string FromHeaderName { get; }

        public string FromQueryName { get; }

        public bool IsFromBody { get; }

        public bool IsFromForm { get; }

        public bool IsFromHeader { get; }

        public bool IsFromQuery { get; }

        public IParameterSymbol Parameter { get; }

        private static string GetName(AttributeData attributeData)
        {
            foreach (var item in attributeData.NamedArguments)
            {
                if (item.Key.Equals("Name"))
                {
                    return item.Value.Value.ToString();
                }
            }
            return default;
        }
    }

    #endregion Class
}