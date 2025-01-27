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
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 文件日志记录器
/// <para>会在指定目录下，生成logs文件夹，然后按[yyyy-MM-dd].log的形式，每日生成日志</para>
/// </summary>
public sealed class FileLogger : LoggerBase, IDisposable
{
    private const int MaxRetryCount = 10;
    private readonly ConcurrentDictionary<string, FileStorageWriter> m_writers = new ConcurrentDictionary<string, FileStorageWriter>();
    private Func<LogLevel, string> m_createLogFolder;
    private bool m_disposedValue;
    private int m_retryCount;

    /// <summary>
    /// 初始化文件日志记录器
    /// </summary>
    /// <remarks>
    /// 在类的构造函数中定义了一个lambda表达式，用于创建日志文件夹的路径
    /// </remarks>
    public FileLogger()
    {
        // 定义一个lambda表达式，根据当前时间生成日志文件夹的路径
        // 这里的lambda表达式接收一个日志级别参数，但实际上并不使用它，只是为了符合委托的定义
        // 表达式的结果是根据当前日期格式化后的字符串，确保每天的日志被打包在不同的文件夹中
        this.m_createLogFolder = (logLevel) =>
        {
            return Path.Combine("logs", DateTime.Now.ToString("[yyyy-MM-dd]"));
        };
    }

    /// <summary>
    /// 析构函数，用于在对象被垃圾回收时进行资源清理
    /// </summary>
    ~FileLogger()
    {
        // 析构函数，用于在对象被垃圾回收时进行资源清理
        // 将 disposing 参数设为 false，表示仅释放非托管资源
        // 此处的清理工作已经委托给 Dispose 方法处理
        this.Dispose(disposing: false);
    }

    /// <summary>
    /// 获取或设置一个函数，用于根据日志级别创建日志文件夹。
    /// </summary>
    /// <value>
    /// 当前用于根据日志级别创建日志文件夹的函数。
    /// </value>
    /// <exception cref="System.ArgumentNullException">
    /// 如果尝试设置的值为 null，则抛出此异常。
    /// </exception>
    public Func<LogLevel, string> CreateLogFolder
    {
        get => this.m_createLogFolder;
        set => this.m_createLogFolder = ThrowHelper.ThrowArgumentNullExceptionIf(value, nameof(this.CreateLogFolder));
    }

    /// <summary>
    /// 文件名格式化字符串。默认为“0000”，当第1个文件，即为0001.log，第2个文件为0002.log，以此类推。
    /// </summary>
    public string FileNameFormat { get; set; } = "0000";

    /// <summary>
    /// 最大日志尺寸
    /// </summary>
    public int MaxSize { get; set; } = 1024 * 1024;

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        var loggerString = this.CreateLogString(logLevel, source, message, exception);

        this.WriteLogStringToFile(loggerString, logLevel);
    }

    private void Dispose(bool disposing)
    {
        if (!this.m_disposedValue)
        {
            if (disposing)
            {
                foreach (var item in this.m_writers)
                {
                    item.Value.SafeDispose();
                }

                this.m_writers.Clear();
            }

            this.m_disposedValue = true;
        }
    }

    private FileStorageWriter GetFileStorageWriter(string dirPath)
    {
        if (this.m_writers.TryGetValue(dirPath, out var writer))
        {
            return writer;
        }
        else
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var count = 0;
            while (true)
            {
                var filePath = Path.Combine(dirPath, $"{count.ToString(this.FileNameFormat)}" + ".log");
                if (!(File.Exists(filePath) && new FileInfo(filePath).Length > this.MaxSize))
                {
                    writer = FilePool.GetWriter(filePath);
                    writer.SeekToEnd();
                    writer.FileStorage.AccessTimeout = TimeSpan.MaxValue;

                    if (this.m_writers.TryAdd(dirPath, writer))
                    {
                        return writer;
                    }
                    else
                    {
                        return this.GetFileStorageWriter(dirPath);
                    }
                }
                count++;
            }
        }
    }

    private void WriteLogStringToFile(string logString, LogLevel logLevel)
    {
        var dirPath = this.CreateLogFolder(logLevel);

        var writer = this.GetFileStorageWriter(dirPath);

        lock (writer)
        {
            try
            {
                writer.Write(Encoding.UTF8.GetBytes(logString));
                writer.FileStorage.Flush();
                if (writer.FileStorage.Length > this.MaxSize)
                {
                    if (this.m_writers.TryRemove(dirPath, out var fileStorageWriter))
                    {
                        fileStorageWriter.SafeDispose();
                    }
                }

                this.m_retryCount = 0;
            }
            catch
            {
                if (this.m_writers.TryRemove(dirPath, out var fileStorageWriter))
                {
                    fileStorageWriter.SafeDispose();
                }

                if (++this.m_retryCount < MaxRetryCount)
                {
                    this.WriteLogStringToFile(logString, logLevel);
                }
            }
        }
    }
}