//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 方法体
    /// </summary>
    public class MethodItem
    {
        /// <summary>
        /// 方法名
        /// </summary>
        public string Method { get; internal set; }

        /// <summary>
        /// 返回值类型
        /// </summary>
        internal Type ReturnType;

        /// <summary>
        /// 参数类型
        /// </summary>
        internal List<Type> ParameterTypes;

        /// <summary>
        /// 返回值类型
        /// </summary>
        public string ReturnTypeString { get; internal set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public List<string> ParameterTypesString { get; internal set; }

        /// <summary>
        /// 是否含有Out或Ref
        /// </summary>
        public bool IsOutOrRef { get; internal set; }
    }
}