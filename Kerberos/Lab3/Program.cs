using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    private const int KeySize = 8;

    static void Main()
    {
        string clientId = "clientC";
        string tgsId = "TGS";
        string ssId = "SS";

        // Шаг 1: Клиент отправляет идентификатор серверу AS
        Console.WriteLine($"Клиент {clientId} отправляет запрос на аутентификацию серверу AS.");

        // Шаг 2: Сервер AS выдает TGT
        byte[] clientKey = GenerateKey();
        string tgt = GenerateTGT(clientId, tgsId);
        Console.WriteLine($"AS выдает TGT: {tgt}");

        // Шаг 3: Клиент отправляет TGT и аутентификационный блок серверу TGS
        string authRequest1 = CreateAuthRequest(clientId);
        string tgsResponse = SendToTGS(tgt, authRequest1);
        Console.WriteLine($"Клиент получает ответ от TGS: {tgsResponse}");

        // Шаг 4: Клиент отправляет билет и аутентификационный блок серверу SS
        string authRequest2 = CreateAuthRequest(clientId);
        string ssResponse = SendToSS(tgsResponse, authRequest2);
        Console.WriteLine($"Сервер SS отвечает: {ssResponse}");
    }

    static byte[] GenerateKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[KeySize];
            rng.GetBytes(key);
            return key;
        }
    }

    static string GenerateTGT(string clientId, string tgsId)
    {
        return Encrypt($"{{ClientId:{clientId}, TgsId:{tgsId}, Timestamp:{DateTime.UtcNow}}}");
    }

    static string CreateAuthRequest(string clientId)
    {
        return $"{clientId},{DateTime.UtcNow}";
    }

    static string SendToTGS(string tgt, string authRequest)
    {
        return Encrypt($"TGS Response: {tgt} | AuthRequest: {authRequest}");
    }

    static string SendToSS(string tgsResponse, string authRequest)
    {
        return $"SS Response: {tgsResponse} | AuthRequest: {authRequest}";
    }

    static string Encrypt(string data)
    {
        using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
        {
            byte[] key = GenerateKey();
            des.Key = key;
            des.IV = key; 

            ICryptoTransform encryptor = des.CreateEncryptor();
            byte[] input = Encoding.UTF8.GetBytes(data);
            byte[] output = encryptor.TransformFinalBlock(input, 0, input.Length);
            return Convert.ToBase64String(output);
        }
    }
}