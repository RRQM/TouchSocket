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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道客户端扩展方法类
/// 提供命名管道客户端的便捷连接方法
/// </summary>
public static class NamedPipeClientExtension
{
    #region 连接

    /// <summary>
    /// 异步连接到指定的命名管道服务器
    /// </summary>
    /// <typeparam name="TClient">命名管道客户端类型，必须实现<see cref="INamedPipeClient"/>接口</typeparam>
    /// <param name="client">命名管道客户端实例</param>
    /// <param name="pipeName">要连接的命名管道名称</param>
    /// <param name="millisecondsTimeout">连接超时时间（毫秒），默认为 5000ms</param>
    /// <param name="token">用于取消操作的取消令牌</param>
    /// <returns>返回连接成功的客户端实例</returns>
    /// <exception cref="OperationCanceledException">当连接操作被取消或超时时抛出</exception>
    /// <exception cref="ArgumentNullException">当 pipeName 为空时抛出</exception>
    /// <exception cref="InvalidOperationException">当客户端状态不允许连接时抛出</exception>
    public static async Task<TClient> ConnectAsync<TClient>(this TClient client, string pipeName, int millisecondsTimeout = 5000, CancellationToken token = default) where TClient : INamedPipeClient
    {
        // 配置客户端连接参数
        TouchSocketConfig config;
        if (client.Config == null)
        {
            // 客户端未配置时，创建新的配置对象
            config = new TouchSocketConfig();
            config.SetPipeName(pipeName);
            await client.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            // 使用现有配置并设置管道名称
            config = client.Config;
            config.SetPipeName(pipeName);
        }

        // 创建超时控制器，结合用户提供的取消令牌
        using var timeoutTokenSource = new TimeoutTokenSource(millisecondsTimeout, token);

        try
        {
            // 执行异步连接操作
            await client.ConnectAsync(timeoutTokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch (OperationCanceledException ex)
        {
            // 处理取消异常，判断是超时还是用户主动取消
            timeoutTokenSource.HandleCancellation(ex);
        }
        catch
        {
            // 重新抛出其他异常
            throw;
        }

        // 返回已连接的客户端实例
        return client;
    }

    #endregion 连接
}