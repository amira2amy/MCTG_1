namespace MCTG_1;

public class CardHandler
{
    public List<Card> baughtCards;
    public List<List<Card>> packages;

    public CardHandler()
    {
        packages = new List<List<Card>>();
        baughtCards = new List<Card>();
    }
}