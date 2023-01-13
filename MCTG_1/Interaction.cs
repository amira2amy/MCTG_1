using System.Security.Cryptography;
using NpgsqlTypes;

namespace MCTG_1;
using Npgsql;
using System;


public class Interaction
{
    private readonly string _connectionString =
        "Host=185.65.234.37;Username=underline;Password=underline;Database=mtcg_db";

    public NpgsqlConnection conn;

    public Interaction()
    {
        conn = new NpgsqlConnection(_connectionString);
        conn.Open();
    }

    public int GetUserID(User currentUser)
    {
        int id = -1;
        using (var cmd = new NpgsqlCommand())
        {
            cmd.Connection = conn;
            cmd.CommandText = "SELECT ID FROM mtcg_db.public.user WHERE username = @username";
            cmd.Parameters.AddWithValue("username", currentUser.Username);
            cmd.Prepare();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
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
        int id = GetUserID(currentUser);
        NpgsqlCommand cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText =
            "INSERT INTO mtcg_db.public.deck (card_id, card_name, card_damage, user_fk) VALUES (@card_id, @card_name, @card_damage, @user_fk)";
        cmd.Parameters.Add("card_id", NpgsqlDbType.Integer).Value = card.ID;
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
                    return reader.GetInt32(0);
                }
            }
        }

        return 0;
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
                    return true;
                }
            }
        }

        return false;


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
                        return true;
                    }
                }
            }

            return false;
        }

    }

    public int GetElo(User user)
    {
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
                    return reader.GetInt32(0);
                }
            }
        }

        return 0;
    }

    public int UserAmount()
    {
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
                    return reader.GetInt32(0);
                }
            }
        }

        return 0;
    }

    public string GetEloAndUsername(int id)
    {
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
                    return reader.GetString(0) + ": " + reader.GetInt32(1);
                }
            }
        }

        return "";
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
}