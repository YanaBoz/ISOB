using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

class KerberosServer
{
    private const string ConnectionString = "Data Source=bd,1433;Initial Catalog=Lab3_Serv;User ID=sa;Password=Kolobok@12345;TrustServerCertificate=True;";
    private const int AuthServerPort = 8080;
    private const int TicketServerPort = 8082;
    private const int ServiceServerPort = 8083;

    static void Main()
    {
        Console.WriteLine("Kerberos Server started...");

        InitializeDatabase();

        Thread authServerThread = new Thread(StartAuthenticationServer);
        authServerThread.Start();

        Thread tgsServerThread = new Thread(StartTicketGrantingServer);
        tgsServerThread.Start();

        Thread serviceServerThread = new Thread(StartServiceServer);
        serviceServerThread.Start();
    }

    static void InitializeDatabase()
    {
        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
                    BEGIN
                        CREATE TABLE Users (
                            Id NVARCHAR(100) PRIMARY KEY,   
                            PasswordHash NVARCHAR(256) NOT NULL  
                        );
                    END";

                using (var command = new SqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Таблица 'Users' создана или уже существует.");
                }

                AddUserIfNotExists(connection, "clientC", "password123");
                AddUserIfNotExists(connection, "clientD", "password456");
                AddUserIfNotExists(connection, "clientE", "password789");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при инициализации базы данных: " + ex.Message);
        }
    }

    static void AddUserIfNotExists(SqlConnection connection, string clientId, string password)
    {
        try
        {
            string insertUserQuery = @"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @Id)
                BEGIN
                    INSERT INTO Users (Id, PasswordHash) VALUES 
                    (@Id, @PasswordHash);
                END";

            using (var insertCommand = new SqlCommand(insertUserQuery, connection))
            {
                insertCommand.Parameters.AddWithValue("@Id", clientId);
                insertCommand.Parameters.AddWithValue("@PasswordHash", ComputeHash(password));
                insertCommand.ExecuteNonQuery();
                Console.WriteLine($"Пользователь '{clientId}' добавлен в базу данных.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении пользователя {clientId}: " + ex.Message);
        }
    }

    static void StartAuthenticationServer()
    {
        try
        {
            var listener = new TcpListener(IPAddress.Any, AuthServerPort);
            listener.Start();

            Console.WriteLine("Authentication Server (AS) is waiting for requests...");

            while (true)
            {
                var clientSocket = listener.AcceptSocket();
                Thread clientThread = new Thread(() => HandleAuthenticationRequest(clientSocket));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при запуске сервера аутентификации: " + ex.Message);
        }
    }

    static void HandleAuthenticationRequest(Socket clientSocket)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string clientRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("AS received: " + clientRequest);

            if (clientRequest.StartsWith("LOGIN"))
            {
                string clientId = clientRequest.Split(' ')[1];
                string password = clientRequest.Split(' ')[2];

                string passwordHash = ComputeHash(password);
                Console.WriteLine($"Хэш пароля для клиента {clientId}: {passwordHash}");

                bool isAuthenticated = AuthenticateUser(clientId, password);
                string response = isAuthenticated ? "AUTH_OK" : "AUTH_FAILED";

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                clientSocket.Send(responseBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке запроса на аутентификацию: " + ex.Message);
        }
        finally
        {
            clientSocket.Close();
        }
    }

    static void StartTicketGrantingServer()
    {
        try
        {
            var listener = new TcpListener(IPAddress.Any, TicketServerPort);
            listener.Start();

            Console.WriteLine("Ticket Granting Server (TGS) is waiting for requests...");

            while (true)
            {
                var clientSocket = listener.AcceptSocket();
                Thread clientThread = new Thread(() => HandleTicketRequest(clientSocket));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при запуске TGS: " + ex.Message);
        }
    }

    static void HandleTicketRequest(Socket clientSocket)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string clientRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("TGS received: " + clientRequest);

            if (clientRequest.StartsWith("TICKET"))
            {
                string ticketRequest = clientRequest.Split(' ')[1];

                string ticket = $"TICKET_FOR_{ticketRequest}";
                Console.WriteLine($"Получен запрос на билет для: {ticketRequest}. Билет: {ticket}");

                string response = "TICKET_GRANTED: " + ticket;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                clientSocket.Send(responseBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке запроса на получение билета: " + ex.Message);
        }
        finally
        {
            clientSocket.Close();
        }
    }

    static void StartServiceServer()
    {
        try
        {
            var listener = new TcpListener(IPAddress.Any, ServiceServerPort);
            listener.Start();

            Console.WriteLine("Service Server (SS) is waiting for requests...");

            while (true)
            {
                var clientSocket = listener.AcceptSocket();
                Thread clientThread = new Thread(() => HandleServiceRequest(clientSocket));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при запуске сервера услуг: " + ex.Message);
        }
    }

    static void HandleServiceRequest(Socket clientSocket)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = clientSocket.Receive(buffer);
            string clientRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("SS received: " + clientRequest);

            if (clientRequest.StartsWith("SERVICE_REQUEST"))
            {
                string serviceRequest = clientRequest.Split(' ')[1];
                string response = "SERVICE_ACCESS_GRANTED: " + serviceRequest;

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                clientSocket.Send(responseBytes);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обработке запроса на доступ к сервису: " + ex.Message);
        }
        finally
        {
            clientSocket.Close();
        }
    }

    static bool AuthenticateUser(string clientId, string password)
    {
        try
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT PasswordHash FROM Users WHERE Id = @Id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", clientId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader.GetString(0);
                            string passwordHash = ComputeHash(password);
                            return storedPasswordHash == passwordHash;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при аутентификации пользователя: " + ex.Message);
        }
        return false;
    }

    static string ComputeHash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            string hash = Convert.ToBase64String(hashBytes);
            Console.WriteLine($"Вычисленный хэш для входа: {hash}");
            return hash;
        }
    }
}
