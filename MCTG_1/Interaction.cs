using System.Security.Cryptography;
using NpgsqlTypes;

namespace MCTG_1;
using Npgsql;
using System;


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
        int id = -1;
        using (var con = new NpgsqlConnection(Conn.ConnectionString))
        {
            con.Open();
            using (var cmd = new NpgsqlCommand("SELECT id FROM mtcg_db.public.user WHERE username = @username;", con))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Prepare();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
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
        int id = GetUserID(currentUser.Username);
        NpgsqlCommand cmd = new NpgsqlCommand();
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
        int coins = 0;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT coins FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    coins = reader.GetInt32(0);
                }
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
        bool exists = false;
        //check if username is in database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    exists = true;
                }
            }
        }

        return exists;


    }

    //method to register a new user
    public bool RegisterUser(User user)
    {
        //check if username already exists
        if (UserExists(user.Username))
        {
            return false;
        }

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
        if (!UserExists(user.Username))
        {
            return false;
        }
        bool check = false;
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
        int elo = 0;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT elo FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    elo = reader.GetInt32(0);
                }
            }
        }

        return elo;
    }

    public int UserAmount()
    {
        int amount = 0;
        //get amount of users in database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT COUNT(*) FROM mtcg_db.public.user";
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

    public string GetEloAndUsername(int id)
    {
        string eloAndUsername = "";
        //get elo and username of user with id
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT username, elo FROM mtcg_db.public.user WHERE id = @id";
            cmd.Parameters.AddWithValue("id", id);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    eloAndUsername = reader.GetString(0) + ": " + reader.GetInt32(1);
                }
            }
        }

        return eloAndUsername;
    }
    
    public void AddNameAndBio(User user)
    {
        //add name and bio to user
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET name = @name, bio = @bio WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Parameters.AddWithValue("bio", user.Bio);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }
    
    //method to insert bio to current user
    public void InsertBioAndName(User user)
    {
        //check if username exists
        if (!UserExists(user.Username))
        {
            return;
        }

        //insert bio and name into database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET bio = @bio, name = @name WHERE username = @username";
            cmd.Parameters.AddWithValue("username", user.Username);
            cmd.Parameters.AddWithValue("bio", user.Bio);
            cmd.Parameters.AddWithValue("name", user.Name);
            cmd.Prepare();
            cmd.ExecuteNonQuery();

        }
    }
    
    //update Name, Bio, Image in user table
    public void UpdateUser(User user)
    {
        //check if username exists
        if (!UserExists(user.Username))
        {
            return;
        }

        //insert bio and name into database
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "UPDATE mtcg_db.public.user SET bio = @bio, name = @name, image = @image WHERE username = @username";

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
        if (!UserExists(user.Username))
        {
            return null;
        }
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
                {
                    result = reader.GetInt32(0) + " " + reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetInt32(3) + " " + reader.GetInt32(4) + " " + reader.GetString(5)+ " " + reader.GetString(6)+ " " + reader.GetString(7);    
                }
            }
        }

        return result;
    }

    public void SaveCards(List<Card> boughtCards, string username)
    {
        int id = GetUserID(username);
        foreach (Card card in boughtCards)
        {
            //insert card in database with user id
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO mtcg_db.public.cards (userid, cardid, card_name, card_damage) VALUES (@userid, @cardid, @card_name, @card_damage)";
                cmd.Parameters.AddWithValue("userid", id);
                cmd.Parameters.AddWithValue("cardid", card.Id);
                cmd.Parameters.AddWithValue("card_name", card.Name);
                cmd.Parameters.AddWithValue("card_damage", card.Damage);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }

    public string GetCardInfo(string username)
    {
        //get all cards from user
        string cardInfo = "";
        int id = GetUserID(username);
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
        string deckInfo = "Deck: \n";
        int id = GetUserID(username);
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT card_id, card_name, card_damage FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
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
            List<Card> randomdeck = Get4RandomCards(id);
            SaveDeck(randomdeck, username);
            return GetDeckInfo(username);
        } else
        {
            return deckInfo;
        }
        
    }
    
    public void SaveDeck(List<Card> deck, string username)
    {
        int id = GetUserID(username);
        //delete current deck
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
        foreach (Card card in deck)
        {
            //insert card in database with user id
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "INSERT INTO mtcg_db.public.deck (user_fk, card_id, card_name, card_damage) VALUES (@user_fk, @card_id, @card_name, @card_damage)";
                cmd.Parameters.AddWithValue("user_fk", id);
                cmd.Parameters.AddWithValue("card_id", card.Id);
                cmd.Parameters.AddWithValue("card_name", card.Name);
                cmd.Parameters.AddWithValue("card_damage", card.Damage);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public void DeleteDeck(string username)
    {
        int id = GetUserID(username);
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "DELETE FROM mtcg_db.public.deck WHERE user_fk = @user_fk";
            cmd.Parameters.AddWithValue("user_fk", id);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
    }

    public List<Card> Get4RandomCards(int userid)
    {
        //get 4 random cards from user
        List<Card> randomCards = new List<Card>();
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT cardid, card_name, card_damage FROM mtcg_db.public.cards WHERE userid = @userid ORDER BY RANDOM() LIMIT 4";
            cmd.Parameters.AddWithValue("userid", userid);
            cmd.Prepare();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card card = new Card();
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

    public List<Card> GetDeckByStrings(List<string> deckStrings, string username)
    {
        int userId = GetUserID(username);
        List<Card> deck = new List<Card>();
        foreach (string cardString in deckStrings)
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = "SELECT cardid, card_name, card_damage FROM mtcg_db.public.cards WHERE userid = @userid AND cardid = @cardid";
                cmd.Parameters.AddWithValue("userid", userId);
                cmd.Parameters.AddWithValue("cardid", cardString);
                cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Card card = new Card();
                        card.Id = reader.GetString(0);
                        card.Name = reader.GetString(1);
                        card.Damage = reader.GetDouble(2);
                        deck.Add(card);
                    }
                }
            }
        }
        return deck;
    }
    
    
}