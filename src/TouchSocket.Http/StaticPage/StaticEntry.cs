using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    public class StaticEntry
    {
        public StaticEntry(byte[] value, TimeSpan timeout)
        {
            this.Value = value;
            this.Timespan = timeout;
        }

        public StaticEntry(FileInfo fileInfo, TimeSpan timespan)
        {
            this.FileInfo = fileInfo;
            this.Timespan = timespan;
        }

        public bool IsCacheBytes => this.Value != null;

        public FileInfo FileInfo { get; set; }
        public byte[] Value { get; }
        public TimeSpan Timespan { get; set; }
    }
}
