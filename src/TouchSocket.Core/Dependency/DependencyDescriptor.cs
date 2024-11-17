//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;

namespace TouchSocket.Core
{
    /// <summary>
    /// 注入依赖对象
    /// </summary>
    public class DependencyDescriptor
    {
        /// <summary>
        /// 初始化一个单例实例。
        /// </summary>
        /// <param name="fromType">要从该类型创建实例的类型。</param>
        /// <param name="instance">创建的单例实例。</param>
        public DependencyDescriptor(Type fromType, object instance)
        {
            this.FromType = fromType; // 设置从哪个类型创建实例
            this.ToInstance = instance; // 设置要创建的实例
            this.Lifetime = Lifetime.Singleton; // 设置实例的生命周期为单例
            this.ToType = instance.GetType(); // 设置实例的类型
        }

        /// <summary>
        /// 初始化一个完整的服务注册
        /// </summary>
        /// <param name="fromType">要注册的服务的类型</param>
        /// <param name="toType">服务在容器中实际使用的类型</param>
        /// <param name="lifetime">服务的生命周期</param>
        public DependencyDescriptor(Type fromType, Type toType, Lifetime lifetime)
        {
            this.FromType = fromType;
            this.Lifetime = lifetime;
            this.ToType = toType;
        }

        /// <summary>
        /// 初始化一个简单的服务描述
        /// </summary>
        /// <param name="fromType">指定服务的类型</param>
        public DependencyDescriptor(Type fromType)
        {
            this.FromType = fromType;
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        public Type FromType { get; }

        /// <summary>
        /// 实例化工厂委托
        /// </summary>
        public Func<IResolver, object> ImplementationFactory { get; set; }

        /// <summary>
        /// 生命周期
        /// </summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 在获取到注册时触发委托。
        /// <para>
        /// 在单例实例注册时，不会触发。在单例注册时，只会触发一次，在瞬态注册时，会每次都触发。
        /// </para>
        /// </summary>
        public Action<object> OnResolved { get; set; }

        /// <summary>
        /// 实例
        /// </summary>
        public object ToInstance { get; set; }

        /// <summary>
        /// 实例类型
        /// </summary>
        public Type ToType { get; }
    }
}