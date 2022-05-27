using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Log
{
    /// <summary>
    /// 快捷日志
    /// </summary>
    public class EasyLogger : ILog
    {
        private readonly Action<LogType, object, string, Exception> m_action;
        private readonly Action<string> m_action1;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">参数依次为：日志类型，触发源，消息，异常</param>
        public EasyLogger(Action<LogType,object,string,Exception> action)
        {
            this.m_action = action;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="action">参数为日志消息输出。</param>
        public EasyLogger(Action<string> action)
        {
            this.m_action1 = action;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        public virtual void Debug(LogType logType, object source, string message)
        {
            this.Debug(logType, source, message, null);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public virtual void Debug(LogType logType, object source, string message, Exception exception)
        {
            try
            {
                if (m_action!=null)
                {
                     m_action.Invoke(logType, source, message, exception);
                    return;
                }
                if (m_action1 != null)
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
                    m_action1.Invoke(stringBuilder.ToString());
                    return;
                }
            }
            catch
            {

            }
        }
    }
}
