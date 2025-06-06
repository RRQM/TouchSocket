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
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http;

/// <summary>
/// 可以配置跨域的插件
/// </summary>
public class CorsPlugin : PluginBase, IHttpPlugin
{
    private readonly ICorsService m_corsService;
    private readonly string m_policyName;

    /// <summary>
    /// 可以配置跨域的插件
    /// </summary>
    /// <param name="corsService"></param>
    /// <param name="policyName"></param>
    public CorsPlugin(ICorsService corsService, string policyName)
    {
        this.m_corsService = corsService;
        this.m_policyName = policyName;
    }

    /// <inheritdoc/>
    public async Task OnHttpRequest(IHttpSessionClient client, HttpContextEventArgs e)
    {
        var corsPolicy = this.m_corsService.GetPolicy(this.m_policyName) ?? throw new Exception($"没有找到名称为{this.m_policyName}的跨域策略。");

        corsPolicy.Apply(e.Context);

        await e.InvokeNext();
    }
}