using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    /// <summary>
    /// 表示一个键值对集合，通常用于表示HTTP表单中的数据
    /// </summary>
    public interface IFormCollection : IEnumerable<KeyValuePair<string, string>>
    {
        /// <summary>
        /// 获取集合中键值对的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取包含上传文件的集合
        /// </summary>
        IMultifileCollection Files { get; }

        /// <summary>
        /// 获取集合中所有键的集合
        /// </summary>
        ICollection<string> Keys { get; }

        /// <summary>
        /// 根据键获取对应的值
        /// </summary>
        /// <param name="key">要获取值的键</param>
        /// <returns>与指定键关联的值</returns>
        string this[string key] { get; }

        /// <summary>
        /// 根据键获取对应的值
        /// </summary>
        /// <param name="key">要获取的键</param>
        /// <returns>与指定键关联的值</returns>
        string Get(string key);

        /// <summary>
        /// 判断集合中是否包含指定的键
        /// </summary>
        /// <param name="key">要检查的键</param>
        /// <returns>如果集合包含指定的键，则返回true；否则返回false</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 尝试根据键获取对应的值
        /// </summary>
        /// <param name="key">要获取值的键</param>
        /// <param name="value">与指定键关联的值，如果键不存在则为null</param>
        /// <returns>如果键存在于集合中，则返回true；否则返回false</returns>
        bool TryGetValue(string key, out string value);
    }
}