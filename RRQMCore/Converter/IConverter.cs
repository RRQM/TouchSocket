using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Converter
{
    /// <summary>
    /// 转换器接口
    /// </summary>
    public interface IConverter<TSource>
    {

        /// <summary>
        /// 转换器执行顺序
        /// <para>该属性值越小，越靠前执行。值相等时，按添加先后顺序</para>
        /// <para>该属性效果，仅在<see cref="RRQMConverter{TSource}.Add(IConverter{TSource})"/>之前设置有效。</para>
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// 尝试将源数据转换目标类型对象
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool TryConvertFrom(TSource source,Type targetType,out object target);

        /// <summary>
        /// 尝试将目标类型对象转换源数据
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        bool TryConvertTo(object target,out TSource source);
    }
}
