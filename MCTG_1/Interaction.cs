using Npgsql;
using NpgsqlTypes;

namespace MCTG_1;

public class Interaction
{
    public NpgsqlConnection conn;

    public Interaction()
    {
        conn = new NpgsqlConnection(Conn.ConnectionString);
        conn.Open();
    }

    public int GetUserID(string username)
    {
        var id = -1;
        using (var con = new NpgsqlConnection(Conn.ConnectionString))
        {
            con.Open();
            using (var cmd = new NpgsqlCommand("SELECT id FROM mtcg_db.public.user WHERE username = @username;", con))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                while (reader.Read()) id = reader.GetInt32(0);
            }
        }


        return id;
    }

    public void InsertCard(Card card, User currentUser)
    {
        //insert card into database
        /*using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO mtcg_db.public.deck (card_id, card_name, card_damage, user_fk) VALUES (@card_id, @card_name, @card_damage, @user_fk)";
            cmd.Parameters.AddWithValue("card_id", card.ID);
            cmd.Parameters.AddWithValue("card_name", card.Name);
            cmd.Parameters.AddWithValue("card_damage", (object)damage);
            cmd.Parameters.AddWithValue("user_fk", GetUserID(currentUser));
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }*/
        var id = GetUserID(currentUser.Username);
        var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText =
            "INSERT INTO mtcg_db.public.deck (card_id, card_name, card_damage, user_fk) VALUES (@card_id, @card_name, @card_damage, @user_fk)";
        cmd.Parameters.Add("card_id", NpgsqlDbType.Integer).Value = card.Id;
        cmd.Parameters.Add("card_name", NpgsqlDbType.Varchar, 100).Value = card.Name;
        // ReSharper disable HeapView.BoxingAllocation
        cmd.Parameters.Add("card_damage", NpgsqlDbType.Integer).Value = card.Damage;
        cmd.Parameters.Add("user_fk", NpgsqlDbType.Integer).Value = id;
        cmd.Prepare();
        using (cmd)
        {
            cmd.ExecuteNonQuery();
        }

        /*
        //insert new user into database
        using(var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO mtcg_db.public.user (username, password) VALUES (@username, @password)";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        
        }
         */
    }


    public int GetUserCoins(User user)
    {
        var coins = 0;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT coins FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) coins = reader.GetInt32(0);
            }
        }

        return coins;
    }

    public void UpdateCoins(User user)
    {
        if (GetUserCoins(user) >= 5)
        {
            //reduce coins by 5
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE mtcg_db.public.user SET coins = coins - 5 WHERE username = @username";
                cmd.Parameters.AddWithValue("username", user.Username);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void SetCoins()
    {
        //set user coins to 20
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET coins = 20";
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public bool UserExists(string username)
    {
        var exists = false;
        //check if username is in database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read()) exists = true;
            }
        }

        return exists;
    }

    //method to register a new user
    public bool RegisterUser(User user)
    {
        //check if username already exists
        if (UserExists(user.Username)) return false;

        //insert new user into database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO mtcg_db.public.user (username, password) VALUES (@username, @password)";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("password", user.Password);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        return true;
    }

    //method to login
    public bool Login(User user)
    {
        //check if username exists
        if (!UserExists(user.Username)) return false;
        var check = false;
        //check if password is correct
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.GetString(2) == user.Password)
                    {
                        check = true;
                    }
                }
            }

            return check;
        }
    }

    public int GetElo(User user)
    {
        var elo = 0;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT elo FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) elo = reader.GetInt32(0);
            }
        }

        return elo;
    }

    public int UserAmount()
    {
        var amount = 0;
        //get amount of users in database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT COUNT(*) FROM mtcg_db.public.user";
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) amount = reader.GetInt32(0);
            }
        }

        return amount;
    }

    public string GetEloAndUsername(int id)
    {
        var eloAndUsername = "";
        //get elo and username of user with id
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT username, elo FROM mtcg_db.public.user WHERE id = @id";
            cmd.Parameters.AddWithValue("id", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) eloAndUsername = reader.GetString(0) + ": " + reader.GetInt32(1);
            }
        }

        return eloAndUsername;
    }

    //update Name, Bio, Image in user table
    public void UpdateUser(User user)
    {
        //check if username exists
        if (!UserExists(user.Username)) return;

        //insert bio and name into database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText =
                "UPDATE mtcg_db.public.user SET bio = @bio, name = @name, image = @image WHERE username = @username";

            cmd.Parameters.AddWithValue("username", user.Username);
            // ReSharper disable PossibleNullReferenceException
            cmd.Parameters.AddWithValue("bio", user.Bio);
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Parameters.AddWithValue("image", user.Image);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    //method to select all from user
    public string? SelectUser(User user)
    {
        //check if username exists
        if (!UserExists(user.Username)) return null;
        string? result = null;
        //select all from user
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    result = reader.GetInt32(0) + " " + reader.GetString(1) + " " + reader.GetString(2) + " " +
                             reader.GetInt32(3) + " " + reader.GetInt32(4) + " " + reader.GetString(5) + " " +
                             reader.GetString(6) + " " + reader.GetString(7);
            }
        }

        return result;
    }

    public void SaveCards(List<Card> boughtCards, string username)
    {
        var id = GetUserID(username);
        foreach (var card in boughtCards)
            //insert card in database with user id
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText =
                    "INSERT INTO mtcg_db.public.cards (userid, cardid, card_name, card_damage) VALUES (@userid, @cardid, @card_name, @card_damage)";
                cmd.Parameters.AddWithValue("userid", id);
                cmd.Parameters.AddWithValue("cardid", card.Id);
                cmd.Parameters.AddWithValue("card_name", card.Name);
                cmd.Parameters.AddWithValue("card_damage", card.Damage);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
    }

    public string GetCardInfo(string username)
    {
        //get all cards from user
        var cardInfo = "";
        var id = GetUserID(username);
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT cardid, card_name, card_damage FROM mtcg_db.public.cards WHERE userid = @userid";
            cmd.Parameters.AddWithValue("userid", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    cardInfo += reader.GetString(0) + " " + reader.GetString(1) + " " + reader.GetDouble(2) + "\n";
                }
            }
        }

        return cardInfo;
    }

    public string GetDeckInfo(string username)
    {
        //get all cards from user
        var deckInfo = "Deck: \n";
        var id = GetUserID(username);
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText =
                "SELECT card_id, card_name, card_damage FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    deckInfo += reader.GetString(0) + " " + reader.GetString(1) + " " + reader.GetDouble(2) + "\n";
                }
            }
        }

        if (deckInfo == "Deck: \n")
        {
            var randomdeck = Get4RandomCards(id);
            SaveDeck(randomdeck, username);
            return GetDeckInfo(username);
        }

        return deckInfo;
    }

    public int GetCardsAmountFromDeck(string username)
    {
        int id = GetUserID(username);
        int amount = 0;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText =
                "SELECT COUNT(*) FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    amount = reader.GetInt32(0);
                }
            }
        }

        return amount;
    }

    public void SaveDeck(List<Card> deck, string username)
    {
        var id = GetUserID(username);
        //delete current deck
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        foreach (var card in deck)
            //insert card in database with user id
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText =
                    "INSERT INTO mtcg_db.public.deck (user_fk, card_id, card_name, card_damage) VALUES (@user_fk, @card_id, @card_name, @card_damage)";
                cmd.Parameters.AddWithValue("user_fk", id);
                cmd.Parameters.AddWithValue("card_id", card.Id);
                cmd.Parameters.AddWithValue("card_name", card.Name);
                cmd.Parameters.AddWithValue("card_damage", card.Damage);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
    }

    public List<Card> Get4RandomCards(int userid)
    {
        //get 4 random cards from user
        var randomCards = new List<Card>();
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText =
                "SELECT cardid, card_name, card_damage FROM mtcg_db.public.cards WHERE userid = @userid ORDER BY RANDOM() LIMIT 4";
            cmd.Parameters.AddWithValue("userid", userid);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var card = new Card();
                    card.Id = reader.GetString(0);
                    card.Name = reader.GetString(1);
                    card.Damage = reader.GetDouble(2);
                    randomCards.Add(card);
                }
            }
        }

        return randomCards;
    }

    public void DeleteAllCards()
    {
        //delete all cards from cards table
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.cards";
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteAllDecks()
    {
        //delete all cards from cards table
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.deck";
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteDeckFromUser(string username)
    {
        var userID = GetUserID(username);
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", userID);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public List<Card> GetDeckByStrings(List<string> deckStrings, string username)
    {
        var userId = GetUserID(username);
        var deck = new List<Card>();
        foreach (var cardString in deckStrings)
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText =
                    "SELECT cardid, card_name, card_damage FROM mtcg_db.public.cards WHERE userid = @userid AND cardid = @cardid";
                cmd.Parameters.AddWithValue("userid", userId);
                cmd.Parameters.AddWithValue("cardid", cardString);
                cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var card = new Card();
                        card.Id = reader.GetString(0);
                        card.Name = reader.GetString(1);
                        card.Damage = reader.GetDouble(2);
                        deck.Add(card);
                    }
                }
            }

        return deck;
    }

    public string GetAvailableTrades(string username)
    {
        var trades = "Trades available (without yours): \n";
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * from mtcg_db.public.tradings WHERE username!=@username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    trades += "CardID: " + reader.GetString(1) + " CardType: " + reader.GetString(2) +
                              " Minimum Damage: " + reader.GetDouble(3) + " User: " + reader.GetString(4) + "\n";
            }
        }

        return trades;
    }

    public string UploadTrade(Trade trade)
    {
        //check if card belongs to user
        if (!CardBelongsToUser(trade.CardToTrade, trade.username)) return "Card does not belong to you!";

        //upload trade
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText =
                "INSERT INTO mtcg_db.public.tradings (id, cardid, cardtype, mindamage, username) VALUES (@id, @cardid, @cardtype, @mindamage, @username)";
            cmd.Parameters.AddWithValue("id", trade.Id);
            cmd.Parameters.AddWithValue("cardid", trade.CardToTrade);
            cmd.Parameters.AddWithValue("cardtype", trade.Type);
            cmd.Parameters.AddWithValue("mindamage", trade.MinimumDamage);
            cmd.Parameters.AddWithValue("username", trade.username);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        return "Trade successfully created!";
    }

    public bool CardBelongsToUser(string cardid, string username)
    {
        var id = GetUserID(username);
        var belongsToUser = false;

        //check if card belongs to user
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * from mtcg_db.public.cards WHERE userid=@userid AND cardid=@cardid";
            cmd.Parameters.AddWithValue("userid", id);
            cmd.Parameters.AddWithValue("cardid", cardid);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) belongsToUser = true;
            }
        }

        return belongsToUser;
    }

    public bool TradeBelongsToUser(string tradeid, string username)
    {
        //check if trade belongs to user
        var belongsToUser = false;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * from mtcg_db.public.tradings WHERE username=@username AND id=@id";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("id", tradeid);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read()) belongsToUser = true;
            }
        }

        return belongsToUser;
    }

    public string DeleteTrade(string tradeid, string username)
    {
        if (!TradeBelongsToUser(tradeid, username)) return "This trade does not belong to you!";

        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.tradings WHERE id=@id";
            cmd.Parameters.AddWithValue("id", tradeid);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        return "Trade deleted!";
    }

    public List<Card> GetDeckOfUser(string username)
    {
        var id = GetUserID(username);

        var deck = new List<Card>();
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT card_id, card_name, card_damage FROM mtcg_db.public.deck WHERE user_fk = @userid";
            cmd.Parameters.AddWithValue("userid", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var card = new Card();
                    card.Id = reader.GetString(0);
                    card.Name = reader.GetString(1);
                    card.Damage = reader.GetDouble(2);
                    deck.Add(card);
                }
            }
        }

        return deck;
    }

    public void UpdateWinner(string username)
    {
        //add 3 points to elo of user
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET elo = elo + 3 WHERE username = @username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateLoser(string username)
    {
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET elo = elo - 5 WHERE username = @username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }
    
    public bool DeleteUser(string username)
    {
        var id = GetUserID(username);
        if (id == -1)
        {
            return false;
        }
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.user WHERE id = @id";
            cmd.Parameters.AddWithValue("id", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        return true;
    }
}