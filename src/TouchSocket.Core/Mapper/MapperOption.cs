using System.Collections.Generic;

namespace TouchSocket.Core
{
    /// <summary>
    /// 映射配置
    /// </summary>
    public class MapperOption
    {
        /// <summary>
        /// 需要忽略的属性名称
        /// </summary>
        public List<string> IgnoreProperties { get; set; }

        /// <summary>
        /// 映射属性名称
        /// </summary>
        public Dictionary<string, string> MapperProperties { get; set; }
    }
}