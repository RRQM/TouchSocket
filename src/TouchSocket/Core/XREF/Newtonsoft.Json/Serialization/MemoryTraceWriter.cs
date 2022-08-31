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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Represents a trace writer that writes to memory. When the trace message limit is
    /// reached then old trace messages will be removed as new messages are added.
    /// </summary>
    public class MemoryTraceWriter : ITraceWriter
    {
        private readonly Queue<string> _traceMessages;
        private readonly object _lock;

        /// <summary>
        /// Gets the <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
        /// For example a filter level of <see cref="TraceLevel.Info"/> will exclude <see cref="TraceLevel.Verbose"/> messages and include <see cref="TraceLevel.Info"/>,
        /// <see cref="TraceLevel.Warning"/> and <see cref="TraceLevel.Error"/> messages.
        /// </summary>
        /// <value>
        /// The <see cref="TraceLevel"/> that will be used to filter the trace messages passed to the writer.
        /// </value>
        public TraceLevel LevelFilter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTraceWriter"/> class.
        /// </summary>
        public MemoryTraceWriter()
        {
            this.LevelFilter = TraceLevel.Verbose;
            this._traceMessages = new Queue<string>();
            this._lock = new object();
        }

        /// <summary>
        /// Writes the specified trace level, message and optional exception.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel"/> at which to write this trace.</param>
        /// <param name="message">The trace message.</param>
        /// <param name="ex">The trace exception. This parameter is optional.</param>
        public void Trace(TraceLevel level, string message, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
            sb.Append(" ");
            sb.Append(level.ToString("g"));
            sb.Append(" ");
            sb.Append(message);

            string s = sb.ToString();

            lock (this._lock)
            {
                if (this._traceMessages.Count >= 1000)
                {
                    this._traceMessages.Dequeue();
                }

                this._traceMessages.Enqueue(s);
            }
        }

        /// <summary>
        /// Returns an enumeration of the most recent trace messages.
        /// </summary>
        /// <returns>An enumeration of the most recent trace messages.</returns>
        public IEnumerable<string> GetTraceMessages()
        {
            return this._traceMessages;
        }

        /// <summary>
        /// Returns a <see cref="String"/> of the most recent trace messages.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> of the most recent trace messages.
        /// </returns>
        public override string ToString()
        {
            lock (this._lock)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string traceMessage in this._traceMessages)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.Append(traceMessage);
                }

                return sb.ToString();
            }
        }
    }
}