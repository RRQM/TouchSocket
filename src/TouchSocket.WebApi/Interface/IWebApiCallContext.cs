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

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 定义了一个用于Web API调用上下文的接口，继承自IHttpCallContext。
    /// </summary>
    /// <remarks>
    /// 此接口旨在为Web API调用提供特定的上下文信息，以便在这样的调用中传递和处理额外的数据。
    /// 实现这个接口的类应该提供特定于Web API环境的方法和属性。
    /// </remarks>
    public interface IWebApiCallContext : IHttpCallContext
    {
    }
}