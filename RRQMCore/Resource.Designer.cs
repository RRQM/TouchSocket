//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace RRQMCore {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RRQMCore.Resource", typeof(Resource).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’为空。 的本地化字符串。
        /// </summary>
        internal static string ArgumentNull {
            get {
                return ResourceManager.GetString("ArgumentNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 取消操作 的本地化字符串。
        /// </summary>
        internal static string Canceled {
            get {
                return ResourceManager.GetString("Canceled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 从‘{0}’创建写入流失败，信息：{1}。 的本地化字符串。
        /// </summary>
        internal static string CreateWriteStreamFail {
            get {
                return ResourceManager.GetString("CreateWriteStreamFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 默认设置值。 的本地化字符串。
        /// </summary>
        internal static string Default {
            get {
                return ResourceManager.GetString("Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 发生未知错误。 的本地化字符串。
        /// </summary>
        internal static string Error {
            get {
                return ResourceManager.GetString("Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 其他异常消息。 的本地化字符串。
        /// </summary>
        internal static string Exception {
            get {
                return ResourceManager.GetString("Exception", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 文件‘{0}’已存在。 的本地化字符串。
        /// </summary>
        internal static string FileExists {
            get {
                return ResourceManager.GetString("FileExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 事件操作器异常。 的本地化字符串。
        /// </summary>
        internal static string GetEventArgsFail {
            get {
                return ResourceManager.GetString("GetEventArgsFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 从‘{0}’路径加载流异常。 的本地化字符串。
        /// </summary>
        internal static string LoadStreamFail {
            get {
                return ResourceManager.GetString("LoadStreamFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 长时间没有响应。 的本地化字符串。
        /// </summary>
        internal static string NoResponse {
            get {
                return ResourceManager.GetString("NoResponse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 没有找到路径‘{0}’对应的流文件。 的本地化字符串。
        /// </summary>
        internal static string NotFindStream {
            get {
                return ResourceManager.GetString("NotFindStream", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 操作超时 的本地化字符串。
        /// </summary>
        internal static string Overtime {
            get {
                return ResourceManager.GetString("Overtime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’设置的路径‘{1}’不合法。 的本地化字符串。
        /// </summary>
        internal static string PathInvalid {
            get {
                return ResourceManager.GetString("PathInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 远程终端异常，信息：{0}。 的本地化字符串。
        /// </summary>
        internal static string RemoteException {
            get {
                return ResourceManager.GetString("RemoteException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 远程文件‘{0}’不存在。 的本地化字符串。
        /// </summary>
        internal static string RemoteFileNotExists {
            get {
                return ResourceManager.GetString("RemoteFileNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 远程终端不支持响应该操作。 的本地化字符串。
        /// </summary>
        internal static string RemoteNotSupported {
            get {
                return ResourceManager.GetString("RemoteNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 远程终端拒绝该操作，反馈信息：{0}。 的本地化字符串。
        /// </summary>
        internal static string RemoteRefuse {
            get {
                return ResourceManager.GetString("RemoteRefuse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 通道设置失败。 的本地化字符串。
        /// </summary>
        internal static string SetChannelFail {
            get {
                return ResourceManager.GetString("SetChannelFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 接收流容器为空。 的本地化字符串。
        /// </summary>
        internal static string StreamBucketNull {
            get {
                return ResourceManager.GetString("StreamBucketNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。 的本地化字符串。
        /// </summary>
        internal static string StreamReferencing {
            get {
                return ResourceManager.GetString("StreamReferencing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 操作成功。 的本地化字符串。
        /// </summary>
        internal static string Success {
            get {
                return ResourceManager.GetString("Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 未知原因。 的本地化字符串。
        /// </summary>
        internal static string Unknown {
            get {
                return ResourceManager.GetString("Unknown", resourceCulture);
            }
        }
    }
}
