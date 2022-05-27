using RRQMCore.ByteManager;
using RRQMCore.Dependency;
using RRQMCore.Log;
using System;

namespace RRQMSocket
{
    /// <summary>
    /// 终端接口
    /// </summary>
    public interface IClient : IRRQMDependencyObject, IDisposable
    {

        /// <summary>
        /// 处理未经过适配器的数据。返回值表示是否继续向下传递。
        /// </summary>
        Func<ByteBlock, bool> OnHandleRawBuffer { get; set; }

        /// <summary>
        /// 处理经过适配器后的数据。返回值表示是否继续向下传递。
        /// </summary>
        Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get; set; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; }

        /// <summary>
        /// 终端协议
        /// </summary>
        Protocol Protocol { get; set; }

        /// <summary>
        /// 简单IOC容器
        /// </summary>
        IContainer Container { get; }
    }
}
