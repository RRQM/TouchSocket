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
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// IDmtpRpcRequestPackage接口定义了远程过程调用请求包的结构和行为。
    /// 它继承自IReadonlyRouterPackage，提供额外的属性和方法来支持远程过程调用机制。
    /// </summary>
    public interface IDmtpRpcRequestPackage : IReadonlyRouterPackage
    {
        /// <summary>
        /// 获取序列化类型。
        /// 序列化类型指示了用于序列化包内容的方法或格式。
        /// </summary>
        /// <value>序列化类型</value>
        SerializationType SerializationType { get; }

        /// <summary>
        /// 获取元数据。
        /// 元数据提供了关于包的附加信息，如发送者、接收者等。
        /// </summary>
        /// <value>元数据对象</value>
        Metadata Metadata { get; }

        /// <summary>
        /// 获取反馈类型。
        /// 反馈类型指示了调用方期望的反馈方式，如无反馈、单向反馈等。
        /// </summary>
        /// <value>反馈类型</value>
        FeedbackType Feedback { get; }

        /// <summary>
        /// 获取调用键。
        /// 调用键是用于标识和跟踪特定远程过程调用的唯一标识符。
        /// </summary>
        /// <value>调用键字符串</value>
        string InvokeKey { get; }
    }
}
