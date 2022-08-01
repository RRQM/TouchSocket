//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

namespace TouchSocket.Core.Log
{
    /// <summary>
    /// 文件日志记录器
    /// <para>会在指定目录下，生成logs文件夹，然后按[yyyy-MM-dd].log的形式，每日生成日志</para>
    /// </summary>
    public class FileLogger : ILog
    {
        private readonly string rootPath;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootPath">日志根目录</param>
        public FileLogger(string rootPath = null)
        {
            this.rootPath = Path.Combine(rootPath == null ? AppDomain.CurrentDomain.BaseDirectory : rootPath, "logs");
            if (!Directory.Exists(this.rootPath))
            {
                Directory.CreateDirectory(this.rootPath);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public void Debug(LogType logType, object source, string message)
        {
            this.Debug(logType, source, message, null);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void Debug(LogType logType, object source, string message, Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
            stringBuilder.Append(" | ");
            stringBuilder.Append(logType.ToString());
            stringBuilder.Append(" | ");
            stringBuilder.Append(message);

            if (exception != null)
            {
                stringBuilder.Append(" | ");
                stringBuilder.Append($"【堆栈】：{(exception == null ? "未知" : exception.StackTrace)}");
            }
            stringBuilder.AppendLine();

            this.Print(stringBuilder.ToString());
        }

        private int m_day = -1;
        private int m_count;
        private void Print(string logString)
        {
            try
            {
                lock (typeof(FileLogger))
                {
                    if (this.m_day != DateTime.Now.DayOfYear)
                    {
                        this.m_day = DateTime.Now.DayOfYear;
                        this.m_count = 0;
                    }
                    else
                    {
                        if (new FileInfo(Path.Combine(this.rootPath, $"{DateTime.Now:[yyyy-MM-dd]}-{this.m_count:00}" + ".log")).Length > 1024 * 1024)
                        {
                            this.m_count++;
                        }
                    }
                    string path = Path.Combine(this.rootPath, $"{DateTime.Now:[yyyy-MM-dd]}-{this.m_count:00}" + ".log");
                    File.AppendAllText(path, logString);
                }
            }
            catch
            {
            }
        }
    }
}