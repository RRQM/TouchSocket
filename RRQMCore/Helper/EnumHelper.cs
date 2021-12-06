using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore.Helper
{
    /// <summary>
    /// 枚举
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 获取自定义attribute 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumObj"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum enumObj) where T : Attribute
        {
            Type type = enumObj.GetType();
            Attribute attr = null;
            string enumName = Enum.GetName(type, enumObj);  //获取对应的枚举名
            FieldInfo field = type.GetField(enumName);
            attr = field.GetCustomAttribute(typeof(T), false);
            return (T)attr;
        }
    }
}
