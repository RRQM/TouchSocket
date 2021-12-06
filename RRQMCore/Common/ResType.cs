using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore
{
    /// <summary>
    /// RRQMCore资源枚举
    /// </summary>
    public enum ResType
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        UnknownError,

        /// <summary>
        /// 已知错误
        /// </summary>
        KnownError,

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
        NotFindStream,

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

    }
}
