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
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TouchSocket.Core
//{
//    /// <summary>
//    /// 异常助手
//    /// </summary>
//    public static class ThrowHelper
//    {
//        private static readonly ConcurrentDictionary<Enum, Func<string, Exception, Exception>> m_pairs =
//            new ConcurrentDictionary<Enum, Func<string, Exception, Exception>>();

//        /// <summary>
//        /// 添加抛出规则。
//        /// </summary>
//        /// <param name="enum"></param>
//        /// <param name="func"></param>
//        public static void Add(Enum @enum,Func<string,Exception,Exception> func)
//        {
//            if (@enum is null)
//            {
//                throw new ArgumentNullException(nameof(@enum));
//            }

//            if (func is null)
//            {
//                throw new ArgumentNullException(nameof(func));
//            }

//            m_pairs.TryRemove(@enum,out _);
//            m_pairs.TryAdd(@enum,func);
//        }

//        public static Exception Throw(Enum @enum,string msg,Exception exception)
//        {
//        }

//    }
//}