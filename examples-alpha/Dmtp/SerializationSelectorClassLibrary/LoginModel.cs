using MemoryPack;

namespace SerializationSelectorClassLibrary
{
    [MemoryPackable]
    public partial class LoginModel
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
}