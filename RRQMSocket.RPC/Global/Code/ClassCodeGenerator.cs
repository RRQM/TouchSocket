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
using RRQMCore;
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 代码辅助类
    /// </summary>
    public class ClassCodeGenerator
    {
        private static readonly string[] dicType = { "Dictionary`2", "IDictionary`2" };
        private static readonly string[] listType = { "List`1", "HashSet`1", "IList`1", "ISet`1", "ICollection`1", "IEnumerable`1" };
        private Assembly assembly;
        private Dictionary<Type, string> genericTypeDic;
        private Dictionary<Type, ClassCellCode> propertyDic;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="assembly"></param>
        public ClassCodeGenerator(Assembly assembly)
        {
            this.assembly = assembly;
            this.propertyDic = new Dictionary<Type, ClassCellCode>();
            this.genericTypeDic = new Dictionary<Type, string>();
        }

        /// <summary>
        /// 程序集
        /// </summary>
        public Assembly Assembly
        {
            get { return assembly; }
        }

        /// <summary>
        /// 获取类单元参数
        /// </summary>
        /// <returns></returns>
        public ClassCellCode[] GetClassCellCodes()
        {
            return this.propertyDic.Values.ToArray();
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
                return this.GetTypeFullName(elementType) + type.Name.Replace(elementType.Name, string.Empty);
            }
            else if (type.IsByRef)
            {
                return this.GetTypeFullName(type.GetElementType());
            }
            else if (type.IsPrimitive || type == typeof(string))
            {
                return type.FullName;
            }
            else if (listType.Contains(type.Name) || dicType.Contains(type.Name))
            {
                return genericTypeDic[type];
            }
            else if (propertyDic.ContainsKey(type))
            {
                return propertyDic[type].Name;
            }
            else
            {
                return type.FullName;
            }

        }

        internal void AddTypeString(Type type)
        {
            if (type.IsByRef)
            {
                type = type.GetRefOutType();
            }

            if (!type.IsPrimitive && type != typeof(string))
            {
                if (type.IsArray)
                {
                    AddTypeString(type.GetElementType());
                }
                else if (type.IsGenericType)
                {
                    Type[] types = type.GetGenericArguments();
                    foreach (Type itemType in types)
                    {
                        AddTypeString(itemType);
                    }

                    if (listType.Contains(type.Name))
                    {
                        string typeInnerString = this.GetTypeFullName(types[0]);
                        string typeString = $"System.Collections.Generic.{type.Name.Replace("`1", string.Empty)}<{typeInnerString}>";
                        if (!genericTypeDic.ContainsKey(type))
                        {
                            genericTypeDic.Add(type, typeString);
                        }
                    }
                    else if (dicType.Contains(type.Name))
                    {
                        string keyString = this.GetTypeFullName(types[0]);
                        string valueString = this.GetTypeFullName(types[1]);
                        string typeString = $"System.Collections.Generic.{type.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>";
                        if (!genericTypeDic.ContainsKey(type))
                        {
                            genericTypeDic.Add(type, typeString);
                        }
                    }
                }
                else if (type.IsInterface || type.IsAbstract)
                {
                    throw new RRQMRPCException("服务参数类型不允许接口或抽象类");
                }
                else if (type.IsEnum)
                {
                    Type baseType = Enum.GetUnderlyingType(type);
                    StringBuilder stringBuilder = new StringBuilder();
                    if (baseType == RRQMReadonly.byteType)
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
                    else if (baseType == RRQMReadonly.shortType)
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
                    else if (baseType == RRQMReadonly.intType)
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
                    else if (baseType == RRQMReadonly.longType)
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
                    if (!propertyDic.ContainsKey(type))
                    {
                        propertyDic.Add(type, new ClassCellCode() { Name = type.Name, Code = stringBuilder.ToString() });
                    }
                }
                else
                {
                    string className;
                    if (type.GetCustomAttribute<RRQMProxyAttribute>() is RRQMProxyAttribute attribute)
                    {
                        className = attribute.ClassName;
                    }
                    else if (CodeGenerator.TryGetProxyTypeName(type, out className))
                    {

                    }
                    else if (type.Assembly == this.assembly)
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
                        AddTypeString(type.BaseType);
                        if (type.BaseType.IsGenericType)
                        {
                            Type[] types = type.BaseType.GetGenericArguments();
                            foreach (Type itemType in types)
                            {
                                AddTypeString(itemType);
                            }
                            if (listType.Contains(type.BaseType.Name))
                            {
                                string typeString = this.GetTypeFullName(types[0]);
                                stringBuilder.Append($":{type.BaseType.Name.Replace("`1", string.Empty)}<{typeString}>");
                            }
                            else if (dicType.Contains(type.BaseType.Name))
                            {
                                string keyString = this.GetTypeFullName(types[0]);
                                string valueString = this.GetTypeFullName(types[1]);
                                stringBuilder.Append($": {type.BaseType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>");
                            }
                        }
                        else if (type.BaseType.IsClass)
                        {
                            stringBuilder.AppendLine($": {this.GetTypeFullName(type.BaseType)}");
                        }
                    }

                    stringBuilder.AppendLine("{");
                    PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty);

                    foreach (PropertyInfo itemProperty in propertyInfos)
                    {
                        AddTypeString(itemProperty.PropertyType);
                        if (propertyDic.ContainsKey(itemProperty.PropertyType))
                        {
                            stringBuilder.Append($"public {itemProperty.PropertyType.Name} {itemProperty.Name}");
                        }
                        else if (itemProperty.PropertyType.IsGenericType)
                        {
                            Type[] types = itemProperty.PropertyType.GetGenericArguments();
                            foreach (Type itemType in types)
                            {
                                AddTypeString(itemType);
                            }

                            if (listType.Contains(itemProperty.PropertyType.Name))
                            {
                                string typeString = this.GetTypeFullName(types[0]);
                                stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`1", string.Empty)}<{typeString}> {itemProperty.Name}");
                            }
                            else if (dicType.Contains(itemProperty.PropertyType.Name))
                            {
                                string keyString = this.GetTypeFullName(types[0]);
                                string valueString = this.GetTypeFullName(types[1]);
                                stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemProperty.Name}");
                            }
                        }
                        else
                        {
                            AddTypeString(itemProperty.PropertyType);
                            stringBuilder.Append($"public {itemProperty.PropertyType.FullName} {itemProperty.Name}");
                        }

                        stringBuilder.AppendLine("{get;set;}");
                    }

                    stringBuilder.AppendLine("}");

                    if (!propertyDic.ContainsKey(type))
                    {
                        propertyDic.Add(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
                    }
                }
            }
        }
    }
}