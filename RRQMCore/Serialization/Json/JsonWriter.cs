using RRQMCore.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace RRQMCore.Serialization
{
    /// <summary>
    /// Json解析器。
    /// </summary>
    public static class JsonWriter
    {
        /// <summary>
        /// 转换为Json
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string ToJson(this object item)
        {
            StringBuilder stringBuilder = new StringBuilder();
            AppendValue(stringBuilder, item);
            return stringBuilder.ToString();
        }

        static void AppendValue(StringBuilder stringBuilder, object item)
        {
            if (item == null)
            {
                stringBuilder.Append("null");
                return;
            }

            Type type = item.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    stringBuilder.Append(((bool)item) ? "true" : "false");
                    break;
                case TypeCode.String:
                case TypeCode.Char:
                    {
                        stringBuilder.Append('"');
                        string str = item.ToString();
                        for (int i = 0; i < str.Length; ++i)
                        {
                            if (str[i] < ' ' || str[i] == '"' || str[i] == '\\')
                            {
                                stringBuilder.Append('\\');
                                int j = "\"\\\n\r\t\b\f".IndexOf(str[i]);
                                if (j >= 0)
                                    stringBuilder.Append("\"\\nrtbf"[j]);
                                else
                                    stringBuilder.AppendFormat("u{0:X4}", (uint)str[i]);
                            }
                            else
                            {
                                stringBuilder.Append(str[i]);
                            }
                        }

                        stringBuilder.Append('"');
                        break;
                    }
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    {
                        stringBuilder.Append(item.ToString());
                    }
                    break;
                case TypeCode.Single:
                    stringBuilder.Append(((float)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Double:
                    stringBuilder.Append(((double)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Decimal:
                    stringBuilder.Append(((decimal)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case TypeCode.DateTime:
                    stringBuilder.Append('"');
                    stringBuilder.Append(((DateTime)item).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    stringBuilder.Append('"');
                    break;
                case TypeCode.Empty:
                case TypeCode.Object:
                default:
                    {
                        if (type == typeof(byte[]))
                        {
                            stringBuilder.Append('"');
                            stringBuilder.Append(((byte[])item).ToBase64());
                            stringBuilder.Append('"');
                        }
                        else if (type.IsEnum)
                        {
                            stringBuilder.Append('"');
                            stringBuilder.Append(item.ToString());
                            stringBuilder.Append('"');
                        }
                        else if (item is IList)
                        {
                            stringBuilder.Append('[');
                            bool isFirst = true;
                            IList list = item as IList;
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (isFirst)
                                    isFirst = false;
                                else
                                    stringBuilder.Append(',');
                                AppendValue(stringBuilder, list[i]);
                            }
                            stringBuilder.Append(']');
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        {
                            Type keyType = type.GetGenericArguments()[0];

                            //Refuse to output dictionary keys that aren't of type string
                            if (keyType != typeof(string))
                            {
                                stringBuilder.Append("{}");
                                return;
                            }

                            stringBuilder.Append('{');
                            IDictionary dict = item as IDictionary;
                            bool isFirst = true;
                            foreach (object key in dict.Keys)
                            {
                                if (isFirst)
                                    isFirst = false;
                                else
                                    stringBuilder.Append(',');
                                stringBuilder.Append('\"');
                                stringBuilder.Append((string)key);
                                stringBuilder.Append("\":");
                                AppendValue(stringBuilder, dict[key]);
                            }
                            stringBuilder.Append('}');
                        }
                        else
                        {
                            stringBuilder.Append('{');

                            bool isFirst = true;
                            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            for (int i = 0; i < fieldInfos.Length; i++)
                            {
                                if (fieldInfos[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
                                    continue;

                                object value = fieldInfos[i].GetValue(item);
                                if (value != null)
                                {
                                    if (isFirst)
                                        isFirst = false;
                                    else
                                        stringBuilder.Append(',');
                                    stringBuilder.Append('\"');
                                    stringBuilder.Append(GetMemberName(fieldInfos[i]));
                                    stringBuilder.Append("\":");
                                    AppendValue(stringBuilder, value);
                                }
                            }
                            PropertyInfo[] propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            for (int i = 0; i < propertyInfo.Length; i++)
                            {
                                if (!propertyInfo[i].CanRead || propertyInfo[i].IsDefined(typeof(IgnoreDataMemberAttribute), true))
                                    continue;

                                object value = propertyInfo[i].GetValue(item, null);
                                if (value != null)
                                {
                                    if (isFirst)
                                        isFirst = false;
                                    else
                                        stringBuilder.Append(',');
                                    stringBuilder.Append('\"');
                                    stringBuilder.Append(GetMemberName(propertyInfo[i]));
                                    stringBuilder.Append("\":");
                                    AppendValue(stringBuilder, value);
                                }
                            }

                            stringBuilder.Append('}');
                        }
                    }
                    break;
            }

        }

        static string GetMemberName(MemberInfo member)
        {
            if (member.IsDefined(typeof(DataMemberAttribute), true))
            {
                DataMemberAttribute dataMemberAttribute = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute), true);
                if (!string.IsNullOrEmpty(dataMemberAttribute.Name))
                    return dataMemberAttribute.Name;
            }

            return member.Name;
        }
    }
}
