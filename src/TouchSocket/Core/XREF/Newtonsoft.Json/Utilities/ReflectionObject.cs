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

#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

#endregion License

using TouchSocket.Core.XREF.Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

#if !HAVE_LINQ

using TouchSocket.Core.XREF.Newtonsoft.Json.Utilities.LinqBridge;

#else
using System.Linq;

#endif

namespace TouchSocket.Core.XREF.Newtonsoft.Json.Utilities
{
    internal class ReflectionMember
    {
        public Type MemberType { get; set; }
        public Serialization.Func<object, object> Getter { get; set; }
        public Serialization.Action<object, object> Setter { get; set; }
    }

    internal class ReflectionObject
    {
        public ObjectConstructor<object> Creator { get; }
        public IDictionary<string, ReflectionMember> Members { get; }

        private ReflectionObject(ObjectConstructor<object> creator)
        {
            this.Members = new Dictionary<string, ReflectionMember>();
            this.Creator = creator;
        }

        public object GetValue(object target, string member)
        {
            Serialization.Func<object, object> getter = this.Members[member].Getter;
            return getter(target);
        }

        public void SetValue(object target, string member, object value)
        {
            Serialization.Action<object, object> setter = this.Members[member].Setter;
            setter(target, value);
        }

        public Type GetType(string member)
        {
            return this.Members[member].MemberType;
        }

        public static ReflectionObject Create(Type t, params string[] memberNames)
        {
            return Create(t, null, memberNames);
        }

        public static ReflectionObject Create(Type t, MethodBase creator, params string[] memberNames)
        {
            ReflectionDelegateFactory delegateFactory = JsonTypeReflector.ReflectionDelegateFactory;

            ObjectConstructor<object> creatorConstructor = null;
            if (creator != null)
            {
                creatorConstructor = delegateFactory.CreateParameterizedConstructor(creator);
            }
            else
            {
                if (ReflectionUtils.HasDefaultConstructor(t, false))
                {
                    Serialization.Func<object> ctor = delegateFactory.CreateDefaultConstructor<object>(t);

                    creatorConstructor = args => ctor();
                }
            }

            ReflectionObject d = new ReflectionObject(creatorConstructor);

            foreach (string memberName in memberNames)
            {
                MemberInfo[] members = t.GetMember(memberName, BindingFlags.Instance | BindingFlags.Public);
                if (members.Length != 1)
                {
                    throw new ArgumentException("Expected a single member with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, memberName));
                }

                MemberInfo member = members.Single();

                ReflectionMember reflectionMember = new ReflectionMember();

                switch (member.MemberType())
                {
                    case MemberTypes.Field:
                    case MemberTypes.Property:
                        if (ReflectionUtils.CanReadMemberValue(member, false))
                        {
                            reflectionMember.Getter = delegateFactory.CreateGet<object>(member);
                        }

                        if (ReflectionUtils.CanSetMemberValue(member, false, false))
                        {
                            reflectionMember.Setter = delegateFactory.CreateSet<object>(member);
                        }
                        break;

                    case MemberTypes.Method:
                        MethodInfo method = (MethodInfo)member;
                        if (method.IsPublic)
                        {
                            ParameterInfo[] parameters = method.GetParameters();
                            if (parameters.Length == 0 && method.ReturnType != typeof(void))
                            {
                                MethodCall<object, object> call = delegateFactory.CreateMethodCall<object>(method);
                                reflectionMember.Getter = target => call(target);
                            }
                            else if (parameters.Length == 1 && method.ReturnType == typeof(void))
                            {
                                MethodCall<object, object> call = delegateFactory.CreateMethodCall<object>(method);
                                reflectionMember.Setter = (target, arg) => call(target, arg);
                            }
                        }
                        break;

                    default:
                        throw new ArgumentException("Unexpected member type '{0}' for member '{1}'.".FormatWith(CultureInfo.InvariantCulture, member.MemberType(), member.Name));
                }

                if (ReflectionUtils.CanReadMemberValue(member, false))
                {
                    reflectionMember.Getter = delegateFactory.CreateGet<object>(member);
                }

                if (ReflectionUtils.CanSetMemberValue(member, false, false))
                {
                    reflectionMember.Setter = delegateFactory.CreateSet<object>(member);
                }

                reflectionMember.MemberType = ReflectionUtils.GetMemberUnderlyingType(member);

                d.Members[memberName] = reflectionMember;
            }

            return d;
        }
    }
}