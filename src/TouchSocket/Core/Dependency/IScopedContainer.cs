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

namespace TouchSocket.Core.Dependency
{
    /// <summary>
    /// 具有区域效应的容器。
    /// </summary>
    public interface IScopedContainer
    {
        /// <summary>
        /// 创建目标类型的对应实例。
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="ps"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(Type fromType, object[] ps = null, string key = "");

        /// <summary>
        /// 判断某类型是否已经注册
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRegistered(Type fromType, string key = "");
    }
}