using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 序列化器
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public interface ISerializer<TSource>
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        TSource Serialize(object target);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        object Deserialize(TSource source,Type targetType);
    }
}
