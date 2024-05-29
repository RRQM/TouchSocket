using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    class MemCacheEntry
    {
        public byte[] Value { get; }
        public TimeSpan Timespan { get; private set; }

        public MemCacheEntry(byte[] value, TimeSpan timespan = new TimeSpan())
        {
            this.Value = value;
            this.Timespan = timespan;
        }

        public MemCacheEntry(string value, TimeSpan timespan = new TimeSpan())
        {
            this.Value = Encoding.UTF8.GetBytes(value);
            this.Timespan = timespan;
        }
    };

}
