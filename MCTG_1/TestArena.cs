using NUnit.Framework;

namespace MCTG_1;

[TestFixture]
public class TestArena
{
    public Arena arena;
    
    public TestArena()
    {
        arena = new Arena();
    }

    [Test]
    public void TestCardvsCard()
    {
        Card card1 = new Card();
        Card card2 = new Card();
        card1.Damage = 10;
        card2.Damage = 5;
        
        Assert.AreEqual("Card 1 wins", arena.CardvsCard(card1, card2));
    }

    [Test]
    public void TestCardvsCardLoss()
    {
        Card card1 = new Card();
        Card card2 = new Card();
        card1.Damage = 5;
        card2.Damage = 10;
        
        Assert.AreEqual("Card 2 wins", arena.CardvsCard(card1, card2));
    }

    [Test]
    public void TestAddToLobby()
    {
        User user = new User();
        arena.AddToLobby(user);
        
        Assert.AreEqual(1, arena.Lobby.Count);
    }
}