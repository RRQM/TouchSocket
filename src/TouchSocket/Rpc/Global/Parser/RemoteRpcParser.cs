//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TouchSocket.Rpc
//{
//    /// <summary>
//    /// 远程Rpc解析器
//    /// </summary>
//    public class RemoteRpcParser : IRpcParser
//    {
//        public Action<IRpcParser, MethodInvoker, MethodInstance> RRQMExecuteMethod => throw new NotImplementedException();

//        public MethodMap MethodMap => throw new NotImplementedException();

//        public RpcService RpcService => throw new NotImplementedException();

//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public void GetProxyInfo(GetProxyInfoArgs args)
//        {
//            throw new NotImplementedException();
//        }

//        public void OnEndInvoke(MethodInvoker methodInvoker, MethodInstance methodInstance)
//        {
//            throw new NotImplementedException();
//        }

//        public void OnRegisterServer(IServerProvider provider, MethodInstance[] methodInstances)
//        {
//            throw new NotImplementedException();
//        }

//        public void OnUnregisterServer(IServerProvider provider, MethodInstance[] methodInstances)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetExecuteMethod(Action<IRpcParser, MethodInvoker, MethodInstance> executeMethod)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetMethodMap(MethodMap methodMap)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetRpcService(RpcService service)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}