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

namespace TouchSocket.Http;

/// <summary>
/// 跨域相关配置
/// </summary>
public class CorsOptions
{
    private readonly Dictionary<string, CorsPolicy> m_corsPolicys = new Dictionary<string, CorsPolicy>();

    /// <summary>
    /// 跨域策略集
    /// </summary>
    public Dictionary<string, CorsPolicy> CorsPolicys => this.m_corsPolicys;

    /// <summary>
    /// 添加跨域策略
    /// </summary>
    /// <param name="policyName"></param>
    /// <param name="corsBuilderAction"></param>
    public void Add(string policyName, Action<CorsBuilder> corsBuilderAction)
    {
        var corsBuilder = new CorsBuilder();
        corsBuilderAction.Invoke(corsBuilder);
        this.m_corsPolicys.Add(policyName, corsBuilder.Build());
    }

    /// <summary>
    /// 添加跨域策略
    /// </summary>
    /// <param name="policyName"></param>
    /// <param name="corsResult"></param>
    public void Add(string policyName, CorsPolicy corsResult)
    {
        this.m_corsPolicys.Add(policyName, corsResult);
    }
}