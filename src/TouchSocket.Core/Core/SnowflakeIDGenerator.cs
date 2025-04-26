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

namespace TouchSocket.Core;

/// <summary>
/// 雪花Id生成器(该代码来自网络)
/// </summary>
public class SnowflakeIdGenerator
{
    private const int SequenceBits = 10;

    /// <summary>
    /// 一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
    /// </summary>
    private const long SequenceMask = -1L ^ -1L << SequenceBits;

    private const int TimestampLeftShift = SequenceBits + WorkerIdBits;

    private const int WorkerIdBits = 4;

    //计数器字节数，10个字节用来保存计数码
    private const int WorkerIdShift = SequenceBits;

    private static long s_sequence = 0L;

    //机器Id
    private static long s_workerId;

    private readonly long m_twepoch = 687888001020L;

    private long m_lastTimestamp = -1L;

    static SnowflakeIdGenerator()
    {
    }


    /// <summary>
    /// 初始化 SnowflakeIdGenerator 类的新实例。
    /// </summary>
    /// <param name="workerId">工作机器的唯一标识符，用于区分不同的工作机器。</param>
    /// <exception cref="Exception">如果 workerId 大于最大工作机器ID或小于0，则抛出异常。</exception>
    public SnowflakeIdGenerator(long workerId)
    {
        // 检查 workerId 是否在有效范围内，如果不在，则抛出异常
        if (workerId > MaxWorkerId || workerId < 0)
        {
            throw new Exception(string.Format("worker Id can't be greater than {0} or less than 0 ", MaxWorkerId));
        }

        // 设置工作机器ID
        s_workerId = workerId;
        // 设置时间戳基准值，这是Snowflake算法中用到的一个重要参数
        this.m_twepoch = DateTimeOffset.UtcNow.Ticks - 10000;
    }

    /// <summary>
    /// 最大机器Id
    /// </summary>
    public static long MaxWorkerId { get; private set; } = -1L ^ (-1L << WorkerIdBits); //最大机器Id

    //唯一时间，这是一个避免重复的随机量，自行设定不要大于当前时间戳
    //机器码字节数。4个字节用来保存机器码(定义为Long类型会出现，最大偏移64位，所以左移64位没有意义)

    //机器码数据左移位数，就是后面计数器占用的位数
    //时间戳左移动位数就是机器码和计数器总字节数
    /// <summary>
    /// 获取Id
    /// </summary>
    /// <returns></returns>
    public long NextId()
    {
        lock (this)
        {
            var timestamp = this.TimeGen();
            if (this.m_lastTimestamp == timestamp)
            { //同一微妙中生成Id
                s_sequence = (s_sequence + 1) & SequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                if (s_sequence == 0)
                {
                    //一微妙内产生的Id计数已达上限，等待下一微妙
                    timestamp = this.TillNextMillis(this.m_lastTimestamp);
                }
            }
            else
            { //不同微秒生成Id
                s_sequence = 0; //计数清0
            }
            if (timestamp < this.m_lastTimestamp)
            { //如果当前时间戳比上一次生成Id时时间戳还小，抛出异常，因为不能保证现在生成的Id之前没有生成过
                throw new Exception(string.Format("Clock moved backwards.  Refusing to generate id for {0} milliseconds",
                    this.m_lastTimestamp - timestamp));
            }
            this.m_lastTimestamp = timestamp; //把当前时间戳保存为最后生成Id的时间戳
            var nextId = (timestamp - this.m_twepoch << TimestampLeftShift) | s_workerId << WorkerIdShift | s_sequence;
            return nextId;
        }
    }

    /// <summary>
    /// 获取下一微秒时间戳
    /// </summary>
    /// <param name="lastTimestamp"></param>
    /// <returns></returns>
    private long TillNextMillis(long lastTimestamp)
    {
        var timestamp = this.TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = this.TimeGen();
        }
        return timestamp;
    }

    /// <summary>
    /// 生成当前时间戳
    /// </summary>
    /// <returns></returns>
    private long TimeGen()
    {
        return Environment.TickCount;
    }
}