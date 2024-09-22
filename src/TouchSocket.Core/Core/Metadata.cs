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

using System.Collections.Generic;

namespace TouchSocket.Core
{
    /// <summary>
    /// 元数据键值对。
    /// </summary>
    [FastConverter(typeof(MetadataFastBinaryConverter))]
    public class Metadata : Dictionary<string, string>, IPackage
    {
        /// <inheritdoc/>
        /// <param name="key"></param>
        /// <returns></returns>
        public new string this[string key] => this.TryGetValue(key, out var value) ? value : null;

        /// <summary>
        /// 向元数据集合添加一个键值对。如果键已经存在，则覆盖其值。
        /// </summary>
        /// <param name="name">要添加的键。</param>
        /// <param name="value">与键关联的值。</param>
        /// <returns>返回当前元数据对象，以支持链式调用。</returns>
        public new Metadata Add(string name, string value)
        {
            base.Add(name, value); // 调用基类方法添加键值对，如果键存在则覆盖值。
            return this; // 返回当前元数据对象，允许进行链式调用。
        }

        /// <inheritdoc/>
        public void Package<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            byteBlock.WriteInt32(this.Count);
            foreach (var item in this)
            {
                byteBlock.WriteString(item.Key, FixedHeaderType.Byte);
                byteBlock.WriteString(item.Value, FixedHeaderType.Byte);
            }
        }

        /// <inheritdoc/>
        public void Unpackage<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
        {
            var count = byteBlock.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var key = byteBlock.ReadString(FixedHeaderType.Byte);
                var value = byteBlock.ReadString(FixedHeaderType.Byte);
                this.Add(key, value);
            }
        }
    }
}