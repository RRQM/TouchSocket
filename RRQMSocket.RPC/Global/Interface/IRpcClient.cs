//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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

using RRQMCore;
using System;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// RPC接口
    /// </summary>
    public interface IRpcClient : IDisposable
    {
        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Invoke(string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>服务器返回结果</returns>
        T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters);

        /// <summary>
        /// RPC调用
        /// <para>如果调用端为客户端，则会调用服务器RPC服务。</para>
        /// <para>如果调用端为服务器，则会反向调用客户端RPC服务。</para>
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types">对应类型集合</param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回值</returns>
        T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types);

        /// <summary>
        /// RPC调用
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="parameters">参数</param>
        /// <param name="types"></param>
        /// <param name="invokeOption">RPC调用设置</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RRQMSerializationException">序列化异常</exception>
        /// <exception cref="RRQMRPCInvokeException">RPC异常</exception>
        /// <exception cref="RRQMRPCNoRegisterException">RPC服务器未注册</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types);
    }
}