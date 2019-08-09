using System;


// dotnet publish --self-contained -c Release -r win10-x64
namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {

            Server server = new Server();

            server.RunServer();
        }
    }
}
