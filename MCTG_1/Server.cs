using System.Net;
using System.Net.Sockets;
using System.Text;
using Json.Net;

namespace MCTG_1;

public class Server
{
    private readonly Arena arena;
    private readonly CardHandler cardHandler;
    private readonly Interaction interaction;
    private readonly TcpListener listener;

    public Server()
    {
        listener = new TcpListener(IPAddress.Any, 10001);
        interaction = new Interaction();
        cardHandler = new CardHandler();
        arena = new Arena();
    }

    public void StartServer()
    {
        listener.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            var client = listener.AcceptTcpClient();
            var thread = new Thread(() => RequestHandler(client));
            thread.Start();
        }
    }

    private void RequestHandler(TcpClient client)
    {
        var stream = client.GetStream();
        var request = "";
        var bts = new byte[2048];
        int btsR;
        do
        {
            btsR = stream.Read(bts, 0, bts.Length);
            request = Encoding.UTF8.GetString(bts, 0, btsR);
        } while (stream.DataAvailable);

        var method = request.Split(' ')[0];
        var url = request.Split(' ')[1];

        //split request in seperate lines
        var requestLines = request.Split('\n');
        //search for authorization header and get token
        var token = "";
        var userToken = "";
        foreach (var line in requestLines)
            if (line.Contains("Authorization"))
            {
                token = line.Split(' ')[2];
                userToken = token.Split("-")[0];
            }

        //get last string in requestlines
        var body = requestLines[requestLines.Length - 1];

        Console.WriteLine(request);
        Console.WriteLine("----------------------------");
        var answerClient = "";
        var userAuthorization = "";
        var contentType = "";

        if (method == "POST" && url == "/users")
        {
            // parse request body and create a new User object
            var user = JsonNet.Deserialize<User>(body);
            if (interaction.RegisterUser(user))
                answerClient = "User " + user.Username + " registered successfully";
            else
                answerClient = "User " + user.Username + " already exists";
        }
        else if (method == "POST" && url == "/sessions")
        {
            var json = body;
            // parse request body and create a new User object
            var user = JsonNet.Deserialize<User>(json);
            if (interaction.Login(user))
                answerClient = "User " + user.Username + " logged in successfully";
            else
                answerClient = "User " + user.Username + " login failed";
        }
        else if (method == "POST" && url == "/packages")
        {
            if (userToken == "admin")
            {
                var json = body;
                // parse request body and create a new User object
                var package = JsonNet.Deserialize<List<Card>>(json);
                cardHandler.packages.Add(package);
                answerClient = "Package created with " + package.Count + " cards";
                answerClient = cardHandler.packages.Count + " packages created";
            }
            else
            {
                answerClient = "Not an admin";
            }
        }
        else if (method == "POST" && url == "/transactions/packages")
        {
            var currentUser = new User();
            currentUser.Username = userToken;

            if (interaction.GetUserCoins(currentUser) >= 5)
            {
                if (cardHandler.packages.Count > 0)
                {
                    cardHandler.baughtCards = cardHandler.packages[0];
                    interaction.SaveCards(cardHandler.baughtCards, currentUser.Username);
                    cardHandler.packages.Remove(cardHandler.packages[0]);
                    interaction.UpdateCoins(currentUser);
                    answerClient += "Package bought by " + currentUser.Username;
                    answerClient += "\nPackages left: " + cardHandler.packages.Count;
                    answerClient += "\nCoins left: " + interaction.GetUserCoins(currentUser);
                }
                else
                {
                    answerClient = "No packages left";
                }
            }
            else
            {
                answerClient = "Not enough coins";
            }
        }
        else if (method == "GET" && url == "/cards")
        {
            if (string.IsNullOrEmpty(token))
            {
                answerClient = "No token";
            }
            else
            {
                var currentUser = new User();
                currentUser.Username = userToken;
                answerClient = interaction.GetCardInfo(currentUser.Username);
            }
        }
        else if (url == "/deck")
        {
            if (method == "GET")
            {
                if (string.IsNullOrEmpty(token))
                {
                    answerClient = "No token";
                }
                else
                {
                    var currentUser = new User();
                    currentUser.Username = userToken;
                    answerClient = interaction.GetDeckInfo(currentUser.Username);
                }
            }
            else if (method == "PUT")
            {
                if (string.IsNullOrEmpty(token))
                {
                    answerClient = "No token";
                }
                else
                {
                    var currentUser = new User();
                    currentUser.Username = userToken;
                    var deckString = JsonNet.Deserialize<List<string>>(body);
                    if (deckString.Count != 4)
                    {
                        answerClient = "Deck must contain 4 cards";
                    }
                    else
                    {
                        var deck = interaction.GetDeckByStrings(deckString, currentUser.Username);
                        if (deck.Count < 4)
                        {
                            answerClient = "Failed: One or more cards not found in your cards";
                        }
                        else
                        {
                            interaction.SaveDeck(deck, currentUser.Username);
                            answerClient = "Deck saved";
                        }
                    }
                }
            }
        }
        else if (method == "GET" && url == "/stats")
        {
            var currentUser = new User();
            int elo;
            currentUser.Username = userToken;
            elo = interaction.GetElo(currentUser);
            answerClient = "Elo for " + userToken + ": " + elo;
        }
        else if (method == "GET" && url == "/score")
        {
            var currentUser = new User();
            currentUser.Username = userToken;
            if (interaction.UserExists(currentUser.Username))
            {
                var allElo = "";
                Console.WriteLine("User exists");
                for (var i = 0; i < interaction.UserAmount(); i++)
                {
                    allElo += interaction.GetEloAndUsername(i);
                    allElo += "\n";
                }

                answerClient = allElo;
            }
            else
            {
                answerClient = "User does not exist";
            }
        }
        else if (url.Contains("/users/"))
        {
            Console.WriteLine(url);
            var urls = url.Split('/');
            var dataUser = urls[2];
            Console.WriteLine(dataUser);
            if (method == "PUT")
            {
                if (dataUser == userToken)
                {
                    //User currentuser = new User();
                    //currentuser.Username = dataUser;
                    var user = JsonNet.Deserialize<User>(body);
                    user.Username = dataUser;
                    answerClient = "Your data is set: " + user.Name + " " + user.Bio + " " + user.Image;
                    interaction.UpdateUser(user);
                }
                else
                {
                    answerClient = "User does not exist";
                }
            }
            else if (method == "GET")
            {
                if (dataUser == userToken)
                {
                    var currentuser = new User();
                    currentuser.Username = dataUser;
                    Console.WriteLine("User exists");
                    answerClient = interaction.SelectUser(currentuser);
                }
                else
                {
                    answerClient = "User does not exist";
                }
            }
        }
        else if (url == "/tradings")
        {
            if (method == "GET")
            {
                answerClient = interaction.GetAvailableTrades(userToken);
            }
            else if (method == "POST")
            {
                var json = body;
                var trade = JsonNet.Deserialize<Trade>(json);
                trade.username = userToken;

                answerClient = interaction.UploadTrade(trade);
            }
        }
        else if (url.Contains("/tradings/"))
        {
            var tradeId = url.Split('/')[2];

            if (method == "DELETE") answerClient = interaction.DeleteTrade(tradeId, userToken);
        }
        else if (url == "/battles")
        {
            if (method == "POST")
            {
                var currentUser = new User();
                currentUser.Username = userToken;
                currentUser.Deck = interaction.GetDeckOfUser(currentUser.Username);
                //Something wrong here!!!!!!
                arena.AddToLobby(currentUser);

                if (arena.Lobby.Count == 2)
                {
                    arena.Battle();
                    answerClient = arena.PrintLog();
                }
                else
                {
                    answerClient = "Waiting for opponent";
                }
            }
        }

        var response = new StringBuilder();
        var writer = new StringWriter();
        response.Append("HTTP/1.1 200 OK\r\n");
        response.Append("Content-Type: application/json\r\n");
        response.Append("Content-Length: " + answerClient.Length + "\r\n\r\n");
        response.Append(answerClient);
        var buffer = Encoding.UTF8.GetBytes(response.ToString());
        stream.Write(buffer, 0, buffer.Length);

        client.Close();
    }
}