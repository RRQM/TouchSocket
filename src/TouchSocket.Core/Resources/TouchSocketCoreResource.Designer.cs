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
    public class TouchSocketCoreResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TouchSocketCoreResource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TouchSocket.Core.Resources.TouchSocketCoreResource", typeof(TouchSocketCoreResource).Assembly);
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
        ///   查找类似 此适配器已被其他终端使用，请重新创建对象。 的本地化字符串。
        /// </summary>
        public static string AdapterAlreadyUsed {
            get {
                return ResourceManager.GetString("AdapterAlreadyUsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’为空。 的本地化字符串。
        /// </summary>
        public static string ArgumentIsNull {
            get {
                return ResourceManager.GetString("ArgumentIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 当前读取器不允许读取。 的本地化字符串。
        /// </summary>
        public static string BlockReaderNotAllowReading {
            get {
                return ResourceManager.GetString("BlockReaderNotAllowReading", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 配置文件为空。 的本地化字符串。
        /// </summary>
        public static string ConfigIsNull {
            get {
                return ResourceManager.GetString("ConfigIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 默认情况。 的本地化字符串。
        /// </summary>
        public static string Default {
            get {
                return ResourceManager.GetString("Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 发生异常，信息：‘{0}’。 的本地化字符串。
        /// </summary>
        public static string ExceptionOccurred {
            get {
                return ResourceManager.GetString("ExceptionOccurred", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 文件‘{0}’不存在。 的本地化字符串。
        /// </summary>
        public static string FileNotExists {
            get {
                return ResourceManager.GetString("FileNotExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 该路径’{0}‘的文件已经被加载为仅写入模式。 的本地化字符串。
        /// </summary>
        public static string FileOnlyWrittenTo {
            get {
                return ResourceManager.GetString("FileOnlyWrittenTo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 该路径’{0}‘的文件已经被加载为仅读取模式。 的本地化字符串。
        /// </summary>
        public static string FileReadOnly {
            get {
                return ResourceManager.GetString("FileReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 对于枚举类型：‘{0}’，设置了无效的枚举值‘{1}’。 的本地化字符串。
        /// </summary>
        public static string InvalidEnum {
            get {
                return ResourceManager.GetString("InvalidEnum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’的值是无效参数。 的本地化字符串。
        /// </summary>
        public static string InvalidParameter {
            get {
                return ResourceManager.GetString("InvalidParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 应为由数组支持的缓冲区。 的本地化字符串。
        /// </summary>
        public static string MemoryGetArrayFail {
            get {
                return ResourceManager.GetString("MemoryGetArrayFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Token消息为‘{0}’的未注册。 的本地化字符串。
        /// </summary>
        public static string MessageNotFound {
            get {
                return ResourceManager.GetString("MessageNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 没有找到类型‘{0}’的公共构造函数。 的本地化字符串。
        /// </summary>
        public static string NotFindPublicConstructor {
            get {
                return ResourceManager.GetString("NotFindPublicConstructor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 类型为’{0}‘，HashCode为’{1}‘的对象实例已被释放。 的本地化字符串。
        /// </summary>
        public static string ObjectDisposed {
            get {
                return ResourceManager.GetString("ObjectDisposed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 操作已被取消。 的本地化字符串。
        /// </summary>
        public static string OperationCanceled {
            get {
                return ResourceManager.GetString("OperationCanceled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 操作超时。 的本地化字符串。
        /// </summary>
        public static string OperationOvertime {
            get {
                return ResourceManager.GetString("OperationOvertime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 操作成功。 的本地化字符串。
        /// </summary>
        public static string OperationSuccessful {
            get {
                return ResourceManager.GetString("OperationSuccessful", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 类型：{0}，信息：{1} 的本地化字符串。
        /// </summary>
        public static string ResultToString {
            get {
                return ResourceManager.GetString("ResultToString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 没有找到路径‘{0}’对应的流文件。 的本地化字符串。
        /// </summary>
        public static string StreamNotFind {
            get {
                return ResourceManager.GetString("StreamNotFind", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 路径‘{0}’对应的流文件，仍然被‘{1}’对象应用。 的本地化字符串。
        /// </summary>
        public static string StreamReferencing {
            get {
                return ResourceManager.GetString("StreamReferencing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 无法将字符串’{0}‘转为类型’{1}‘。 的本地化字符串。
        /// </summary>
        public static string StringParseToTypeFail {
            get {
                return ResourceManager.GetString("StringParseToTypeFail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Token消息为‘{0}’的已注册。 的本地化字符串。
        /// </summary>
        public static string TokenExisted {
            get {
                return ResourceManager.GetString("TokenExisted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 未知错误。 的本地化字符串。
        /// </summary>
        public static string UnknownError {
            get {
                return ResourceManager.GetString("UnknownError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 无法创建未被注册的类型‘{0}’的实例。 的本地化字符串。
        /// </summary>
        public static string UnregisteredType {
            get {
                return ResourceManager.GetString("UnregisteredType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’的值为’{1}‘，超出‘{2}’到‘{3}’的范围。 的本地化字符串。
        /// </summary>
        public static string ValueBetweenAnd {
            get {
                return ResourceManager.GetString("ValueBetweenAnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’的值为’{1}‘，小于‘{2}’。 的本地化字符串。
        /// </summary>
        public static string ValueLessThan {
            get {
                return ResourceManager.GetString("ValueLessThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 参数‘{0}’的值为’{1}‘，大于‘{2}’。 的本地化字符串。
        /// </summary>
        public static string ValueMoreThan {
            get {
                return ResourceManager.GetString("ValueMoreThan", resourceCulture);
            }
        }
    }
}