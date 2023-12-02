////------------------------------------------------------------------------------
////  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  API首页：http://rrqm_home.gitee.io/touchsocket/
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------

//using System;

//namespace TouchSocket.Core.AspNetCore
//{
//    public class AspNetCoreResolver : IResolver
//    {
//        private readonly Func<Type, bool> m_func;
//        private readonly IServiceProvider m_serviceProvider;

//        public AspNetCoreResolver(IServiceProvider serviceProvider, Func<Type, bool> funcIsRegistered)
//        {
//            this.m_serviceProvider = serviceProvider;
//            this.m_func = funcIsRegistered;
//        }

//        public object GetService(Type serviceType)
//        {
//            return this.m_serviceProvider.GetService(serviceType);
//        }

//        public bool IsRegistered(Type fromType, string key)
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsRegistered(Type fromType)
//        {
//            return this.m_func.Invoke(fromType);
//        }

//        public object Resolve(Type fromType, string key)
//        {
//            throw new NotImplementedException();
//        }

//        public object Resolve(Type fromType)
//        {
//            return this.m_serviceProvider.GetService(fromType);
//        }
//    }
//}