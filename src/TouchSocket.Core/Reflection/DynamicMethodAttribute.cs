using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Struct| AttributeTargets.Method| AttributeTargets.Interface)]
    public sealed class DynamicMethodAttribute : Attribute
    {
    }
}
