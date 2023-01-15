namespace MCTG_1;

public class User
{
    public User()
    {
        Deck = new List<Card>();
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public int Money { get; set; } = 20;
    public int Elo { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }
    public string Image { get; set; }

    public List<Card> Deck { get; set; }
}