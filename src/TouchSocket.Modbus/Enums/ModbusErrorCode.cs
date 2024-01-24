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

using System.ComponentModel;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus错误码
    /// </summary>
    public enum ModbusErrorCode : byte
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 0,

        /// <summary>
        /// 功能码不能被从机识别
        /// </summary>
        [Description("功能码不能被从机识别")]
        FunctionCodeNotDefined = 1,

        /// <summary>
        /// 非法数据地址
        /// </summary>
        [Description("非法数据地址")]
        AddressInvalid = 2,

        /// <summary>
        /// 值不被从机接受
        /// </summary>
        [Description("值不被从机接受")]
        ValueInvalid = 3,

        /// <summary>
        /// 当从机试图执行请求的操作时，发生了不可恢复的错误
        /// </summary>
        [Description("当从机试图执行请求的操作时，发生了不可恢复的错误")]
        TaskError = 4,

        /// <summary>
        /// 从机已接受请求并正在处理，但需要很长时间。返回此响应是为了防止在主机中发生超时错误。主站可以在下一个轮询程序中发出一个完整的消息，以确定处理是否完成
        /// </summary>
        [Description("从机已接受请求并正在处理，但需要很长时间。返回此响应是为了防止在主机中发生超时错误。主站可以在下一个轮询程序中发出一个完整的消息，以确定处理是否完成")]
        HoldOn = 5,

        /// <summary>
        /// 从站正在处理长时间命令。Master应该稍后重试
        /// </summary>
        [Description("从站正在处理长时间命令。Master应该稍后重试")]
        Busy = 6,

        /// <summary>
        /// 从站不能执行程序功能。主站应该向从站请求诊断或错误信息。
        /// </summary>
        [Description("从站不能执行程序功能。主站应该向从站请求诊断或错误信息。")]
        ExecuteError = 7,

        /// <summary>
        /// 从站在内存中检测到奇偶校验错误。主设备可以重试请求，但从设备上可能需要服务。
        /// </summary>
        [Description("从站在内存中检测到奇偶校验错误。主设备可以重试请求，但从设备上可能需要服务。")]
        MemoryVerificationError = 8,

        /// <summary>
        /// 专门用于Modbus网关。表示配置错误的网关。
        /// </summary>
        [Description("专门用于Modbus网关。表示配置错误的网关。")]
        GatewayError = 10,

        /// <summary>
        /// 专用于Modbus网关的响应。当从站无法响应时发送
        /// </summary>
        [Description("专用于Modbus网关的响应。当从站无法响应时发送")]
        GatewayUnavailable = 11
    }
}