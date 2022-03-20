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

using System;

namespace RRQMCore.Dependency
{
    /// <summary>
    /// 依赖项属性
    /// </summary>
    public class DependencyProperty
    {
        private DependencyProperty()
        {
        }

        private string name;

        /// <summary>
        /// 属性名
        /// </summary>
        public string Name => this.name;

        private Type owner;

        /// <summary>
        /// 所属类型
        /// </summary>
        public Type Owner => this.owner;

        private Type valueType;

        /// <summary>
        /// 值类型
        /// </summary>
        public Type ValueType => this.valueType;

        private object value;

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefauleValue => this.value;

        internal void DataValidation(object value)
        {
            if (value == null)
            {
                if (typeof(ValueType).IsAssignableFrom(this.valueType))
                {
                    throw new RRQMException($"属性“{this.name}”赋值类型不允许出现Null");
                }
            }
            else if (!this.valueType.IsAssignableFrom(value.GetType()))
            {
                throw new RRQMException($"属性“{this.name}”赋值类型与注册类型不一致，应当注入“{this.valueType}”类型");
            }
        }

        internal void SetDefauleValue(object value)
        {
            this.DataValidation(value);
            this.value = value;
        }

        /// <summary>
        /// 注册依赖项属性
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="valueType"></param>
        /// <param name="owner"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DependencyProperty Register(string propertyName, Type valueType, Type owner, object value)
        {
            DependencyProperty dp = new DependencyProperty();
            dp.name = propertyName;
            dp.valueType = valueType;
            dp.owner = owner;
            dp.SetDefauleValue(value);
            return dp;
        }
    }
}