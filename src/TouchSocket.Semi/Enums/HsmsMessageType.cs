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

namespace TouchSocket.Semi;

/// <summary>
/// 定义 HSMS（SEMI E37）消息类型。
/// </summary>
public enum HsmsMessageType : byte
{
    /// <summary>
    /// 数据消息（SECS-II 数据）。
    /// </summary>
    DataMessage = 0,

    /// <summary>
    /// 选择请求（Select.req）。
    /// </summary>
    SelectRequest = 1,

    /// <summary>
    /// 选择响应（Select.rsp）。
    /// </summary>
    SelectResponse = 2,

    /// <summary>
    /// 取消选择请求（Deselect.req）。
    /// </summary>
    DeselectRequest = 3,

    /// <summary>
    /// 取消选择响应（Deselect.rsp）。
    /// </summary>
    DeselectResponse = 4,

    /// <summary>
    /// 链路测试请求（Linktest.req）。
    /// </summary>
    LinkTestRequest = 5,

    /// <summary>
    /// 链路测试响应（Linktest.rsp）。
    /// </summary>
    LinkTestResponse = 6,

    /// <summary>
    /// 拒绝请求（Reject.req）。
    /// </summary>
    RejectRequest = 7,

    /// <summary>
    /// 分离请求（Separate.req）。
    /// </summary>
    SeparateRequest = 9
}
