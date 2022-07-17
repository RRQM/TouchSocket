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
using System.Text;
using TouchSocket.Core.XREF.Newtonsoft.Json;

namespace TouchSocket.Core.Extensions
{
    /// <summary>
    /// JsonExtension
    /// </summary>
    public static class JsonExtension
    {
        #region Json转换

        /// <summary>
        /// 序列化成Json数据
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToJsonBytes(this object obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None));
        }

        /// <summary>
        /// 转换为json字符串。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None);
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this byte[] buffer, int offset, int len)
        {
            return Encoding.UTF8.GetString(buffer, offset, len).ToJsonObject<T>();
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer).ToJsonObject<T>();
        }

        /// <summary>
        ///  反序列化成Json数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T ToJsonObject<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// 反序列化成Json数据
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToJsonObject(this string jsonString, Type type)
        {
            return JsonConvert.DeserializeObject(jsonString, type);
        }

        #endregion Json转换
    }
}