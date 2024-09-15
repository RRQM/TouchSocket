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

using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// Dmtp消息事件参数类，继承自PluginEventArgs
    /// 用于封装Dmtp消息相关的事件数据
    /// </summary>
    public class DmtpMessageEventArgs : PluginEventArgs
    {
        /// <summary>
        /// 初始化DmtpMessageEventArgs对象
        /// </summary>
        /// <param name="message">Dmtp消息实例</param>
        /// 将传入的Dmtp消息对象存储在当前类的DmtpMessage属性中
        public DmtpMessageEventArgs(DmtpMessage message)
        {
            this.DmtpMessage = message;
        }

        /// <summary>
        /// Dmtp消息
        /// </summary>
        /// <value>当前事件相关的Dmtp消息对象</value>
        public DmtpMessage DmtpMessage { get; }
    }
}