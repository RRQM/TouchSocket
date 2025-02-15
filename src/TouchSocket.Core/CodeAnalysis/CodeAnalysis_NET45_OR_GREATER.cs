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

//#if NET45_OR_GREATER || NETSTANDARD2_0
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace System.Diagnostics.CodeAnalysis
//{
//    /// <summary>
//    /// 表示方法在返回特定值时不会返回 null。
//    /// </summary>
//    /// <remarks>
//    /// 该属性用于对方法的返回值进行说明，即在方法返回特定值时，其返回的对象不会是 null。
//    /// 这对于代码分析工具和编译器来说，是一个重要的元数据，可以帮助提高代码质量和安全性。
//    /// </remarks>
//    public sealed class NotNullWhenAttribute : Attribute
//    {
//        /// <summary>
//        /// 初始化 NotNullWhenAttribute 类的实例。
//        /// </summary>
//        /// <param name="returnValue">
//        /// 表示当方法返回此值时，方法的返回对象不会是 null。
//        /// </param>
//        public NotNullWhenAttribute(bool returnValue)
//        {
//        }
//    }
//    /// <summary>
//    /// 指示方法不会返回值的属性。
//    /// </summary>
//    /// <remarks>
//    /// 此属性用于指示方法在正常执行过程中不会返回控制权，例如，因为它会引发异常或执行无限循环。
//    /// </remarks>
//    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
//    public sealed class DoesNotReturnAttribute : Attribute
//    {

//    }
//}
//#endif