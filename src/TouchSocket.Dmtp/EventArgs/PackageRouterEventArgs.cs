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
    /// 包路由事件参数类，继承自MsgPermitEventArgs
    /// </summary>
    /// <remarks>
    /// 该类用于封装路由类型和路由数据包，以便进行路由操作
    /// </remarks>
    public class PackageRouterEventArgs : MsgPermitEventArgs
    {

        /// <summary>
        /// 构造函数，初始化路由类型和路由数据包
        /// </summary>
        /// <param name="routerType">路由类型</param>
        /// <param name="package">路由数据包</param>
        public PackageRouterEventArgs(RouteType routerType, IReadonlyRouterPackage package)
        {
            this.RouterType = routerType;
            this.Package = package;
        }

        /// <summary>
        /// 路由类型
        /// </summary>
        public RouteType RouterType { get; private set; }

        /// <summary>
        /// 路由数据包。一般为不完全数据，仅包含基本的路由信息。
        /// </summary>
        public IReadonlyRouterPackage Package { get; private set; }
    }
}