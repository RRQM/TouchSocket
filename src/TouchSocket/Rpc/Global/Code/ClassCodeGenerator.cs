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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 代码辅助类
    /// </summary>
    public class ClassCodeGenerator
    {
        private static readonly string[] m_dicType = { "Dictionary`2", "IDictionary`2" };
        private static readonly string[] m_listType = { "List`1", "HashSet`1", "IList`1", "ISet`1", "ICollection`1", "IEnumerable`1" };
        private readonly Assembly[] m_assembly;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assembly"></param>
        public ClassCodeGenerator(Assembly[] assembly)
        {
            m_assembly = assembly;
            PropertyDic = new ConcurrentDictionary<Type, ClassCellCode>();
            //GenericTypeDic = new ConcurrentDictionary<Type, string>();
        }

        /// <summary>
        /// 程序集
        /// </summary>
        public Assembly[] Assembly => m_assembly;

        ///// <summary>
        ///// 泛型类型字典
        ///// </summary>
        //public ConcurrentDictionary<Type, string> GenericTypeDic { get; private set; }

        /// <summary>
        /// 属性类型字典。
        /// </summary>
        public ConcurrentDictionary<Type, ClassCellCode> PropertyDic { get; private set; }

        /// <summary>
        /// 获取类单元参数
        /// </summary>
        /// <returns></returns>
        public ClassCellCode[] GetClassCellCodes()
        {
            return PropertyDic.Values.ToArray();
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTypeFullName(Type type)
        {
            if (type.FullName == null)
            {
                return type.Name.Replace("&", string.Empty);
            }
            else if (type == typeof(void))
            {
                return null;
            }
            else if (typeof(Task).IsAssignableFrom(type))
            {
                Type[] ts = type.GetGenericArguments();
                if (ts.Length == 1)
                {
                    return ts[0].Name;
                }
                else
                {
                    return type.Name;
                }
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                return GetTypeFullName(elementType) + type.Name.Replace(elementType.Name, string.Empty);
            }
            else if (type.IsNullableType())
            {
                return GetTypeFullName(type.GetGenericArguments().Length==0? type: type.GetGenericArguments()[0]);
            }
            else if (type.IsValueTuple())
            {
                Type[] elementTypes = type.GetGenericArguments();

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("(");

                List<string> strings = new List<string>();
                var tupleNames = new List<string>();
                if (tupleElementNames != null && tupleElementNames.Count > 0)
                {
                    tupleNames.AddRange(tupleElementNames.Skip(0).Take(elementTypes.Length));
                    tupleElementNames.RemoveRange(0, elementTypes.Length);
                }
                for (int i = 0; i < elementTypes.Length; i++)
                {
                    string item = GetTypeFullName(elementTypes[i]);
                    if (tupleNames.Count > 0)
                    {
                        strings.Add($"{item} {tupleNames[i]}");
                    }
                    else
                    {
                        strings.Add($"{item}");
                    }
                }
                //var strs = elementTypes.Select(e => GetTypeFullName(e));


                //foreach (var item in strs)
                //{

                //}
                stringBuilder.Append(string.Join(",", strings));
                stringBuilder.Append(")");
                return stringBuilder.ToString();
            }
            else if (type.IsByRef)
            {
                return GetTypeFullName(type.GetElementType());
            }
            else if (type.IsPrimitive || type == typeof(string))
            {
                return type.FullName;
            }
            else if (m_listType.Contains(type.Name))
            {
                string typeInnerString = GetTypeFullName(type.GetGenericArguments()[0]);
                string typeString = $"System.Collections.Generic.{type.Name.Replace("`1", string.Empty)}<{typeInnerString}>";
                return typeString;
            }
            else if (m_listType.Contains(type.Name) || m_dicType.Contains(type.Name))
            {
                string keyString = GetTypeFullName(type.GetGenericArguments()[0]);
                string valueString = GetTypeFullName(type.GetGenericArguments()[1]);
                string typeString = $"System.Collections.Generic.{type.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>";
                return typeString;
            }
            else if (PropertyDic.ContainsKey(type))
            {
                return PropertyDic[type].Name;
            }
            else
            {
                return type.FullName;
            }
        }

        private List<string> tupleElementNames;
        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(ParameterInfo parameterInfo)
        {
            if (parameterInfo.ParameterType.FullName.Contains("System.ValueTuple"))
            {
                tupleElementNames = parameterInfo.GetTupleElementNames()?.ToList();
            }
            else
            {
                tupleElementNames = default;
            }
            return GetTypeFullName(parameterInfo.ParameterType);
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(PropertyInfo  propertyInfo)
        {
            if (propertyInfo.PropertyType.FullName.Contains("System.ValueTuple"))
            {
                tupleElementNames = propertyInfo.GetTupleElementNames()?.ToList();
            }
            else
            {
                tupleElementNames = default;
            }
            return GetTypeFullName(propertyInfo.PropertyType);
        }

        /// <summary>
        /// 获取类型全名
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public string GetTypeFullName(FieldInfo fieldInfo)
        {
            if (fieldInfo.FieldType.FullName.Contains("System.ValueTuple"))
            {
                tupleElementNames = fieldInfo.GetTupleElementNames()?.ToList();
            }
            else
            {
                tupleElementNames = default;
            }
            return GetTypeFullName(fieldInfo.FieldType);
        }

        internal void CheckDeep()
        {
            //foreach (var strItem in GenericTypeDic)
            //{
            //    bool goon = true;
            //    string strItemNew = strItem.Value;
            //    while (goon)
            //    {
            //        goon = false;
            //        foreach (var item in GenericTypeDic.Keys)
            //        {
            //            if (strItemNew.Contains(item.FullName))
            //            {
            //                strItemNew = strItemNew.Replace(item.FullName, item.Name);
            //                goon = true;
            //            }
            //        }
            //    }
            //    GenericTypeDic[strItem.Key] = strItemNew;
            //}

            foreach (var strItem in PropertyDic)
            {
                bool goon = true;
                string strItemNew = strItem.Value.Code;
                while (goon)
                {
                    goon = false;
                    foreach (var item in PropertyDic.Keys)
                    {
                        if (strItemNew.Contains(item.FullName))
                        {
                            strItemNew = strItemNew.Replace(item.FullName, item.Name);
                            goon = true;
                        }
                    }
                }
                PropertyDic[strItem.Key].Code = strItemNew;
            }
        }

        /// <summary>
        /// 添加类型字符串
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deep"></param>
        public void AddTypeString(Type type, ref int deep)
        {
            if (CodeGenerator.m_ignoreTypes.Contains(type))
            {
                return;
            }
            if (CodeGenerator.m_ignoreAssemblies.Contains(type.Assembly))
            {
                return;
            }
            deep++;
            if (deep > 50)
            {
                return;
            }
            if (type.IsByRef)
            {
                type = type.GetRefOutType();
            }

            if (type.IsPrimitive || type == typeof(string))
            {
                return;
            }

            if (type == TouchSocketCoreUtility.objType)
            {
                return;
            }
            if (type.IsInterface || type.IsAbstract)
            {
                return;
            }
            if (type.IsArray)
            {
                AddTypeString(type.GetElementType(), ref deep);
            }
            else if (type.IsGenericType)
            {
                Type[] types = type.GetGenericArguments();
                foreach (Type itemType in types)
                {
                    AddTypeString(itemType, ref deep);
                }
            }
            else if (type.IsEnum)
            {
                Type baseType = Enum.GetUnderlyingType(type);
                StringBuilder stringBuilder = new StringBuilder();
                if (baseType == TouchSocketCoreUtility.byteType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:byte");
                    stringBuilder.AppendLine("{");
                    Array array = Enum.GetValues(type);
                    foreach (object item in array)
                    {
                        string enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(byte)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.shortType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:short");
                    stringBuilder.AppendLine("{");
                    Array array = Enum.GetValues(type);
                    foreach (object item in array)
                    {
                        string enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(short)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.intType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:int");
                    stringBuilder.AppendLine("{");
                    Array array = Enum.GetValues(type);
                    foreach (object item in array)
                    {
                        string enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(int)item},");
                    }
                }
                else if (baseType == TouchSocketCoreUtility.longType)
                {
                    stringBuilder.AppendLine($"public enum {type.Name}:long");
                    stringBuilder.AppendLine("{");
                    Array array = Enum.GetValues(type);
                    foreach (object item in array)
                    {
                        string enumString = item.ToString();
                        stringBuilder.AppendLine($"{enumString}={(long)item},");
                    }
                }

                stringBuilder.AppendLine("}");
                if (!PropertyDic.ContainsKey(type))
                {
                    string className;
                    if (type.GetCustomAttribute<RpcProxyAttribute>() is RpcProxyAttribute attribute)
                    {
                        className = attribute.ClassName ?? type.Name;
                    }
                    else if (CodeGenerator.TryGetProxyTypeName(type, out className))
                    {
                    }
                    else if (AllowGen(type.Assembly))
                    {
                        className = type.Name;
                    }
                    else
                    {
                        return;
                    }
                    PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
                }
            }
            else
            {
                string className;
                if (type.GetCustomAttribute<RpcProxyAttribute>() is RpcProxyAttribute attribute)
                {
                    className = attribute.ClassName ?? type.Name;
                }
                else if (CodeGenerator.TryGetProxyTypeName(type, out className))
                {
                }
                else if (AllowGen(type.Assembly))
                {
                    className = type.Name;
                }
                else
                {
                    return;
                }
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("");
                if (type.IsStruct())
                {
                    stringBuilder.AppendLine($"public struct {className}");
                }
                else
                {
                    stringBuilder.AppendLine($"public class {className}");
                }

                if (!type.IsStruct() && type.BaseType != typeof(object))
                {
                    AddTypeString(type.BaseType, ref deep);
                    if (type.BaseType.IsGenericType)
                    {
                        Type[] types = type.BaseType.GetGenericArguments();
                        foreach (Type itemType in types)
                        {
                            AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(type.BaseType.Name))
                        {
                            string typeString = GetTypeFullName(types[0]);
                            stringBuilder.Append($":{type.BaseType.Name.Replace("`1", string.Empty)}<{typeString}>");
                        }
                        else if (m_dicType.Contains(type.BaseType.Name))
                        {
                            string keyString = GetTypeFullName(types[0]);
                            string valueString = GetTypeFullName(types[1]);
                            stringBuilder.Append($": {type.BaseType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>");
                        }
                    }
                    else if (type.BaseType.IsClass)
                    {
                        stringBuilder.AppendLine($": {GetTypeFullName(type.BaseType)}");
                    }
                }

                stringBuilder.AppendLine("{");
                foreach (PropertyInfo itemProperty in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
                {
                    AddTypeString(itemProperty.PropertyType, ref deep);
                    if (PropertyDic.ContainsKey(itemProperty.PropertyType))
                    {
                        stringBuilder.Append($"public {itemProperty.PropertyType.Name} {itemProperty.Name}");
                    }
                    else if (itemProperty.IsNullableType())
                    {
                        stringBuilder.Append($"public {GetTypeFullName(itemProperty)}? {itemProperty.Name}");
                    }
                    else if (itemProperty.PropertyType.IsGenericType)
                    {
                        Type[] types = itemProperty.PropertyType.GetGenericArguments();
                        foreach (Type itemType in types)
                        {
                            AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(itemProperty.PropertyType.Name))
                        {
                            string typeString = GetTypeFullName(types[0]);
                            stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`1", string.Empty)}<{typeString}> {itemProperty.Name}");
                        }
                        else if (m_dicType.Contains(itemProperty.PropertyType.Name))
                        {
                            string keyString = GetTypeFullName(types[0]);
                            string valueString = GetTypeFullName(types[1]);
                            stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemProperty.Name}");
                        }
                    }
                    else
                    {
                        AddTypeString(itemProperty.PropertyType, ref deep);
                        stringBuilder.Append($"public {GetTypeFullName(itemProperty.PropertyType)} {itemProperty.Name}");
                    }

                    stringBuilder.AppendLine("{get;set;}");
                }

                foreach (FieldInfo itemField in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    AddTypeString(itemField.FieldType, ref deep);
                    if (PropertyDic.ContainsKey(itemField.FieldType))
                    {
                        stringBuilder.Append($"public {itemField.FieldType.Name} {itemField.Name}");
                    }
                    else if (itemField.IsNullableType())
                    {
                        stringBuilder.Append($"public {GetTypeFullName(itemField)}? {itemField.Name}");
                    }
                    else if (itemField.FieldType.IsGenericType)
                    {
                        Type[] types = itemField.FieldType.GetGenericArguments();
                        foreach (Type itemType in types)
                        {
                            AddTypeString(itemType, ref deep);
                        }
                        if (m_listType.Contains(itemField.FieldType.Name))
                        {
                            string typeString = GetTypeFullName(types[0]);
                            stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`1", string.Empty)}<{typeString}> {itemField.Name}");
                        }
                        else if (m_dicType.Contains(itemField.FieldType.Name))
                        {
                            string keyString = GetTypeFullName(types[0]);
                            string valueString = GetTypeFullName(types[1]);
                            stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemField.Name}");
                        }
                    }
                    else
                    {
                        AddTypeString(itemField.FieldType, ref deep);
                        stringBuilder.Append($"public {GetTypeFullName(itemField.FieldType)} {itemField.Name}");
                    }

                    stringBuilder.AppendLine(";");
                }

                stringBuilder.AppendLine("}");

                if (!PropertyDic.ContainsKey(type))
                {
                    PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
                }
            }
        }

        private bool AllowGen(Assembly assembly)
        {
            foreach (var item in m_assembly)
            {
                if (assembly == item)
                {
                    return true;
                }
            }
            return false;
        }
    }
}