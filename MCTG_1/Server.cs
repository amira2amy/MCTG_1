using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Data;
using System.Security.Cryptography;
using Json.Net;
using System.Net.Sockets;



namespace MCTG_1;

public class Server
{
    private TcpListener listener;
    private Interaction interaction;
    private CardHandler cardHandler;
    private List<Card> kienboecDeck;
    private List<Card> altenhofDeck;
    public Server()
    {
        listener = new TcpListener(IPAddress.Any, 10001);
        interaction = new Interaction();
        cardHandler = new CardHandler();
        kienboecDeck = new List<Card>();
        altenhofDeck = new List<Card>();
    }

    public void StartServer()
    {
        listener.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Thread thread = new Thread(() => RequestHandler(client));
            thread.Start();
        }
    }

    private void RequestHandler(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        string request = "";
        byte[] bts = new byte[2048];
        int btsR;
        do
        {
            btsR = stream.Read(bts, 0, bts.Length);
            request = Encoding.UTF8.GetString(bts, 0, btsR);
        } while (stream.DataAvailable);
        
        string method = request.Split(' ')[0];
        string url = request.Split(' ')[1];
        
        //split request in seperate lines
        string[] requestLines = request.Split('\n');
        //search for authorization header and get token
        string token = "";
        string userToken = "";
        foreach (string line in requestLines)
        {
            if (line.Contains("Authorization"))
            {
                token = line.Split(' ')[2];
                userToken = token.Split("-")[0];
            }
        }
        
        //get last string in requestlines
        string body = requestLines[requestLines.Length - 1];
        
        Console.WriteLine(request);
        Console.WriteLine("----------------------------");
        string responseString = "";
        string userAuthorization = "";
        string contentType = "";

        if (method == "POST" && url == "/users")
        {
            // parse request body and create a new User object
            User user = JsonNet.Deserialize<User>(body);
            if (interaction.RegisterUser(user))
            {
                responseString = "User " + user.Username + " registered successfully";
            }
            else
            {
                responseString = "User " + user.Username + " already exists";
            }
        }else if (method == "POST" && url == "/sessions")
        {
            string json = body;
            // parse request body and create a new User object
            User user = JsonNet.Deserialize<User>(json);
            if (interaction.Login(user))
            {
                responseString = "User " + user.Username + " logged in successfully";
            }
            else
            {
                responseString = "User " + user.Username + " login failed";
            }
        }else if (method == "POST" && url == "/packages")
        {
            if (userToken == "admin")
            {
                string json = body;
                // parse request body and create a new User object
                List<Card> package = JsonNet.Deserialize<List<Card>>(json);
                cardHandler.packages.Add(package);
                responseString = "Package created with " + package.Count + " cards";
                responseString = cardHandler.packages.Count + " packages created";
            }
            else
            {
                responseString = "Not an admin";
            }


        }
        else if (method == "POST" && url == "/transactions/packages")
        {
            User currentUser = new User();
            currentUser.Username = userToken;

            if (interaction.GetUserCoins(currentUser) >= 5)
            {
                if (cardHandler.packages.Count > 0)
                {
                    
                    cardHandler.baughtCards = (cardHandler.packages[0]);
                    interaction.SaveCards(cardHandler.baughtCards, currentUser.Username);
                    cardHandler.packages.Remove(cardHandler.packages[0]);
                    interaction.UpdateCoins(currentUser);
                    responseString += "Package bought by " + currentUser.Username;
                    responseString += "\nPackages left: " + cardHandler.packages.Count;
                    responseString += "\nCoins left: " + interaction.GetUserCoins(currentUser);
                    
                }else
                {
                    responseString = "No packages left";
                }
                
            }
            else
            {
                responseString = "Not enough coins";
            }

        }else if (method == "GET" && url == "/cards")
        {
            if (string.IsNullOrEmpty(token))
            {
                responseString = "No token";
            }else
            {
                User currentUser = new User();
                currentUser.Username = userToken;
                responseString = interaction.GetCardInfo(currentUser.Username);
            }
        }else if (url == "/deck")
        {
            if (method == "GET")
            {
                if (string.IsNullOrEmpty(token))
                {
                    responseString = "No token";
                }else
                {
                    User currentUser = new User();
                    currentUser.Username = userToken;
                    responseString = interaction.GetDeckInfo(currentUser.Username);
                }
            }else if (method == "PUT")
            {
                if (string.IsNullOrEmpty(token))
                {
                    responseString = "No token";
                }else
                {
                    User currentUser = new User();
                    currentUser.Username = userToken;
                    List<string> deckString = JsonNet.Deserialize<List<string>>(body);
                    if (deckString.Count != 4)
                    {
                        responseString = "Deck must contain 4 cards";
                    }
                    else
                    {
                        
                        List<Card> deck = interaction.GetDeckByStrings(deckString, currentUser.Username);
                        if (deck.Count < 4)
                        {
                            responseString = "Failed: One or more cards not found in your cards";
                        }
                        else
                        {
                            interaction.SaveDeck(deck, currentUser.Username);
                            responseString = "Deck saved";
                        }
                    }
                }
            }
        }else if (method == "GET" && url == "/stats")
        {
            User currentUser = new User();
            int elo;
            currentUser.Username = userToken;
            elo = interaction.GetElo(currentUser);
            responseString = "Elo for " + userToken + ": " + elo;
        }else if (method == "GET" && url == "/score")
        {
            User currentUser = new User();
            currentUser.Username = userToken;
            if (interaction.UserExists(currentUser.Username))
            {
                string allElo = "";
                Console.WriteLine("User exists");
                for (int i = 0; i < interaction.UserAmount(); i++)
                {
                    allElo += interaction.GetEloAndUsername(i);
                    allElo += "\n";
                }
                responseString = allElo;
            }
            else
            {
                responseString = "User does not exist";
            }
        }else if (url.Contains("/users/"))
        {
            Console.WriteLine(url);
            string[] urls = url.Split('/');
            string dataUser = urls[2];
            Console.WriteLine(dataUser);
            if (method == "PUT")
            {
                if (dataUser == userToken)
                {
                    //User currentuser = new User();
                    //currentuser.Username = dataUser;
                    User user = JsonNet.Deserialize<User>(body);
                    user.Username = dataUser;
                    responseString = "Your data is set: " + user.Name + " " + user.Bio + " " + user.Image;
                    interaction.UpdateUser(user);
                    
                }else
                {
                    responseString = "User does not exist";
                }
            }else if (method == "GET")
            {
                if (dataUser == userToken)
                {
                    User currentuser = new User();
                    currentuser.Username = dataUser;
                    Console.WriteLine("User exists");
                    responseString = interaction.SelectUser(currentuser);
                }else
                {
                    responseString = "User does not exist";
                }
            }
        }else if (url == "/tradings")
        {
        }
        
        StringBuilder response = new StringBuilder();
        StringWriter writer = new StringWriter();
        response.Append("HTTP/1.1 200 OK\r\n");
        response.Append("Content-Type: application/json\r\n");
        response.Append("Content-Length: " + responseString.Length + "\r\n\r\n");
        response.Append(responseString);
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response.ToString());
        stream.Write(buffer, 0, buffer.Length);
        
        client.Close();
        
    }
    

}