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
using System;

namespace TouchSocket.Core.XREF.Newtonsoft.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> to deserialize properties with no matching class member into the specified collection
    /// and write values during serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class JsonExtensionDataAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value that indicates whether to write extension data when serializing the object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to write extension data when serializing the object; otherwise, <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool WriteData { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to read extension data when deserializing the object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to read extension data when deserializing the object; otherwise, <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool ReadData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonExtensionDataAttribute"/> class.
        /// </summary>
        public JsonExtensionDataAttribute()
        {
            this.WriteData = true;
            this.ReadData = true;
        }
    }
}