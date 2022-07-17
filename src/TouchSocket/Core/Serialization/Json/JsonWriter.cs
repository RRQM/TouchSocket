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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TouchSocket.Core.Extensions;

namespace TouchSocket.Core.Serialization
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

        private static void AppendValue(StringBuilder stringBuilder, object item)
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

        private static string GetMemberName(MemberInfo member)
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