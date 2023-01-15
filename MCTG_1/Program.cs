// See https://aka.ms/new-console-template for more information

namespace MCTG_1;

internal class Program
{
    private static void Main(string[] args)
    {
        var interaction = new Interaction();
        interaction.SetCoins();
        var server = new Server();
        interaction.DeleteAllDecks();
        interaction.DeleteAllCards();
        server.StartServer();
    }
}