//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace TouchSocket.Core
//{
//    /// <summary>
//    /// 源生成容器特性
//    /// </summary>
//    internal class GeneratorContainerAttribute : Attribute
//    {
//    }

//    internal class BaseInjectAttribute : Attribute
//    {
//        /// <summary>
//        /// 注册类型
//        /// </summary>
//        public Type FromType { get; set; }

//        /// <summary>
//        /// 实例类型
//        /// </summary>
//        public Type ToType { get; set; }

//        /// <summary>
//        /// 注册额外键
//        /// </summary>
//        public string Key { get; set; }
//    }

//    /// <summary>
//    /// 自动注入为单例。
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Interface, AllowMultiple = true)]
//    internal class AutoInjectForSingletonAttribute : BaseInjectAttribute
//    {
//    }

//    /// <summary>
//    /// 自动注入为瞬时。
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
//    internal class AutoInjectForTransientAttribute : BaseInjectAttribute
//    {
//    }

//    /// <summary>
//    /// 添加单例注入。
//    /// <para>
//    /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//    /// </para>
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//    internal class AddSingletonInjectAttribute : BaseInjectAttribute
//    {
//        /// <summary>
//        /// 添加单例注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="fromType">注册类型</param>
//        /// <param name="toType">实例类型</param>
//        /// <param name="key">注册额外键</param>
//        public AddSingletonInjectAttribute(Type fromType, Type toType, string key)
//        {
//            this.FromType = fromType;
//            this.ToType = toType;
//            this.Key = key;
//        }

//        /// <summary>
//        /// 添加单例注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="type">注册类型与实例类型一致</param>
//        public AddSingletonInjectAttribute(Type type) : this(type, type, null)
//        {
//        }

//        /// <summary>
//        /// 添加单例注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="fromType">注册类型</param>
//        /// <param name="toType">实例类型</param>
//        public AddSingletonInjectAttribute(Type fromType, Type toType) : this(fromType, toType, null)
//        {
//        }
//    }

//    /// <summary>
//    /// 添加瞬态注入。
//    /// <para>
//    /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//    /// </para>
//    /// </summary>
//    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//    internal class AddTransientInjectAttribute : BaseInjectAttribute
//    {
//        /// <summary>
//        /// 添加瞬态注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="fromType">注册类型</param>
//        /// <param name="toType">实例类型</param>
//        /// <param name="key">注册额外键</param>
//        public AddTransientInjectAttribute(Type fromType, Type toType, string key)
//        {
//            this.FromType = fromType;
//            this.ToType = toType;
//            this.Key = key;
//        }

//        /// <summary>
//        /// 添加瞬态注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="type">注册类型与实例类型一致</param>
//        public AddTransientInjectAttribute(Type type) : this(type, type, null)
//        {
//        }

//        /// <summary>
//        /// 添加瞬态注入。
//        /// <para>
//        /// 该标签仅添加在继承<see cref="ManualContainer"/>的容器上时有用。
//        /// </para>
//        /// </summary>
//        /// <param name="fromType">注册类型</param>
//        /// <param name="toType">实例类型</param>
//        public AddTransientInjectAttribute(Type fromType, Type toType) : this(fromType, toType, null)
//        {
//        }
//    }
//}