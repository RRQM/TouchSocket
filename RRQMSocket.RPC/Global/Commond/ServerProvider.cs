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
using System.Reflection;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC范围类
    /// </summary>
    public abstract class ServerProvider
    {
        /// <summary>
        /// 默认复刻程序集
        /// </summary>
        public static Assembly DefaultAssembly { get; set; }


        /// <summary>
        /// 该服务所属的服务器
        /// </summary>
        public RPCService RPCService { get; internal set; }

        internal void RPC(int index, IRPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
            switch (index)
            {
                case 1:
                    {
                        RPCEnter(parser, methodInvoker, methodInstance);
                        break;
                    }
                case 2:
                    {
                        RPCError(parser, methodInvoker, methodInstance);
                        break;
                    }
                case 3:
                    {
                        RPCLeave(parser, methodInvoker, methodInstance);
                        break;
                    }
            }
        }

        /// <summary>
        /// RPC即将进入,
        /// 若是想放弃本次执行，请抛出<see cref="RRQMAbandonRPCException"/>
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected virtual void RPCEnter(IRPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
        }

        /// <summary>
        /// 执行RPC发生错误
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected virtual void RPCError(IRPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
        }

        /// <summary>
        /// RPC方法执行完成
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="methodInvoker"></param>
        /// <param name="methodInstance"></param>
        protected virtual void RPCLeave(IRPCParser parser, MethodInvoker methodInvoker, MethodInstance methodInstance)
        {
        }
    }
}