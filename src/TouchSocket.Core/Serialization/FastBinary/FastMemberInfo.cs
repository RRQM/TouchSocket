using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    internal class FastMemberInfo
    {
        public byte Index;
        private readonly PropertyInfo m_propertyInfo;
        private readonly FieldInfo m_fieldInfo;
        private readonly bool m_isField;
        public FastMemberInfo(MemberInfo memberInfo, bool enableIndex)
        {
            if (enableIndex)
            {
                if (memberInfo.GetCustomAttribute(typeof(FastMemberAttribute), false) is FastMemberAttribute fastMamberAttribute)
                {
                    this.Index = fastMamberAttribute.Index;
                }
                else
                {
                    throw new Exception($"成员{memberInfo.Name}未标识{nameof(FastMemberAttribute)}特性。");
                }
            }

            if (memberInfo is PropertyInfo propertyInfo)
            {
                this.m_propertyInfo = propertyInfo;
            }
            else if (memberInfo is FieldInfo fieldInfo)
            {
                m_isField = true;
                this.m_fieldInfo = fieldInfo;
            }

        }

        public string Name => this.m_isField ? this.m_fieldInfo.Name : this.m_propertyInfo.Name;

        public Type Type =>this.m_isField? this.m_fieldInfo.FieldType : this.m_propertyInfo.PropertyType;

        public void SetValue(ref object instance, object obj)
        {
            if (this.m_isField)
            {
                this.m_fieldInfo.SetValue(instance, obj);
            }
            else
            {
                this.m_propertyInfo.SetValue(instance, obj);
            }
        }
    }
}
