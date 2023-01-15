using NUnit.Framework;

namespace MCTG_1;

[TestFixture]
public class TestInteraction
{
    private Interaction interaction;

    public TestInteraction()
    {
        interaction = new Interaction();
    }

    [Test]
    public void TestGetUserID()
    {
        Assert.AreEqual(3, interaction.GetUserID("test"));
    }

    [Test]
    public void TestGetUserCoins()
    {
        User user = new User();
        user.Username = "test";
        Assert.AreEqual(20, interaction.GetUserCoins(user));
    }

    [Test]
    public void TestUpdateCoins()
    {
        User user = new User();
        user.Username = "test";
        interaction.UpdateCoins(user);
        Assert.AreEqual(15, interaction.GetUserCoins(user));
        interaction.SetCoins();
    }

    [Test]
    public void TestUserExistsTrue()
    {
        Assert.IsTrue(interaction.UserExists("test"));
    }

    [Test]
    public void TestUserExistsFalse()
    {
        Assert.IsFalse(interaction.UserExists("testFalse"));
    }

    [Test]
    public void TestLoginTrue()
    {
        User user = new User();
        user.Username = "test";
        user.Password = "test";
        Assert.IsTrue(interaction.Login(user));
    }
    
    [Test]
    public void TestLoginFalse()
    {
        User user = new User();
        user.Username = "test";
        user.Password = "testFalse";
        Assert.IsFalse(interaction.Login(user));
    }

    [Test, Order(1)]
    public void TestGetElo()
    {
        User user = new User();
        user.Username = "test";
        
        Assert.AreEqual(100, interaction.GetElo(user));
    }

    [Test, Order(2)]
    public void TestGetEloAndUserName()
    {
        Assert.AreEqual("test: 100", interaction.GetEloAndUsername(3));
    }

    [Test, Order(3)]
    public void TestUpdateWinner()
    {
        User user = new User();
        user.Username = "test";
        interaction.UpdateWinner(user.Username);
        Assert.AreEqual(103, interaction.GetElo(user));
    }

    [Test]
    public void TestUpdateLoser()
    {
        User user = new User();
        user.Username = "test";
        interaction.UpdateLoser(user.Username);
        Assert.AreEqual(98, interaction.GetElo(user));
    }

    [Test]
    public void TestRegister()
    {
        User user = new User();
        user.Username = "testRegister";
        user.Password = "testRegister";
        Assert.IsTrue(interaction.RegisterUser(user));
    }


    [Test]
    public void TestRegisterFalse()
    {
        User user = new User();
        user.Username = "testRegister";
        user.Password = "testRegister";
        Assert.IsFalse(interaction.RegisterUser(user));
      
    }

    [Test]
    public void TestSaveDeck()
    {
        User user = new User();
        user.Username = "test";
        
        Card card1 = new Card();
        card1.Name = "testCard1";
        card1.Damage = 10;
        card1.Id = "1";
        Card card2 = new Card();
        card2.Name = "testCard2";
        card2.Damage = 10;
        card2.Id = "2";
        
        List<Card> cards = new List<Card>();
      
        cards.Add(card1);
        cards.Add(card2);
        
        interaction.SaveDeck(cards, user.Username);
        Assert.AreEqual(2, interaction.GetCardsAmountFromDeck(user.Username));
    }


    [Test]
    public void DeleteDeck()
    {
        User user = new User();
        user.Username = "test";
        interaction.DeleteDeckFromUser(user.Username);
        Assert.AreEqual(0, interaction.GetCardsAmountFromDeck(user.Username));
    }

    [Test]
    public void TestTradeBelongsToUser()
    {
        Trade trade = new Trade();
        trade.Id = "testTrade";
        trade.CardToTrade = "testCard";
        trade.Type = "monster";
        trade.MinimumDamage = 10;
        trade.username = "test";

        interaction.UploadTrade(trade);
        Assert.IsTrue(interaction.TradeBelongsToUser(trade.Id, trade.username));
    }

    [Test]
    public void TestDeleteUser()
    {
        User user = new User();
        user.Username = "delreg";
        user.Password = "delreg";
        
        interaction.RegisterUser(user);
        Assert.IsTrue(interaction.DeleteUser(user.Username));
    }
    
}