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

using System.ComponentModel;

namespace TouchSocket.Resources
{
    /// <summary>
    /// TouchSocket资源枚举
    /// </summary>
    public enum TouchSocketRes
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        [Description("未知错误")]
        UnknownError,

        /// <summary>
        /// 操作超时
        /// </summary>
        [Description("操作超时")]
        Overtime,

        /// <summary>
        /// 参数‘{0}’为空。
        /// </summary>
        [Description("参数‘{0}’为空。")]
        ArgumentNull,

        /// <summary>
        /// 远程终端拒绝该操作，反馈信息：{0}。
        /// </summary>
        [Description("远程终端拒绝该操作，反馈信息：{0}。")]
        RemoteRefuse,

        /// <summary>
        /// 远程终端不响应该操作。
        /// </summary>
        [Description("远程终端不支持响应该操作。")]
        RemoteNotSupported,

        /// <summary>
        /// 远程终端异常，信息：{0}。
        /// </summary>
        [Description("发生异常，信息：{0}。")]
        Exception,

        /// <summary>
        /// 通道设置失败。
        /// </summary>
        [Description("通道设置失败。")]
        SetChannelFail,

        /// <summary>
        /// 创建写入流失败
        /// </summary>
        [Description("从‘{0}’创建写入流失败，信息：{1}。")]
        CreateWriteStreamFail,

        /// <summary>
        /// 没有找到流文件
        /// </summary>
        [Description("没有找到路径‘{0}’对应的流文件。")]
        StreamNotFind,

        /// <summary>
        /// 没有找到客户端
        /// </summary>
        [Description("没有找到ID为{0}的客户端。")]
        ClientNotFind,

        /// <summary>
        /// 流文件正在被应用
        /// </summary>
        [Description("路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。")]
        StreamReferencing,

        /// <summary>
        /// 接收流容器为空
        /// </summary>
        [Description("接收流容器为空。")]
        StreamBucketNull,

        /// <summary>
        /// 从‘{0}’路径加载流异常，信息：‘{1}’。
        /// </summary>
        [Description("从‘{0}’路径加载流异常，信息：‘{1}’。")]
        LoadStreamFail,

        /// <summary>
        /// 事件操作器异常
        /// </summary>
        [Description("事件操作器异常。")]
        GetEventArgsFail,

        /// <summary>
        /// 长时间没有响应。
        /// </summary>
        [Description("长时间没有响应。")]
        NoResponse,

        /// <summary>
        /// 该Token消息已注册
        /// </summary>
        [Description("Token消息为‘{0}’的已注册。")]
        TokenExisted,

        /// <summary>
        /// 信息未找到
        /// </summary>
        [Description("Token消息为‘{0}’的未注册。")]
        MessageNotFound,

        /// <summary>
        /// 数据处理适配器为空
        /// </summary>
        [Description("数据处理适配器为空，可能客户端已掉线。")]
        NullDataAdapter,

        /// <summary>
        /// 名称为“{0}”的事件已存在
        /// </summary>
        [Description("名称为“{0}”的事件已存在。")]
        EventExisted,

        /// <summary>
        /// 名称为“{0}”的事件不存在
        /// </summary>
        [Description("名称为“{0}”的事件不存在。")]
        EventNotExist,

        /// <summary>
        /// 目录‘{0}’已存在。
        /// </summary>
        [Description("目录‘{0}’已存在。")]
        DirectoryExisted,

        /// <summary>
        /// 路径‘{0}’无效。
        /// </summary>
        [Description("路径‘{0}’无效。")]
        PathInvalid,

        /// <summary>
        /// 文件‘{0}’已存在。
        /// </summary>
        [Description("文件‘{0}’已存在。")]
        FileExisted,

        /// <summary>
        /// 文件‘{0}’不存在。
        /// </summary>
        [Description("文件‘{0}’不存在。")]
        FileNotExists,

        /// <summary>
        /// 目录‘{0}’不存在。
        /// </summary>
        [Description("目录‘{0}’不存在。")]
        DirectoryNotExists,

        /// <summary>
        /// 客户端没有连接
        /// </summary>
        [Description("客户端没有连接。")]
        NotConnected,

        /// <summary>
        /// 授权密钥无效
        /// </summary>
        [Description("授权密钥无效，程序将在5秒后退出。请检查密钥，或者不使用企业版功能。")]
        LicenceKeyInvalid
    }
}