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
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 日志基类
/// </summary>
public abstract class LoggerBase : ILog
{
    /// <inheritdoc/>
    public LogLevel LogLevel { get; set; } = LogLevel.Debug;

    /// <inheritdoc/>
    public void Log(LogLevel logLevel, object source, string message, Exception exception)
    {
        if (logLevel < this.LogLevel)
        {
            return;
        }
        this.WriteLog(logLevel, source, message, exception);
    }

    /// <summary>
    /// 筛选日志后输出
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    protected abstract void WriteLog(LogLevel logLevel, object source, string message, Exception exception);

    /// <summary>
    /// 获取或设置日期时间格式
    /// </summary>
    /// <remarks>
    /// 这个属性用于定义如何格式化或解析字符串中的日期和时间
    /// 默认格式为 "yyyy-MM-dd HH:mm:ss ffff"，其中 "yyyy" 代表年份，
    /// "MM" 代表月份，"dd" 代表日期，"HH" 代表小时，
    /// "mm" 代表分钟，"ss" 代表秒，"ffff" 代表毫秒
    /// </remarks>
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss ffff";


    /// <summary>
    /// 创建日志字符串。
    /// </summary>
    /// <param name="logLevel">日志级别。</param>
    /// <param name="source">日志来源对象。</param>
    /// <param name="message">日志信息内容。</param>
    /// <param name="exception">关联的异常对象。</param>
    /// <returns>格式化后的日志字符串。</returns>
    protected string CreateLogString(LogLevel logLevel, object source, string message, Exception exception)
    {
        // 使用StringBuilder高效拼接字符串
        var stringBuilder = new StringBuilder();

        // 添加当前时间到日志字符串中
        stringBuilder.Append(DateTime.Now.ToString(this.DateTimeFormat));
        stringBuilder.Append(" | ");

        // 添加日志级别到日志字符串中
        stringBuilder.Append(logLevel.ToString());
        stringBuilder.Append(" | ");

        // 添加日志信息内容到日志字符串中
        stringBuilder.Append(message);

        // 如果有异常信息，则添加到日志字符串中
        if (exception != null)
        {
            stringBuilder.Append(" | ");
            stringBuilder.Append($"[Exception Message]：{exception.Message}");
            stringBuilder.Append($"[Stack Trace]：{exception.StackTrace}");
        }

        // 添加换行符
        stringBuilder.AppendLine();

        // 返回格式化后的日志字符串
        return stringBuilder.ToString();
    }
}