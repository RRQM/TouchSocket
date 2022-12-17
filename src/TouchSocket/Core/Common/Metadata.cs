//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Specialized;

namespace TouchSocket.Core
{
    /// <summary>
    /// 元数据键值对。
    /// </summary>
    [FastConverter(typeof(MetadataFastBinaryConverter))]
    public class Metadata : NameValueCollection, IPackage
    {
        /// <summary>
        /// 元数据键值对。
        /// </summary>
        public Metadata()
        {
        }

        /// <summary>
        /// 添加。如果键存在，将被覆盖。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public new Metadata Add(string name, string value)
        {
            base.Add(name, value);
            return this;
        }

        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Package(ByteBlock byteBlock)
        {
            byteBlock.Write(Count);
            foreach (var item in AllKeys)
            {
                byteBlock.Write(item);
                byteBlock.Write(this[item]);
            }
        }

        /// <summary>
        /// 解包
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Unpackage(ByteBlock byteBlock)
        {
            int count = byteBlock.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = byteBlock.ReadString();
                string value = byteBlock.ReadString();
                Add(key, value);
            }
        }
    }
}