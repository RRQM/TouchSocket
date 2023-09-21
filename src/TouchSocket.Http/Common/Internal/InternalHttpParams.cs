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