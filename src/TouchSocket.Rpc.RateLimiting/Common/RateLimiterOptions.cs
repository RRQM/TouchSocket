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

using System.Collections.Generic;

namespace TouchSocket.Rpc.RateLimiting;


/// <summary>
/// 限速器配置类
/// </summary>
public class RateLimiterOptions
{
    /// <summary>
    /// 限速策略字典，用于存储不同的限速策略
    /// </summary>
    public Dictionary<string, IRateLimiterPolicy> RateLimiterPolicies { get; private set; } = new Dictionary<string, IRateLimiterPolicy>();

    /// <summary>
    /// 添加限速策略方法
    /// </summary>
    /// <param name="policyName">限速策略名称，用作策略的唯一标识</param>
    /// <param name="rateLimiterPolicy">具体的限速策略实例</param>
    /// <returns>当前的RateLimiterOptions实例，支持链式调用</returns>
    public RateLimiterOptions AddPolicy(string policyName, IRateLimiterPolicy rateLimiterPolicy)
    {
        this.RateLimiterPolicies.Add(policyName, rateLimiterPolicy);
        return this;
    }
}