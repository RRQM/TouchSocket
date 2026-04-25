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

namespace TouchSocket.WebApi.Swagger;

/// <summary>
/// Swagger 配置选项。
/// </summary>
public class SwaggerOption
{

    /// <summary>
    /// 是否在浏览器打开Swagger页面
    /// </summary>
    public bool LaunchBrowser { get; set; }

    /// <summary>
    /// 访问Swagger的前缀，默认“swagger”
    /// </summary>
    public string Prefix { get; set; } = "swagger";

    /// <summary>
    /// 设置访问Swagger的前缀，默认“swagger”
    /// </summary>
    public void SetPrefix(string value)
    {
        this.Prefix = value;
    }

    /// <summary>
    /// 在浏览器打开Swagger页面
    /// </summary>
    public void UseLaunchBrowser()
    {
        this.LaunchBrowser = true;
    }

    /// <summary>
    /// OpenAPI 操作元数据构建完成后的回调委托。
    /// 每个 WebApi 方法对应的 <see cref="OpenApiPathValue"/> 生成完毕后会调用此委托，
    /// 可在此修改响应类型、标签、描述等元数据，使 OpenAPI 文档与运行时行为保持一致。
    /// </summary>
    public Action<RpcMethod, OpenApiPathValue> ConfigureOperation { get; set; }
}
