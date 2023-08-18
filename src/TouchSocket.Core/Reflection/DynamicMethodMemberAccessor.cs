using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace TouchSocket.Core
{
    /// <summary>
    /// DynamicMethodMemberAccessor
    /// </summary>
    public class DynamicMethodMemberAccessor : IMemberAccessor
    {
        private readonly ConcurrentDictionary<Type, IMemberAccessor> m_classAccessors = new ConcurrentDictionary<Type, IMemberAccessor>();

        static DynamicMethodMemberAccessor()
        {
            Default = new DynamicMethodMemberAccessor();
        }

        /// <summary>
        /// DynamicMethodMemberAccessor的默认实例。
        /// </summary>
        public static DynamicMethodMemberAccessor Default { get; private set; }

        /// <summary>
        /// 获取字段
        /// </summary>
        public Func<Type, FieldInfo[]> OnGetFieldInfes { get; set; }

        /// <summary>
        /// 获取属性
        /// </summary>
        public Func<Type, PropertyInfo[]> OnGetProperties { get; set; }

        /// <inheritdoc/>
        public object GetValue(object instance, string memberName)
        {
            return this.FindClassAccessor(instance).GetValue(instance, memberName);
        }

        /// <inheritdoc/>
        public void SetValue(object instance, string memberName, object newValue)
        {
            this.FindClassAccessor(instance).SetValue(instance, memberName, newValue);
        }

        private IMemberAccessor FindClassAccessor(object instance)
        {
            var typekey = instance.GetType();
            if (!this.m_classAccessors.TryGetValue(typekey, out var classAccessor))
            {
                var memberAccessor = new MemberAccessor(instance.GetType());
                if (this.OnGetFieldInfes != null)
                {
                    memberAccessor.OnGetFieldInfes = this.OnGetFieldInfes;
                }

                if (this.OnGetProperties != null)
                {
                    memberAccessor.OnGetProperties = this.OnGetProperties;
                }
                memberAccessor.Build();
                classAccessor = memberAccessor;
                this.m_classAccessors.TryAdd(typekey, classAccessor);
            }
            return classAccessor;
        }
    }
}