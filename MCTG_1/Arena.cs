namespace MCTG_1;

public class Arena
{

    public List<User> Lobby;

    public Dictionary<int, string> Log;

    public Arena()
    {
        Lobby = new List<User>();
        Log = new Dictionary<int, string>();
    }

    public void AddToLobby(User user)
    {
        Lobby.Add(user);
    }

    public string PrintLog()
    {
        string log = "";
        foreach (KeyValuePair<int, string> entry in Log)
        {
            log += entry.Key + ": " + entry.Value + "\n";
        }
        return log;
    }
    
    public void Battle()
    {
        if (Lobby.Count == 2)
        {
            int result = OnevOne(Lobby[0].Deck, Lobby[1].Deck);
            if (result == 1)
            {
                Log.Add(-1, Lobby[0].Username + " won the battle!");
            }
            else if(result == 2)
            {
                Log.Add(-1, Lobby[1].Username + " won the battle!");
            }
            else
            {
                Log.Add(-1, "It's a draw! Betweeen " + Lobby[0].Username + " and " + Lobby[1].Username + ".");
            }
        }
    }
    
    public int OnevOne(List<Card> deck1, List<Card> deck2)
    {
        //Fight RANDOMLY
        Random rnd = new Random();
        int rnd1 = 0; 
        int rnd2 = 0;
        int rnd3 = 0;
        int rnd4 = 0;
        string result = "";
        List<Card> deadCards = new List<Card>();
        int round = 0;
    
        do
        {
            round++;
            rnd1 = rnd.Next(0, deck1.Count);
            rnd2 = rnd.Next(0, deck2.Count);
            result = CardvsCard(deck1[rnd1], deck2[rnd2]);
            if (result == "Card 1 wins")
            {
                Console.WriteLine("Card 1 wins");
                //Print deck sizes
                
                deadCards.Add(deck2[rnd2]);
                deck2.RemoveAt(rnd2);

                Console.WriteLine("Deck 1: " + deck1.Count);
                Console.WriteLine("Deck 2: " + deck2.Count);
            }else if (result == "Card 2 wins")
            {
                Console.WriteLine("Card 2 wins");
                //Print deck sizes
                deadCards.Add(deck1[rnd1]);
                deck1.RemoveAt(rnd1);

                Console.WriteLine("Deck 1: " + deck1.Count);
                Console.WriteLine("Deck 2: " + deck2.Count);
            }
            else
            {
                //Reincarnate for each player 
                if (deck1.Count == 1 && deck2.Count == 1)
                {
                    if (deadCards.Count > 0)
                    {
                        Console.WriteLine("Reincarnating1...");
                        rnd3 = rnd.Next(0, deadCards.Count);
                        deck1.Add(deadCards[rnd3]);
                        deadCards.RemoveAt(rnd3);
                        if (deadCards.Count > 0)
                        {
                            Console.WriteLine("Reincarnating2...");
                            rnd4 = rnd.Next(0, deadCards.Count);
                            deck2.Add(deadCards[rnd4]);
                            deadCards.RemoveAt(rnd4);
                        }
                    }
                }
                
                Console.WriteLine("Draw!");
            }
        }while(deck2.Count > 0 && deck1.Count > 0 && round < 100);
        Console.WriteLine("Battle finished");
        
        if (round == 100)
        {
            return 0;
        }
        
        if (deck1.Count > 0)
        {
            return 1;
        }
        
        
        return 2;
        
    }

    public string CardvsCard(Card card1, Card card2)
    {
        if(card1.Damage > card2.Damage)
        {
            //Add to Log
            Log.Add(Log.Count, card1.Name + " wins against " + card2.Name);
            return "Card 1 wins";
            
        }
        else if(card1.Damage < card2.Damage)
        {
            //Add to Log
            Log.Add(Log.Count, card2.Name + " wins against " + card1.Name);
            return "Card 2 wins";
            
        }
        else
        {
            Log.Add(Log.Count, "Draw between " + card1.Name + " and " + card2.Name + ". Both players get a random dead card.");
            return "Draw";
        }
    }


}