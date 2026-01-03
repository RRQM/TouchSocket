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

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示一个Server-Sent Events（SSE）消息。
    /// </summary>
    public class SseMessage
    {
        /// <summary>
        /// 事件类型，如果为空则默认为"message"。
        /// </summary>
        public string EventType { get; internal set; }

        /// <summary>
        /// 事件数据。
        /// </summary>
        public string Data { get; internal set; }

        /// <summary>
        /// 事件ID，用于重连时指定Last-Event-ID。
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// 重连时间（毫秒），如果指定，客户端应使用该值作为重连间隔。
        /// </summary>
        public int? Retry { get; internal set; }

        /// <summary>
        /// 表示该消息是否为注释（以冒号开头的行）。
        /// </summary>
        public bool IsComment { get; internal set; }

        /// <summary>
        /// 获取事件类型，如果为空则返回"message"。
        /// </summary>
        public string GetEventTypeOrDefault() => string.IsNullOrEmpty(this.EventType) ? "message" : this.EventType;

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        public override string ToString()
        {
            return this.IsComment ? $"Comment: {this.Data}" : $"Event: {this.GetEventTypeOrDefault()}, Data: {this.Data}, Id: {this.Id}, Retry: {this.Retry}";
        }
    }
}