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

using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 定义了一个基础的Web API客户端接口，该接口扩展了IRpcClient。
    /// </summary>
    /// <remarks>
    /// 此接口的目的是为Web API客户端提供一个统一的接口定义，任何实现该接口的类都继承了RPC客户端的基本功能，
    /// 并且可能提供了Web API特定的功能和方法。
    /// </remarks>
    public interface IWebApiClientBase : IRpcClient
    {
    }
}