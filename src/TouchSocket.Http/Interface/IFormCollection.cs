using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http
{
    public interface IFormCollection : IEnumerable<KeyValuePair<string, string>>
    {
        int Count { get; }
        IMultifileCollection Files { get; }
        ICollection<string> Keys { get; }
        string this[string key] { get; }
        string Get(string key);

        bool ContainsKey(string key);

        bool TryGetValue(string key, out string value);
    }
}