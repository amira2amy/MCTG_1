// See https://aka.ms/new-console-template for more information

using System;

namespace MCTG_1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Interaction interaction = new Interaction();
            interaction.SetCoins();
            Server server = new Server();
            server.StartServer();
        }
    }
}