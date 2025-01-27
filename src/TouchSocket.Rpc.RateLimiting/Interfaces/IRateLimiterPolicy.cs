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

using System.Threading.RateLimiting;

namespace TouchSocket.Rpc.RateLimiting;


/// <summary>
/// 定义了一个接口，用于根据上下文信息获取限流器实例
/// </summary>
public interface IRateLimiterPolicy
{
    /// <summary>
    /// 根据给定的调用上下文获取相应的限流器实例
    /// </summary>
    /// <param name="callContext">调用上下文，包含关于请求的信息</param>
    /// <returns>返回一个限流器实例</returns>
    RateLimiter GetRateLimiter(ICallContext callContext);
}