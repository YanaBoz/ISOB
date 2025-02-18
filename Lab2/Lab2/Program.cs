using System.Text;

class Crypted
{
    public string Text { get; set; }
    public int? Shift { get; set; }
    public string? Key { get; set; }
}

class Memory
{
    public int Id { get; set; }
    public string EncryptedText { get; set; }  
    public string CipherType { get; set; }  
    public int? Shift { get; set; }  
    public string? Key { get; set; }  
}

class Program
{
    static List<Memory> memoryList = new List<Memory>();
    static int memoryCounter = 1;

    static void Main()
    {
        Crypted crypted = new Crypted();
        while (true)
        {
            Console.Clear();
            int menu = MainMenu();
            if (menu == 0) return;

            if (menu == 1)
            {
                crypted = AddNewPassword();
            }
            else if (menu == 2)
            {
                EncryptDecryptPassword();
                Console.ReadLine();
            }
            else if (menu == 3)
            {
                DecryptPasswordFromMemory();
                Console.ReadLine();
            }
            else if (menu == 4)
            {
                ShowAllPasswords();
                Console.ReadLine();
            }
            else if (menu == 5)
            {
                DeletePassword();
            }
        }
    }

    static int MainMenu()
    {
        Console.WriteLine("Choose an option:");
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Add new password");
        Console.WriteLine("2. Encrypt/Decrypt");
        Console.WriteLine("3. Decrypt password from memory");
        Console.WriteLine("4. Show all passwords");
        Console.WriteLine("5. Delete password");
        int choice = int.Parse(Console.ReadLine());
        return choice == 0 || choice == 1 || choice == 2 || choice == 3 || choice == 4 || choice == 5 ? choice : 0;
    }

    static int Menu2()
    {
        Console.WriteLine("Select encryption type:");
        Console.WriteLine("1. Caesar Cipher");
        Console.WriteLine("2. Vigenere Cipher");
        int choice = int.Parse(Console.ReadLine());
        return choice == 1 || choice == 2 ? choice : 0;
    }

    static Crypted AddNewPassword()
    {
        Crypted crypted = new Crypted();
        Console.WriteLine("Enter password:");
        crypted.Text = Console.ReadLine();

        int choice = Menu2(); 
        string encryptedText = "";

        if (choice == 1) 
        {
            Console.WriteLine("Enter shift value for Caesar Cipher:");
            crypted.Shift = int.Parse(Console.ReadLine());
            crypted.Key = null; 
            encryptedText = CaesarCipher(crypted.Text, crypted.Shift.Value);
        }
        else if (choice == 2) 
        {
            Console.WriteLine("Enter key for Vigenere Cipher:");
            crypted.Key = Console.ReadLine();
            crypted.Shift = null; 
            encryptedText = VigenereCipher(crypted.Text, crypted.Key, true);
        }

        Memory memory = new Memory
        {
            Id = memoryCounter++,
            EncryptedText = encryptedText, 
            CipherType = choice == 1 ? "Caesar" : "Vigenere",
            Shift = crypted.Shift,
            Key = crypted.Key
        };
        memoryList.Add(memory);

        return crypted;
    }

    static void EncryptDecryptPassword()
    {
        Console.WriteLine("Enter password to encrypt/decrypt:");
        string text = Console.ReadLine();

        Console.WriteLine("Select encryption type:");
        int choice = Menu2();
        string result = "";

        if (choice == 1) 
        {
            Console.WriteLine("Enter shift value:");
            int shift = int.Parse(Console.ReadLine());
            result = CaesarCipher(text, shift);
            Console.WriteLine($"Encrypted text (Caesar): {result}");
        }
        else if (choice == 2) 
        {
            Console.WriteLine("Enter key for encryption:");
            string key = Console.ReadLine();
            result = VigenereCipher(text, key, true);
            Console.WriteLine($"Encrypted text (Vigenere): {result}");
        }
    }

    static string CaesarCipher(string text, int shift)
    {
        StringBuilder result = new StringBuilder();
        foreach (char c in text)
        {
            if (char.IsLetter(c))
            {
                char offset = char.IsUpper(c) ? 'A' : 'a';
                result.Append((char)(((c - offset + shift + 26) % 26 + 26) % 26 + offset));
            }
            else if (char.IsDigit(c))
            {
                result.Append((char)(((c - '0' + shift + 10) % 10 + 10) % 10 + '0'));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }

    static string VigenereCipher(string text, string key, bool encrypt)
    {
        StringBuilder result = new StringBuilder();
        int keyIndex = 0;
        int direction = encrypt ? 1 : -1;

        foreach (char c in text)
        {
            if (char.IsLetter(c))
            {
                char offset = char.IsUpper(c) ? 'A' : 'a';
                int keyChar = char.ToUpper(key[keyIndex % key.Length]) - 'A';
                result.Append((char)(((c - offset + direction * keyChar + 26) % 26 + 26) % 26 + offset));
                keyIndex++;
            }
            else if (char.IsDigit(c))
            {
                int keyChar = key[keyIndex % key.Length] - '0';
                result.Append((char)(((c - '0' + direction * keyChar + 10) % 10 + 10) % 10 + '0'));
                keyIndex++;
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }

    static void ShowAllPasswords()
    {
        if (memoryList.Count == 0)
        {
            Console.WriteLine("No passwords stored.");
            return;
        }

        Console.WriteLine("\nStored Passwords:");
        foreach (var memory in memoryList)
        {
            Console.WriteLine($"ID: {memory.Id}, Cipher Type: {memory.CipherType}, Encrypted Password: {memory.EncryptedText}");
            if (memory.CipherType == "Caesar")
            {
                Console.WriteLine($"Shift: {memory.Shift}");
            }
            else if (memory.CipherType == "Vigenere")
            {
                Console.WriteLine($"Key: {memory.Key}");
            }
        }
    }

    static void DecryptPasswordFromMemory()
    {
        if (memoryList.Count == 0)
        {
            Console.WriteLine("No passwords in memory to decrypt.");
            return;
        }

        ShowAllPasswords();

        Console.WriteLine("Enter the ID of the password to decrypt:");
        int id = int.Parse(Console.ReadLine());

        var memory = memoryList.Find(m => m.Id == id);
        if (memory == null)
        {
            Console.WriteLine("Invalid ID.");
            return;
        }

        if (memory.CipherType == "Caesar")
        {
            string decryptedText = CaesarCipher(memory.EncryptedText, -(memory.Shift ?? 0));
            Console.WriteLine($"Decrypted Password (Caesar): {decryptedText}");
        }
        else if (memory.CipherType == "Vigenere")
        {
            string decryptedText = VigenereCipher(memory.EncryptedText, memory.Key ?? string.Empty, false);
            Console.WriteLine($"Decrypted Password (Vigenere): {decryptedText}");
        }
    }

    static void DeletePassword()
    {
        if (memoryList.Count == 0)
        {
            Console.WriteLine("No passwords stored to delete.");
            return;
        }

        ShowAllPasswords();

        Console.WriteLine("Enter the ID of the password you want to delete:");
        int id = int.Parse(Console.ReadLine());

        var memory = memoryList.Find(m => m.Id == id);
        if (memory != null)
        {
            memoryList.Remove(memory);
            Console.WriteLine($"Password with ID {id} has been deleted.");
        }
        else
        {
            Console.WriteLine("Password not found.");
        }
    }
}
