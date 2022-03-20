//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Extensions;
using RRQMCore.XREF.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RRQMCore
{
    /// <summary>
    /// 运行配置类
    /// </summary>
    public abstract class AppConfigBase
    {
        private readonly string fullPath;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fullPath"></param>
        public AppConfigBase(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentException($"“{nameof(fullPath)}”不能为 null 或空。", nameof(fullPath));
            }

            this.fullPath = fullPath;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="overwrite"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Save(bool overwrite, out string msg)
        {
            if (overwrite == false && File.Exists(this.fullPath))
            {
                msg = null;
                return true;
            }
            try
            {
                File.WriteAllBytes(this.fullPath, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, Formatting.Indented)));
                msg = null;
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Load(out string msg)
        {
            try
            {
                if (!File.Exists(this.fullPath))
                {
                    this.Save(false, out _);
                }
                var obj = File.ReadAllText(this.fullPath).ToJsonObject(this.GetType());
                var ps = this.GetType().GetProperties();

                foreach (var item in ps)
                {
                    item.SetValue(this, item.GetValue(obj));
                }
                msg = null;
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取默认配置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDefault<T>() where T : AppConfigBase, new()
        {
            Type type = typeof(T);
            if (list.TryGetValue(type, out object value))
            {
                return (T)value;
            }
            T _default = ((T)Activator.CreateInstance(typeof(T)));
            _default.Load(out _);
            list.Add(type, _default);
            return _default;
        }

        private static Dictionary<Type, object> list = new Dictionary<Type, object>();

        /// <summary>
        /// 获取默认配置，每次调用该方法时，都会重新加载配置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetNewDefault<T>() where T : AppConfigBase, new()
        {
            T _default = ((T)Activator.CreateInstance(typeof(T)));
            _default.Load(out _);
            if (list.ContainsKey(_default.GetType()))
            {
                list[_default.GetType()] = _default;
            }
            else
            {
                list.Add(_default.GetType(), _default);
            }
            return _default;
        }
    }
}