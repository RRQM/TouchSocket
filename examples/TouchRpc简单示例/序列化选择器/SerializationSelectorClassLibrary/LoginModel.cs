using MemoryPack;
using System;

namespace SerializationSelectorClassLibrary
{
    [MemoryPackable]
    public partial class LoginModel
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
}
