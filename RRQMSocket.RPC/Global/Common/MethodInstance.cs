//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC函数实例
    /// </summary>
    public class MethodInstance
    {
        /// <summary>
        /// 执行此RPC的实例
        /// </summary>
        public IServerProvider Provider { get; internal set; }

        /// <summary>
        /// 实例类型
        /// </summary>
        public Type ProviderType { get; internal set; }

        /// <summary>
        /// RPC函数
        /// </summary>
        public MethodInfo Method { get; internal set; }

        /// <summary>
        /// RPC属性集合
        /// </summary>
        public RPCAttribute[] RPCAttributes { get; internal set; }

        /// <summary>
        /// 描述属性集合
        /// </summary>
        public DescriptionAttribute DescriptionAttribute { get; internal set; }

        /// <summary>
        /// 方法唯一令箭
        /// </summary>
        public int MethodToken { get; internal set; }

        /// <summary>
        /// 返回值类型，无返回值时为Null
        /// </summary>
        public Type ReturnType { get; internal set; }

        /// <summary>
        /// 参数类型集合，已处理out及ref，无参数时为空集合，
        /// </summary>
        public Type[] ParameterTypes { get; internal set; }

        /// <summary>
        /// 参数集合
        /// </summary>
        public ParameterInfo[] Parameters { get; internal set; }

        /// <summary>
        /// 参数名集合
        /// </summary>
        public string[] ParameterNames { get; internal set; }

        /// <summary>
        /// 是否异步执行
        /// </summary>
        public AsyncType AsyncType { get; internal set; }

        /// <summary>
        /// 是否有引用类型
        /// </summary>
        public bool IsByRef { get; internal set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnable { get; internal set; }

        /// <summary>
        /// 函数标识
        /// </summary>
        public MethodFlags MethodFlags { get; internal set; }

        /// <summary>
        /// 获取指定类型属性标签
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAttribute<T>()
        {
            object attribute = this.RPCAttributes.FirstOrDefault((a) => { return a.GetType() == typeof(T); });
            if (attribute == null)
            {
                return default;
            }
            return (T)attribute;
        }
    }
}