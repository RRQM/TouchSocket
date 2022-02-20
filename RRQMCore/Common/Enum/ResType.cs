//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace RRQMCore
{
    /// <summary>
    /// RRQM资源枚举
    /// </summary>
    public enum ResType
    {
        /// <summary>
        /// 未知知错误
        /// </summary>
        UnknownError,

        /// <summary>
        /// 参数为空
        /// </summary>
        ArgumentNull,

        /// <summary>
        /// 远程终端拒绝该操作
        /// </summary>
        RemoteRefuse,

        /// <summary>
        /// 远程终端不响应该操作
        /// </summary>
        RemoteNotSupported,

        /// <summary>
        /// 远程终端异常
        /// </summary>
        RemoteException,

        /// <summary>
        /// 通道设置失败
        /// </summary>
        SetChannelFail,

        /// <summary>
        /// 路径无效
        /// </summary>
        PathInvalid,

        /// <summary>
        /// 文件已存在
        /// </summary>
        FileExists,

        /// <summary>
        /// 远程文件不存在
        /// </summary>
        RemoteFileNotExists,

        /// <summary>
        /// 创建写入流失败
        /// </summary>
        CreateWriteStreamFail,

        /// <summary>
        /// 没有找到流文件
        /// </summary>
        StreamNotFind,

        /// <summary>
        /// 没有找到客户端
        /// </summary>
        ClientNotFind,

        /// <summary>
        /// 流文件正在被应用
        /// </summary>
        StreamReferencing,

        /// <summary>
        /// 接收流容器为空
        /// </summary>
        StreamBucketNull,

        /// <summary>
        /// 加载流异常。
        /// </summary>
        LoadStreamFail,

        /// <summary>
        /// 事件操作器异常
        /// </summary>
        GetEventArgsFail,

        /// <summary>
        /// 长时间没有响应。
        /// </summary>
        NoResponse,

        /// <summary>
        /// 该Token消息已注册
        /// </summary>
        TokenExist,

        /// <summary>
        /// 信息未找到
        /// </summary>
        MessageNotFound,

        /// <summary>
        /// 内存块已释放
        /// </summary>
        ByteBlockDisposed,

        /// <summary>
        /// 数据处理适配器为空
        /// </summary>
        NullDataAdapter,

        /// <summary>
        /// 操作超时
        /// </summary>
        Overtime,

        /// <summary>
        /// 名称为“{0}”的事件已存在
        /// </summary>
        EventExisted,

        /// <summary>
        /// 名称为“{0}”的事件不存在
        /// </summary>
        EventNotExist
    }
}