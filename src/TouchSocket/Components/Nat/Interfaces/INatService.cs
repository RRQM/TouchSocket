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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 定义了一个支持NAT（网络地址转换）服务的接口，用于扩展ITcpServiceBase服务以支持NAT穿透功能。
    /// </summary>
    /// <typeparam name="TClient">客户端类型的参数，必须实现INatSessionClient接口。</typeparam>
    public interface INatService<TClient> : ITcpServiceBase<TClient> where TClient : INatSessionClient
    {

    }
}
