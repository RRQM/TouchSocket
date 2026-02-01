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

using System.Text.Json.Serialization;

namespace TouchSocket.WebApi.Swagger;

[JsonSourceGenerationOptions(
WriteIndented = true,
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
Converters = [typeof(OpenApiStringEnumConverter)])]
[JsonSerializable(typeof(OpenApiRoot))]
[JsonSerializable(typeof(OpenApiInfo))]
[JsonSerializable(typeof(OpenApiPath))]
[JsonSerializable(typeof(OpenApiPathValue))]
[JsonSerializable(typeof(OpenApiParameter))]
[JsonSerializable(typeof(OpenApiSchema))]
[JsonSerializable(typeof(OpenApiProperty))]
[JsonSerializable(typeof(OpenApiRequestBody))]
[JsonSerializable(typeof(OpenApiResponse))]
[JsonSerializable(typeof(OpenApiContent))]
[JsonSerializable(typeof(OpenApiComponent))]
[JsonSerializable(typeof(OpenApiDataTypes))]
[JsonSerializable(typeof(Dictionary<string, OpenApiPath>))]
[JsonSerializable(typeof(Dictionary<string, OpenApiPathValue>))]
[JsonSerializable(typeof(Dictionary<string, OpenApiSchema>))]
[JsonSerializable(typeof(Dictionary<string, OpenApiProperty>))]
[JsonSerializable(typeof(Dictionary<string, OpenApiContent>))]
[JsonSerializable(typeof(Dictionary<string, OpenApiResponse>))]
[JsonSerializable(typeof(IEnumerable<string>))]
[JsonSerializable(typeof(IEnumerable<OpenApiParameter>))]
[JsonSerializable(typeof(long[]))]
internal partial class SwaggerJsonSerializerContext : JsonSerializerContext
{
}
