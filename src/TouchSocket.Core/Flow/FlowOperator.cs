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

namespace TouchSocket.Core;

/// <summary>
/// 关于具有流速的操作器。
/// </summary>
public abstract class FlowOperator
{
    /// <summary>
    /// 已完成长度
    /// </summary>
    protected long completedLength;

    /// <summary>
    /// 进度
    /// </summary>
    protected float m_progress;

    private readonly FlowGate m_flowGate = new FlowGate();

    private long m_speedTemp;

    /// <summary>
    /// 流量控制器。
    /// </summary>
    public FlowOperator()
    {
        this.MaxSpeed = long.MaxValue;
    }

    /// <summary>
    /// 已完成长度
    /// </summary>
    /// <returns></returns>
    public long CompletedLength => this.completedLength;

    /// <summary>
    /// 流量控制器。
    /// </summary>
    public FlowGate FlowGate => this.m_flowGate;

    /// <summary>
    /// 由<see cref="Result"/>的结果，判断是否已结束操作。
    /// </summary>
    public virtual bool IsEnd => this.Result.ResultCode != ResultCode.Default;

    /// <summary>
    /// 数据源的全部长度。
    /// </summary>
    public long Length { get; protected set; }

    /// <summary>
    /// 最大传输速度。
    /// </summary>
    public virtual long MaxSpeed { get => this.m_flowGate.Maximum; set => this.m_flowGate.Maximum = value; }

    /// <summary>
    /// 进度
    /// </summary>
    public float Progress => this.m_progress;

    /// <summary>
    /// 执行结果
    /// </summary>
    public Result Result { get; protected set; }

    /// <summary>
    /// 超时时间，默认10*1000ms。
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 可取消令箭
    /// </summary>
    public virtual CancellationToken Token { get; set; }

    /// <summary>
    /// 从上次获取到此次获得的速度
    /// </summary>
    /// <returns>返回本次计算得到的速度</returns>
    public long Speed()
    {
        // 将临时速度变量的值赋给速度变量，以反映本次计算得到的速度
        var speed = this.m_speedTemp;

        // 将临时速度变量重置为0，为下一次速度计算做准备
        this.m_speedTemp = 0;

        // 返回本次计算得到的速度
        return speed;
    }

    /// <summary>
    /// 添加流速(线程安全)
    /// </summary>
    /// <param name="flow">要添加的流速值</param>
    protected virtual void ProtectedAddFlow(int flow)
    {
        // 添加流量前，通过m_flowGate的检查和等待机制确保线程安全
        this.m_flowGate.AddCheckWait(flow);

        // 使用Interlocked类确保在多线程环境下对m_speedTemp的访问是原子操作
        Interlocked.Add(ref this.m_speedTemp, flow);

        if (this.Length > 0)
        {
            // 更新进度：计算已完成长度与总长度的比例，转换为浮点数表示完成度
            this.m_progress = (float)((double)Interlocked.Add(ref this.completedLength, flow) / this.Length);
        }
    }

    /// <summary>
    /// 异步安全地添加流速
    /// </summary>
    /// <param name="flow">要添加的流速值</param>
    /// <returns>一个等待完成的任务</returns>
    protected virtual async Task ProtectedAddFlowAsync(int flow)
    {
        // 使用流速门控机制，安全地添加并检查流速，确保线程安全
        await this.m_flowGate.AddCheckWaitAsync(flow).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        // 原子操作更新临时速度值，以确保线程安全
        Interlocked.Add(ref this.m_speedTemp, flow);

        if (this.Length > 0)
        {
            // 更新进度，计算已完成长度与总长度的比例
            this.m_progress = (float)((double)Interlocked.Add(ref this.completedLength, flow) / this.Length);
        }
    }
}