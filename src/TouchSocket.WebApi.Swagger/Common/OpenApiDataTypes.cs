using Newtonsoft.Json;

namespace TouchSocket.WebApi.Swagger
{
    [JsonConverter(typeof(OpenApiStringEnumConverter))]
    internal enum OpenApiDataTypes
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,

        /// <summary>
        /// 数值
        /// </summary>
        Number,

        /// <summary>
        /// 整数
        /// </summary>
        Integer,

        /// <summary>
        /// 布尔值
        /// </summary>
        Boolean,

        /// <summary>
        /// 二进制（如文件）
        /// </summary>
        Binary,

        /// <summary>
        /// 二进制集合
        /// </summary>
        BinaryCollection,

        /// <summary>
        /// 记录值（字典）
        /// </summary>
        Record,

        /// <summary>
        /// 元组值
        /// </summary>
        Tuple,

        /// <summary>
        /// 数组
        /// </summary>
        Array,

        /// <summary>
        /// 对象
        /// </summary>
        Object,

        /// <summary>
        /// 结构
        /// </summary>
        Struct,

        /// <summary>
        /// Any
        /// </summary>
        Any
    }
}
