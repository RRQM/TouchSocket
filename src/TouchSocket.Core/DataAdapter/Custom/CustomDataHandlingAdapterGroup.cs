// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Core;

/// <summary>
/// 用户自定义数据处理适配器组，用于管理多个自定义数据处理适配器。
/// 该类可以组合多个适配器，并按顺序尝试解析请求信息。
/// </summary>
/// <typeparam name="TRequest">请求信息类型，必须实现 <see cref="IRequestInfo"/> 接口。</typeparam>
public class CustomDataHandlingAdapterGroup<TRequest> : CustomDataHandlingAdapter<TRequest>
    where TRequest : IRequestInfo
{
    private readonly List<CustomDataHandlingAdapterGroup<TRequest>> m_dataHandlingAdapterSlims;

    /// <summary>
    /// 初始化 <see cref="CustomDataHandlingAdapterGroup{TRequest}"/> 类的新实例。
    /// </summary>
    /// <param name="adapters">适配器数组，用于组合多个数据处理适配器。</param>
    public CustomDataHandlingAdapterGroup(params CustomDataHandlingAdapterGroup<TRequest>[] adapters)
    {
        this.m_dataHandlingAdapterSlims = new(adapters);
    }

    /// <inheritdoc/>
    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref TRequest request)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override bool ParseRequestCore<TReader>(ref TReader reader, out TRequest request)
    {
        foreach (var item in this.m_dataHandlingAdapterSlims)
        {
            if (item.TryParseRequest(ref reader, out request))
            {
                return true;
            }
        }
        request = default;
        return false;
    }
}