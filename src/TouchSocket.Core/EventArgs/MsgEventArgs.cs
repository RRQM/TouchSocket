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

namespace TouchSocket.Core;


/// <summary>
/// 消息事件参数类，继承自PluginEventArgs。
/// 该类用于封装消息相关的数据，在事件处理过程中传递。
/// </summary>
public class MsgEventArgs : PluginEventArgs
{
    /// <summary>
    /// 初始化MsgEventArgs类的新实例，指定消息内容。
    /// </summary>
    /// <param name="mes">要传递的消息字符串。</param>
    public MsgEventArgs(string mes)
    {
        this.Message = mes;
    }

    /// <summary>
    /// 初始化MsgEventArgs类的空实例。
    /// </summary>
    public MsgEventArgs()
    {
    }

    /// <summary>
    /// 获取或设置消息文本。
    /// 该属性用于存储和检索事件期间的消息内容。
    /// </summary>
    public string Message { get; set; }
}