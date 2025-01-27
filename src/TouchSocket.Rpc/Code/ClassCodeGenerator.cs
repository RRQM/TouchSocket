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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc;

/// <summary>
/// 代码辅助类
/// </summary>
public class ClassCodeGenerator
{
    private static readonly string[] s_dicType = { "Dictionary`2", "IDictionary`2" };
    private static readonly string[] s_listType = { "List`1", "HashSet`1", "IList`1", "ISet`1", "ICollection`1", "IEnumerable`1" };

    private List<string> m_tupleElementNames;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="assembly"></param>
    public ClassCodeGenerator(Assembly[] assembly)
    {
        this.Assembly = assembly;
        this.PropertyDic = new ConcurrentDictionary<Type, ClassCellCode>();
    }

    /// <summary>
    /// 程序集
    /// </summary>
    public Assembly[] Assembly { get; private set; }

    /// <summary>
    /// 属性类型字典。
    /// </summary>
    public ConcurrentDictionary<Type, ClassCellCode> PropertyDic { get; private set; }

    /// <summary>
    /// 添加类型字符串
    /// </summary>
    /// <param name="type"></param>
    public void AddTypeString(Type type)
    {
        var list = new List<Type>();

        this.GetTransmitTypes(type, ref list);

        foreach (var item in list)
        {
            this.PrivateAddTypeString(item);
        }
    }

