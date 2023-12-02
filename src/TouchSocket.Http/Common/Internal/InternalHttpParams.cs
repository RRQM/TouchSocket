//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    internal class InternalHttpParams : Dictionary<string, string>, IHttpParams
    {
        public InternalHttpParams() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public new string this[string key]
        {
            get
            {
                if (key == null)
                {
                    return null;
                }
                if (this.TryGetValue(key, out var value))
                {
                    return value;
                }
                return null;
            }

            set
            {
                if (key == null)
                {
                    return;
                }

                this.AddOrUpdate(key, value);
            }
        }

        public new void Add(string key, string value)
        {
            if (key == null)
            {
                return;
            }
            this.AddOrUpdate(key, value);
        }

        public string Get(string key)
        {
            if (key == null)
            {
                return null;
            }
            if (this.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
    }
}