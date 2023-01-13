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



namespace MCTG_1;

public class Server
{
    private HttpListener listener;
    private Interaction interaction;
    private CardHandler cardHandler;
    private List<List<Card>> kienboecCards;
    private List<List<Card>> altenhofCards;
    private List<Card> kienboecDeck;
    private List<Card> altenhofDeck;
    public Server()
    {
        listener = new HttpListener();
        interaction = new Interaction();
        cardHandler = new CardHandler();
        kienboecCards = new List<List<Card>>();
        altenhofCards = new List<List<Card>>();
        kienboecDeck = new List<Card>();
        altenhofDeck = new List<Card>();
    }

    public void StartServer()
    {
        listener.Prefixes.Add("http://localhost:10001/");
        listener.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            Thread thread = new Thread(new ParameterizedThreadStart(RequestHandler));
            thread.Start(context);
        }
    }

    private void RequestHandler(object context)
    {
        HttpListenerContext httpContext = (HttpListenerContext)context;
        HttpListenerRequest request = httpContext.Request;
        HttpListenerResponse response = httpContext.Response;
        string responseString = "";
        string userAuthorization = "";
        string contentType = "";

        if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/users")
        {
            using (System.IO.Stream body = request.InputStream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    string json = reader.ReadToEnd();
                    // parse request body and create a new User object
                    User user = JsonNet.Deserialize<User>(json);
                    if (interaction.RegisterUser(user))
                    {
                        Console.WriteLine("User " + user.Username + " registered successfully");
                    }
                    else
                    {
                        Console.WriteLine("User " + user.Username + " already exists");
                    }
                }
            }

        }
        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/sessions")
        {
            using (System.IO.Stream body = request.InputStream)
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    string json = reader.ReadToEnd();
                    // parse request body and create a new User object
                    User user = JsonNet.Deserialize<User>(json);
                    if (interaction.Login(user))
                    {
                        Console.WriteLine("User " + user.Username + " logged in successfully");
                    }
                    else
                    {
                        Console.WriteLine("User " + user.Username + " login failed");
                    }
                }
            }
        }
        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/packages")
        {
            userAuthorization = request.Headers["Authorization"];
            if (userAuthorization == "Basic admin-mtcgToken")
            {
                using (System.IO.Stream body = request.InputStream)
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding))
                    {
                        string json = reader.ReadToEnd();
                        // parse request body and create a new User object

                        List<Card> package = JsonNet.Deserialize<List<Card>>(json);

                        cardHandler.packages.Add(package);
                        Console.WriteLine("Package created with " + package.Count + " cards");
                        Console.WriteLine(cardHandler.packages.Count + " packages created");
                        
                    }
                }
            }
            else
            {
                Console.WriteLine("Not an admin");
            }


        }
        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/transactions/packages")
        {
            User currentUser = new User();
            userAuthorization = request.Headers["Authorization"];
            if (userAuthorization == "Basic kienboec-mtcgToken")
            {
                currentUser.Username= "kienboec";
            }else if (userAuthorization == "Basic altenhof-mtcgToken")
            {
                currentUser.Username = "altenhof";
            }

            if (interaction.GetUserCoins(currentUser) >= 5)
            {
                if (cardHandler.packages.Count > 0)
                {
                    if (currentUser.Username == "kienboec")
                    {
                        kienboecCards.Add(cardHandler.packages[0]);
                        //currentUser.Cards.Add(cardHandler.packages[0]);
                    }else if(currentUser.Username == "altenhof")
                    {
                        altenhofCards.Add(cardHandler.packages[0]);
                    }

                    cardHandler.packages.Remove(cardHandler.packages[0]);
                    interaction.UpdateCoins(currentUser);
                    Console.WriteLine("Package bought by " + currentUser.Username);
                    Console.WriteLine("Packages left: " + cardHandler.packages.Count);
                    Console.WriteLine("Coins left: " + interaction.GetUserCoins(currentUser));
                }else
                {
                    Console.WriteLine("No packages left");
                }
                
            }
            else
            {
                Console.WriteLine("Not enough coins");
            }

        }else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/cards")
        {
            userAuthorization = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(userAuthorization))
            {
                Console.WriteLine("No token");
            }else
            {
                User currentUser = new User();
                if (userAuthorization.Contains("kienboec"))
                {
                    for (int i = 0; i < kienboecCards.Count; i++)
                    {
                        for (int j = 0; j < kienboecCards[i].Count; j++)
                        {
                            Console.WriteLine("Card for kienboec: " + kienboecCards[i][j].Name + " " + kienboecCards[i][j].Damage);
                        }
                    }
                }else if (userAuthorization.Contains("altenhof"))
                {
                    for (int i = 0; i < altenhofCards.Count; i++)
                    {
                        for (int j = 0; j < altenhofCards[i].Count; j++)
                        {
                            Console.WriteLine("Card for altenhof: " + altenhofCards[i][j].Name + " " + altenhofCards[i][j].Damage);
                        }
                    }
                }
                
            }
        }
        /*
        else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/deck")
        {
            userAuthorization = request.Headers["Authorization"];
            contentType = request.Headers["Content-Type"];
            if (userAuthorization.Contains("kienboec") && string.IsNullOrEmpty(contentType))
            {
                User currentUser = new User();
                currentUser.Username = "kienboec";
                //take 4 random cards from kienboecCards and add them to kienboecDeck
                for (int i = 0; i < 4; i++)
                {
                    Random rd = new Random();
                    //pick a random package from kienboecCards
                    int randomPackage = rd.Next(0, kienboecCards.Count);
                    //pick a random card from the package
                    int randomCard = rd.Next(0, kienboecCards[randomPackage].Count);
                    //add the card to the deck
                    kienboecDeck.Add(kienboecCards[randomPackage][randomCard]);
                }
                
                for (int i = 0; i < kienboecDeck.Count; i++)
                {
                    Console.WriteLine("Unconfigured Card for kienboec: " + kienboecDeck[i].Name + " " + kienboecDeck[i].Damage);
                }
                Console.WriteLine("Done");
                for (int i = 0; i < kienboecCards.Count; i++)
                {
                    for (int j = 0; j < kienboecCards[i].Count; j++)
                    {
                        interaction.InsertCard(kienboecCards[i][j], currentUser);
                    }
                }
                
            }else if (userAuthorization.Contains("altenhof") && string.IsNullOrEmpty(contentType))
            {
                User currentUser = new User();
                currentUser.Username = "altenhof";
                //take 4 random cards from altenhofCards and add them to altenhofDeck
                for (int i = 0; i < 4; i++)
                {
                    Random rd = new Random();
                    //pick a random package from altenhofCards
                    int randomPackage = rd.Next(0, altenhofCards.Count);
                    //pick a random card from the package
                    int randomCard = rd.Next(0, altenhofCards[randomPackage].Count);
                    //add the card to the deck
                    altenhofDeck.Add(altenhofCards[randomPackage][randomCard]);
                }
                
                for (int i = 0; i < altenhofDeck.Count; i++)
                {
                    Console.WriteLine("Unconfigured Card for altenhof: " + altenhofDeck[i].Name + " " + altenhofDeck[i].Damage);
                }
                Console.WriteLine("Done");
            }


            kienboecDeck.Clear();
            altenhofDeck.Clear();
        }
        */
        else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/stats")
        {
            User currentUser = new User();
            int elo;
            userAuthorization = request.Headers["Authorization"];
            if (userAuthorization.Contains("kienboec"))
            {
                currentUser.Username = "kienboec";
                elo = interaction.GetElo(currentUser);
                Console.WriteLine("Elo for kienboec: " + elo);
            }else if (userAuthorization.Contains("altenhof"))
            {
                currentUser.Username = "altenhof";
                elo = interaction.GetElo(currentUser);
                Console.WriteLine("Elo for altenhof: " + elo);
            }
            
        }else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/score")
        {
            User currentUser = new User();
            userAuthorization = request.Headers["Authorization"];
            if (userAuthorization.Contains("kienboec"))
            {
                currentUser.Username = "kienboec";
                if (interaction.UserExists(currentUser.Username))
                {
                    Console.WriteLine("User exists");
                    for (int i = 0; i < interaction.UserAmount(); i++)
                    {
                        Console.WriteLine(interaction.GetEloAndUsername(i));
                    }
                }
                else
                {
                    Console.WriteLine("User does not exist");
                }
                
            }
        }


        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;    
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    

}