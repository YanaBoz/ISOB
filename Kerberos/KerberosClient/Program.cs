using System.Net.Sockets;
using System.Text;

class KerberosClient
{
    private const int AuthServerPort = 8080;
    private const int TicketServerPort = 8082;
    private const int ServiceServerPort = 8083;

    static void Main()
    {
        Console.Write("Enter client ID: ");
        string clientId = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        if (Authenticate(clientId, password))
        {
            string ticket = GetTicket(clientId);
            Console.WriteLine($"Received ticket: {ticket}");
            AccessService(ticket);
        }
        else
        {
            Console.WriteLine("Authentication failed.");
        }
    }

    static bool Authenticate(string clientId, string password)
    {
        using (var clientSocket = new TcpClient("localhost", AuthServerPort))
        {
            NetworkStream stream = clientSocket.GetStream();
            string authRequest = $"LOGIN {clientId} {password}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(authRequest);
            stream.Write(requestBytes, 0, requestBytes.Length);

            byte[] responseBytes = new byte[1024];
            int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
            string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
            Console.WriteLine("Client received: " + response);

            return response == "AUTH_OK";
        }
    }

    static string GetTicket(string clientId)
    {
        using (var clientSocket = new TcpClient("localhost", TicketServerPort))
        {
            NetworkStream stream = clientSocket.GetStream();
            string ticketRequest = $"TICKET {clientId}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(ticketRequest);
            stream.Write(requestBytes, 0, requestBytes.Length);

            byte[] responseBytes = new byte[1024];
            int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
            string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);

            Console.WriteLine("Client received ticket: " + response);

            return response.Split(':')[1].Trim(); 
        }
    }

    static void AccessService(string ticket)
    {
        using (var clientSocket = new TcpClient("localhost", ServiceServerPort))
        {
            NetworkStream stream = clientSocket.GetStream();
            string serviceRequest = $"SERVICE_REQUEST {ticket}";
            byte[] requestBytes = Encoding.UTF8.GetBytes(serviceRequest);
            stream.Write(requestBytes, 0, requestBytes.Length);

            byte[] responseBytes = new byte[1024];
            int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);
            string response = Encoding.UTF8.GetString(responseBytes, 0, bytesRead);
            Console.WriteLine("Client received: " + response);
        }
    }
}
