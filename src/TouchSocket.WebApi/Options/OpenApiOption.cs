// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Rpc;
using TouchSocket.WebApi.OpenApi;

namespace TouchSocket.WebApi;

/// <summary>
/// OpenApi配置选项。
/// </summary>
public class OpenApiOption
{
    /// <summary>
    /// 访问 OpenApi JSON 的路径前缀，默认为 "openapi"。
    /// </summary>
    public string Prefix { get; set; } = "openapi";

    /// <summary>
    /// 设置访问 OpenApi JSON 的路径前缀。
    /// </summary>
    /// <param name="value">前缀字符串</param>
    public void SetPrefix(string value)
    {
        this.Prefix = value;
    }

    /// <summary>
    /// OpenAPI 操作元数据构建完成后的回调委托。
    /// 每个 WebApi 方法对应的 <see cref="OpenApiPathValue"/> 生成完毕后会调用此委托，
    /// 可在此修改响应类型、标签、描述等元数据。
    /// </summary>
    public Action<RpcMethod, OpenApiPathValue> ConfigureOperation { get; set; }

    /// <summary>
    /// 自定义标签获取委托。若为 <see langword="null"/>，则默认使用方法所在类的类型名称作为标签。
    /// </summary>
    public Func<RpcMethod, IEnumerable<string>> GetTags { get; set; }
}
