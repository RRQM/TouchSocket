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

namespace TouchSocket.Core
{

    /// <summary>
    /// MsgPermitEventArgs 类继承自 PermitEventArgs 类，用于封装消息许可的事件参数
    /// </summary>
    public class MsgPermitEventArgs : PermitEventArgs
    {
        /// <summary>
        /// 初始化 MsgPermitEventArgs 类的新实例，包含消息内容
        /// </summary>
        /// <param name="mes">要处理的消息</param>
        public MsgPermitEventArgs(string mes)
        {
            this.Message = mes;
        }

        /// <summary>
        /// 初始化 MsgPermitEventArgs 类的新实例，不带初始消息内容
        /// </summary>
        public MsgPermitEventArgs()
        {
        }

        /// <summary>
        /// 获取或设置此许可事件关联的消息
        /// </summary>
        public string Message { get; set; }
    }
}