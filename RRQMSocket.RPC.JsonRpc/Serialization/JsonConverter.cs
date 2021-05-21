//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.IO;

namespace RRQMSocket.RPC.JsonRpc
{
    /// <summary>
    /// Json序列化转换器
    /// </summary>
    public abstract class JsonConverter
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public abstract void Serialize(Stream stream,object parameter);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public abstract object Deserialize(string jsonString, Type parameterType);
    }
}