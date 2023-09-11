using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.WebApi.Swagger
{
    /// <summary>
    /// Swagger描述特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SwaggerDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Swagger描述特性
        /// </summary>
        /// <param name="groups"></param>
        public SwaggerDescriptionAttribute(string[] groups)
        {
            this.Groups = groups;
        }

        /// <summary>
        /// 分组
        /// </summary>
        public string[] Groups { get; set; }
    }
}
