using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    internal class InternalHttpHeader : Dictionary<string, string>, IHttpHeader
    {
        public InternalHttpHeader() : base(StringComparer.OrdinalIgnoreCase)
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

        public string this[HttpHeaders headers]
        {
            get
            {
                if (this.TryGetValue(headers.GetDescription(), out var value))
                {
                    return value;
                }
                return null;
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

        public string Get(HttpHeaders key)
        {
            if (this.TryGetValue(key.GetDescription(), out var value))
            {
                return value;
            }
            return null;
        }
    }
}
