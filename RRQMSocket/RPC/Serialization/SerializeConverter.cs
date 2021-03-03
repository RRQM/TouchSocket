//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
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
    /*
    若汝棋茗
    */

    /// <summary>
    /// 序列化转换器
    /// </summary>
    public abstract class SerializeConverter
    {
        /// <summary>
        /// 序列化RPC方法返回值参数
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public abstract byte[] SerializeParameter(object parameter);

        
        /// <summary>
        /// 反序列化传输对象
        /// </summary>
        /// <param name="parameterBytes"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public abstract object DeserializeParameter(byte[] parameterBytes, Type parameterType);

    }
}