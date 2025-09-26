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

using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi;

/// <summary>
/// 适用于WebApi的跨域特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
public sealed class EnableCorsAttribute : RpcActionFilterAttribute
{
    /// <summary>
    /// 适用于WebApi的跨域特性
    /// </summary>
    /// <param name="policyName">跨域策略名称</param>
    public EnableCorsAttribute(string policyName)
    {
        this.PolicyName = policyName;
    }

    /// <summary>
    /// 跨域策略名称
    /// </summary>
    public string PolicyName { get; set; }

    /// <inheritdoc/>
    public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
    {
        if (callContext is IHttpCallContext httpCallContext && httpCallContext.HttpContext != default)
        {
            var corsService = callContext.Resolver.Resolve<ICorsService>();
            var corsPolicy = corsService.GetPolicy(this.PolicyName) ?? throw new Exception($"没有找到名称为{this.PolicyName}的跨域策略。");

            corsPolicy.Apply(httpCallContext.HttpContext);
        }

        return base.ExecutedAsync(callContext, parameters, invokeResult, exception);
    }
}