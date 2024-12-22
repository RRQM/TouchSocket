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

#if SystemTextJson

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 使用System.Text.Json进行字符串与类之间序列化和反序列化的格式化器。
    /// </summary>
    /// <typeparam name="TState">状态类型。</typeparam>
    public class SystemTextJsonStringToClassSerializerFormatter<TState> : ISerializerFormatter<string, TState>
    {
        /// <summary>
        /// 获取或设置Json序列化选项。
        /// </summary>
        public JsonSerializerOptions JsonSettings { get; set; } = new JsonSerializerOptions();

        /// <summary>
        /// 获取或设置格式化器的顺序。
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 尝试将字符串反序列化为指定类型的对象。
        /// </summary>
        /// <param name="state">状态对象。</param>
        /// <param name="source">源字符串。</param>
        /// <param name="targetType">目标类型。</param>
        /// <param name="target">反序列化后的对象。</param>
        /// <returns>如果反序列化成功，则为true；否则为false。</returns>
        public bool TryDeserialize(TState state, in string source, Type targetType, out object target)
        {
            try
            {
                if (source.IsNullOrEmpty())
                {
                    target = targetType.GetDefault();
                    return true;
                }
                target = JsonSerializer.Deserialize(source, targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试将对象序列化为字符串。
        /// </summary>
        /// <param name="state">状态对象。</param>
        /// <param name="target">要序列化的对象。</param>
        /// <param name="source">序列化后的字符串。</param>
        /// <returns>如果序列化成功，则为true；否则为false。</returns>
        public bool TrySerialize(TState state, in object target, out string source)
        {
            try
            {
                if (target is null)
                {
                    source = string.Empty;
                    return true;
                }
                source = JsonSerializer.Serialize(target, target.GetType(), this.JsonSettings);
                return true;
            }
            catch
            {
                source = null;
                return false;
            }

        }
    }
}
#endif