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
using System.Diagnostics;

namespace TouchSocket.Core.Dependency
{
    /// <summary>
    /// 依赖项属性
    /// </summary>
    [DebuggerDisplay("Name={Name},Type={ValueType}")]
    public class DependencyProperty
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        protected string m_name;

        /// <summary>
        /// 所属类型
        /// </summary>
        protected Type m_owner;

        /// <summary>
        /// 值类型
        /// </summary>
        protected Type m_valueType;

        private object m_value;

        /// <summary>
        /// 依赖项属性
        /// </summary>
        private DependencyProperty()
        {
        }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefauleValue => this.m_value;

        /// <summary>
        /// 属性名
        /// </summary>
        public string Name => this.m_name;

        /// <summary>
        /// 所属类型
        /// </summary>
        public Type Owner => this.m_owner;

        /// <summary>
        /// 值类型
        /// </summary>
        public Type ValueType => this.m_valueType;


        internal void DataValidation(object value)
        {
            if (value == null)
            {
                if (typeof(ValueType).IsAssignableFrom(this.m_valueType))
                {
                    throw new Exception($"属性“{this.m_name}”赋值类型不允许出现Null");
                }
            }
            else if (!this.m_valueType.IsAssignableFrom(value.GetType()))
            {
                throw new Exception($"属性“{this.m_name}”赋值类型与注册类型不一致，应当注入“{this.m_valueType}”类型");
            }
        }

        internal void SetDefauleValue(object value)
        {
            this.DataValidation(value);
            this.m_value = value;
        }

        /// <summary>
        /// 注册依赖项属性。
        /// <para>依赖属性的默认值，可能会应用于所有的<see cref="IDependencyObject"/></para>
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="valueType"></param>
        /// <param name="owner"></param>
        /// <param name="value">依赖项属性值，一般该值应该是值类型，因为它可能会被用于多个依赖对象。</param>
        /// <returns></returns>
        public static DependencyProperty Register(string propertyName, Type valueType, Type owner, object value)
        {
            DependencyProperty dp = new DependencyProperty
            {
                m_name = propertyName,
                m_valueType = valueType,
                m_owner = owner
            };
            dp.SetDefauleValue(value);
            return dp;
        }
    }
}