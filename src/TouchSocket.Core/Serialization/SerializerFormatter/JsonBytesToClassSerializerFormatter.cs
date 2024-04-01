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

using Newtonsoft.Json;
using System;
using System.Text;

namespace TouchSocket.Core
{
    /// <summary>
    /// Json字节转到对应类
    /// </summary>
    public class JsonBytesToClassSerializerFormatter<TState> : ISerializerFormatter<byte[], TState>
    {
        /// <summary>
        /// JsonSettings
        /// </summary>
        public JsonSerializerSettings JsonSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="source"></param>
        /// <param name="targetType"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool TryDeserialize(TState state, in byte[] source, Type targetType, out object target)
        {
            try
            {
                target = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(source), targetType, this.JsonSettings);
                return true;
            }
            catch
            {
                target = default;
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual bool TrySerialize(TState state, in object target, out byte[] source)
        {
            try
            {
                source = JsonConvert.SerializeObject(target, this.JsonSettings).ToUTF8Bytes();
                return true;
            }
            catch (Exception)
            {
                source = null;
                return false;
            }
        }
    }
}