    /// <summary>
    /// 获取类单元参数
    /// </summary>
    /// <returns></returns>
    public ClassCellCode[] GetClassCellCodes()
    {
        return this.PropertyDic.Values.ToArray();
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
            var ts = type.GetGenericArguments();
            return ts.Length == 1 ? this.GetTypeFullName(ts[0]) : type.Name;
        }
        else if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return this.GetTypeFullName(elementType) + type.Name.Replace(elementType.Name, string.Empty);
        }
        else if (type.IsNullableType())
        {
            return this.GetTypeFullName(type.GetGenericArguments().Length == 0 ? type : type.GetGenericArguments()[0]);
        }
        else if (type.IsValueTuple())
        {
            var elementTypes = type.GetGenericArguments();

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");

            var strings = new List<string>();
            var tupleNames = new List<string>();
            if (this.m_tupleElementNames != null && this.m_tupleElementNames.Count > 0)
            {
                tupleNames.AddRange(this.m_tupleElementNames.Skip(0).Take(elementTypes.Length));
                this.m_tupleElementNames.RemoveRange(0, elementTypes.Length);
            }
            for (var i = 0; i < elementTypes.Length; i++)
            {
                var item = this.GetTypeFullName(elementTypes[i]);
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
            return this.GetTypeFullName(type.GetElementType());
        }
        else if (type.IsPrimitive || type == typeof(string))
        {
            return type.FullName;
        }
        else if (s_listType.Contains(type.Name))
        {
            var typeInnerString = this.GetTypeFullName(type.GetGenericArguments()[0]);
            var typeString = $"System.Collections.Generic.{type.Name.Replace("`1", string.Empty)}<{typeInnerString}>";
            return typeString;
        }
        else if (this.PropertyDic.ContainsKey(type))
        {
            return this.PropertyDic[type].Name;
        }
        else if (type.IsGenericType)
        {
            var typeString = ExtractNonGenericTypeName(type.FullName);
            var typesStrings = type.GetGenericArguments().Select(a => this.GetTypeFullName(a));
            return $"{typeString}<{string.Join(",", typesStrings)}>";
        }
        else
        {
            return type.FullName;
        }

    }

    /// <summary>
    /// 获取类型全名
    /// </summary>
    /// <param name="parameterInfo"></param>
    /// <returns></returns>
    public string GetTypeFullName(ParameterInfo parameterInfo)
    {
        this.m_tupleElementNames = parameterInfo.ParameterType.FullName.Contains("System.ValueTuple") ? (parameterInfo.GetTupleElementNames()?.ToList()) : default;
        return this.GetTypeFullName(parameterInfo.ParameterType);
    }

    /// <summary>
    /// 获取类型全名
    /// </summary>
    /// <param name="propertyInfo"></param>
    /// <returns></returns>
    public string GetTypeFullName(PropertyInfo propertyInfo)
    {
        this.m_tupleElementNames = propertyInfo.PropertyType.FullName.Contains("System.ValueTuple") ? (propertyInfo.GetTupleElementNames()?.ToList()) : default;
        return this.GetTypeFullName(propertyInfo.PropertyType);
    }

    /// <summary>
    /// 获取类型全名
    /// </summary>
    /// <param name="fieldInfo"></param>
    /// <returns></returns>
    public string GetTypeFullName(FieldInfo fieldInfo)
    {
        this.m_tupleElementNames = fieldInfo.FieldType.FullName.Contains("System.ValueTuple") ? (fieldInfo.GetTupleElementNames()?.ToList()) : default;
        return this.GetTypeFullName(fieldInfo.FieldType);
    }

    internal void CheckDeep()
    {
        foreach (var strItem in this.PropertyDic)
        {
            var strItemNew = strItem.Value.Code;
            foreach (var item in this.PropertyDic.Keys)
            {
                if (strItemNew.Contains(item.FullName))
                {
                    strItemNew = strItemNew.Replace(item.FullName, item.Name);
                }
            }
            this.PropertyDic[strItem.Key].Code = strItemNew;
        }
    }

    private void AddType(Type type, string code)
    {
        var className = this.GetClassName(type);
        if (className.IsNullOrEmpty())
        {
            return;
        }
        this.PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = code });
    }

    private bool AllowAssembly(Assembly assembly)
    {
        foreach (var item in this.Assembly)
        {
            if (assembly == item)
            {
                return true;
            }
        }
        return false;
    }

    private string GetClassName(Type type)
    {
        string className;
        if (type.GetCustomAttribute<RpcProxyAttribute>() is RpcProxyAttribute attribute)
        {
            className = attribute.ClassName ?? type.Name;
        }
        else if (CodeGenerator.TryGetProxyTypeName(type, out className))
        {
        }
        else if (this.AllowAssembly(type.Assembly))
        {
            if (type.IsGenericType)
            {
                className = ExtractNonGenericTypeName(type.Name);
                var strings = type.GenericTypeArguments.Select(a => this.GetClassName(a));

                return $"{className}_{string.Join("_", strings)}";
            }
            else
            {
                return type.Name;
            }
        }
        else if (type.IsPrimitive())
        {
            return type.Name;
        }
        else
        {
            return null;
        }
        return className;
    }

    private static string ExtractNonGenericTypeName(string typeName)
    {
        // 检查输入是否为null或空字符串
        if (string.IsNullOrEmpty(typeName))
        {
            throw new ArgumentException("Type name cannot be null or empty.", nameof(typeName));
        }

        // 查找泛型参数开始的反引号位置
        var genericStartIndex = typeName.IndexOf('`');
        if (genericStartIndex == -1)
        {
            // 如果没有找到反引号，说明不是泛型类型，直接返回完整的名字
            return typeName;
        }

        // 提取非泛型部分
        var nonGenericType = typeName.Substring(0, genericStartIndex);
        return nonGenericType;
    }

    private void GetTransmitTypes(Type type, ref List<Type> types)
    {
        if (type.IsByRef)
        {
            type = type.GetRefOutType();
        }

        if (types.Contains(type))
        {
            return;
        }

        if (type.IsPrimitive || type == typeof(string))
        {
            return;
        }

        if (type == typeof(object))
        {
            return;
        }

        if (type.IsInterface || type.IsAbstract)
        {
            return;
        }

        if (type.IsArray)
        {
            this.GetTransmitTypes(type.GetElementType(), ref types);
        }
        else if (type.IsGenericType)
        {
            foreach (var itemType in type.GetGenericArguments())
            {
                this.GetTransmitTypes(itemType, ref types);
            }

            types.Add(type);
        }
        else if (type.IsEnum)
        {
            //添加类型
            types.Add(type);
        }
        else
        {
            //添加类型
            types.Add(type);

            if (type.BaseType != null)
            {
                this.GetTransmitTypes(type.BaseType, ref types);
            }

            foreach (var item in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                this.GetTransmitTypes(item.PropertyType, ref types);
            }

            foreach (var item in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                this.GetTransmitTypes(item.FieldType, ref types);
            }
        }
    }

    private void PrivateAddTypeString(Type type)
    {
        if (CodeGenerator.m_ignoreTypes.Contains(type))
        {
            return;
        }
        if (CodeGenerator.m_ignoreAssemblies.Contains(type.Assembly))
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

        if (type == typeof(object))
        {
            return;
        }
        if (type.IsInterface || type.IsAbstract)
        {
            return;
        }

        var className = this.GetClassName(type);
        if (className.IsNullOrEmpty())
        {
            return;
        }
        var stringBuilder = new StringBuilder();
        if (type.IsEnum)
        {
            var baseType = Enum.GetUnderlyingType(type);

            stringBuilder.AppendLine($"public enum {className}:{baseType.FullName}");
            stringBuilder.AppendLine("{");
            var array = Enum.GetValues(type);
            foreach (var item in array)
            {
                var enumString = item.ToString();
                stringBuilder.AppendLine($"{enumString}={Convert.ToInt64(item)},");
            }
            stringBuilder.AppendLine("}");

            this.AddType(type, stringBuilder.ToString());
        }
        else
        {
            if (type.IsStruct())
            {
                stringBuilder.AppendLine($"public struct {className}");
            }
            else
            {
                stringBuilder.AppendLine($"public class {className}");
                if (type.BaseType != typeof(object))
                {
                    if (type.BaseType.IsGenericType)
                    {
                        var types = type.BaseType.GetGenericArguments();

                        if (s_listType.Contains(type.BaseType.Name))
                        {
                            var typeString = this.GetTypeFullName(types[0]);
                            stringBuilder.Append($":{type.BaseType.Name.Replace("`1", string.Empty)}<{typeString}>");
                        }
                        else if (s_dicType.Contains(type.BaseType.Name))
                        {
                            var keyString = this.GetTypeFullName(types[0]);
                            var valueString = this.GetTypeFullName(types[1]);
                            stringBuilder.Append($": {type.BaseType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}>");
                        }
                    }
                    else if (type.BaseType.IsClass)
                    {
                        stringBuilder.AppendLine($": {this.GetTypeFullName(type.BaseType)}");
                    }
                }
            }

            stringBuilder.AppendLine("{");
            foreach (var itemProperty in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                if (this.PropertyDic.ContainsKey(itemProperty.PropertyType))
                {
                    stringBuilder.Append($"public {itemProperty.PropertyType.Name} {itemProperty.Name}");
                }
                else if (itemProperty.IsNullableType())
                {
                    stringBuilder.Append($"public {this.GetTypeFullName(itemProperty)}? {itemProperty.Name}");
                }
                else if (itemProperty.PropertyType.IsGenericType)
                {
                    var types = itemProperty.PropertyType.GetGenericArguments();
                    if (s_listType.Contains(itemProperty.PropertyType.Name))
                    {
                        var typeString = this.GetTypeFullName(types[0]);
                        stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`1", string.Empty)}<{typeString}> {itemProperty.Name}");
                    }
                    else if (s_dicType.Contains(itemProperty.PropertyType.Name))
                    {
                        var keyString = this.GetTypeFullName(types[0]);
                        var valueString = this.GetTypeFullName(types[1]);
                        stringBuilder.Append($"public {itemProperty.PropertyType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemProperty.Name}");
                    }
                }
                else
                {
                    stringBuilder.Append($"public {this.GetTypeFullName(itemProperty.PropertyType)} {itemProperty.Name}");
                }

                stringBuilder.AppendLine(" { get; set; }");
            }

            foreach (var itemField in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (this.PropertyDic.ContainsKey(itemField.FieldType))
                {
                    stringBuilder.Append($"public {itemField.FieldType.Name} {itemField.Name}");
                }
                else if (itemField.IsNullableType())
                {
                    stringBuilder.Append($"public {this.GetTypeFullName(itemField)}? {itemField.Name}");
                }
                else if (itemField.FieldType.IsGenericType)
                {
                    var types = itemField.FieldType.GetGenericArguments();
                    if (s_listType.Contains(itemField.FieldType.Name))
                    {
                        var typeString = this.GetTypeFullName(types[0]);
                        stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`1", string.Empty)}<{typeString}> {itemField.Name}");
                    }
                    else if (s_dicType.Contains(itemField.FieldType.Name))
                    {
                        var keyString = this.GetTypeFullName(types[0]);
                        var valueString = this.GetTypeFullName(types[1]);
                        stringBuilder.Append($"public {itemField.FieldType.Name.Replace("`2", string.Empty)}<{keyString},{valueString}> {itemField.Name}");
                    }
                }
                else
                {
                    stringBuilder.Append($"public {this.GetTypeFullName(itemField.FieldType)} {itemField.Name}");
                }

                stringBuilder.AppendLine(";");
            }

            stringBuilder.AppendLine("}");

            if (!this.PropertyDic.ContainsKey(type))
            {
                this.PropertyDic.TryAdd(type, new ClassCellCode() { Name = className, Code = stringBuilder.ToString() });
            }
        }
    }
}