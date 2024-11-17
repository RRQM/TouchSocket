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
using System.Collections.Generic;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    internal sealed class InternalHttpHeader : Dictionary<string, string>, IHttpHeader
    {
        public InternalHttpHeader() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public new string this[string key]
        {
            get
            {
                return key == null ? null : this.TryGetValue(key, out var value) ? value : null;
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

        public string this[HttpHeaders headers]
        {
            get
            {
                return this.TryGetValue(headers.GetDescription(), out var value) ? value : null;
            }

            set
            {
                this.AddOrUpdate(headers.GetDescription(), value);
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

        public void Add(HttpHeaders key, string value)
        {
            this.AddOrUpdate(key.GetDescription(), value);
        }

        public string Get(string key)
        {
            return key == null ? null : this.TryGetValue(key, out var value) ? value : null;
        }

        public string Get(HttpHeaders key)
        {
            return this.TryGetValue(key.GetDescription(), out var value) ? value : null;
        }
    }
}