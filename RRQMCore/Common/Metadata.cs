using System.Collections.Generic;

namespace RRQMCore
{
    /// <summary>
    /// 可传输的元数据
    /// </summary>
    public class Metadata : Dictionary<string, string>
    {
        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(string key, string value)
        {
            if (this.ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                this.Add(key, value);
            }
        }
    }
}
