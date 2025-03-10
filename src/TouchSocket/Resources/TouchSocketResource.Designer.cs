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

namespace TouchSocket.Resources {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class TouchSocketResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TouchSocketResource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TouchSocket.Resources.TouchSocketResource", typeof(TouchSocketResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 当前适配器为空或者不支持对象（IRequestInfo）发送。 的本地化字符串。
        /// </summary>
        public static string CannotSendRequestInfo {
            get {
                return ResourceManager.GetString("CannotSendRequestInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 客户端没有连接。 的本地化字符串。
        /// </summary>
        public static string ClientNotConnected {
            get {
                return ResourceManager.GetString("ClientNotConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 没有找到Id为‘{0}’的客户端。 的本地化字符串。
        /// </summary>
        public static string ClientNotFind {
            get {
                return ResourceManager.GetString("ClientNotFind", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 已连接的客户端数量已达到设定（{0}）最大值。 的本地化字符串。
        /// </summary>
        public static string ConnectedMaximum {
            get {
                return ResourceManager.GetString("ConnectedMaximum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 调用Dispose主动断开。 的本地化字符串。
        /// </summary>
        public static string DisposeClose {
            get {
                return ResourceManager.GetString("DisposeClose", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Id为‘{0}’的客户端已存在。 的本地化字符串。
        /// </summary>
        public static string IdAlreadyExists {
            get {
                return ResourceManager.GetString("IdAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 发送数据不完全。 的本地化字符串。
        /// </summary>
        public static string IncompleteDataTransmission {
            get {
                return ResourceManager.GetString("IncompleteDataTransmission", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 远程终端主动断开。 的本地化字符串。
        /// </summary>
        public static string RemoteDisconnects {
            get {
                return ResourceManager.GetString("RemoteDisconnects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 新的Socket必须在连接状态。 的本地化字符串。
        /// </summary>
        public static string SocketHaveToConnected {
            get {
                return ResourceManager.GetString("SocketHaveToConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 超时无数据交互，主动断开连接。 的本地化字符串。
        /// </summary>
        public static string TimedoutWithoutAll {
            get {
                return ResourceManager.GetString("TimedoutWithoutAll", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 超时无数据Receive交互，主动断开连接。 的本地化字符串。
        /// </summary>
        public static string TimedoutWithoutReceiving {
            get {
                return ResourceManager.GetString("TimedoutWithoutReceiving", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 超时无数据Send交互，主动断开连接。 的本地化字符串。
        /// </summary>
        public static string TimedoutWithoutSending {
            get {
                return ResourceManager.GetString("TimedoutWithoutSending", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Udp不在运行状态。 的本地化字符串。
        /// </summary>
        public static string UdpStopped {
            get {
                return ResourceManager.GetString("UdpStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 无法获取终结点。 的本地化字符串。
        /// </summary>
        public static string UnableToObtainEndpoint {
            get {
                return ResourceManager.GetString("UnableToObtainEndpoint", resourceCulture);
            }
        }
    }
}