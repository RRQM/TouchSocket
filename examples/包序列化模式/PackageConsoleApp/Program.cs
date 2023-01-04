using TouchSocket.Core;

namespace PackageConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    class MyClass : IPackage
    {
        public void Package(ByteBlock byteBlock)
        {
            throw new NotImplementedException();
        }

        public void Unpackage(ByteBlock byteBlock)
        {
            throw new NotImplementedException();
        }
    }
}