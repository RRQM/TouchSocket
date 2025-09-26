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

namespace TouchSocket.JsonRpc;

/// <summary>
/// JsonRpcParser解析器插件
/// </summary>
public abstract class JsonRpcParserPluginBase : PluginBase
{
    /// <summary>
    /// 获取RPC服务器提供程序。
    /// </summary>
    public IRpcServerProvider RpcServerProvider { get; }

    /// <summary>
    /// 获取动作映射。
    /// </summary>
    public ActionMap ActionMap { get; } = new ActionMap(true);

    /// <summary>
    /// 初始化 <see cref="JsonRpcParserPluginBase"/> 类的新实例。
    /// </summary>
    /// <param name="rpcServerProvider">RPC服务器提供程序。</param>
    public JsonRpcParserPluginBase(IRpcServerProvider rpcServerProvider)
    {
        this.SerializerConverter.Add(new JsonStringToClassSerializerFormatter<JsonRpcActor>());
        if (rpcServerProvider is not null)
        {
            this.RpcServerProvider = rpcServerProvider;
            JsonRpcActor.AddRpcToMap(rpcServerProvider, this.ActionMap);
        }
    }

    /// <summary>
    /// 获取序列化转换器。
    /// </summary>
    public TouchSocketSerializerConverter<string, JsonRpcActor> SerializerConverter { get; } = new TouchSocketSerializerConverter<string, JsonRpcActor>();
}