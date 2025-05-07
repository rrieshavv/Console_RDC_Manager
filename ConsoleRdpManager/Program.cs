
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleRdpManager;

class Server
{
    public string Name { get; set; }
    public string IP { get; set; }
    public string EncryptedUsername { get; set; }
    public string EncryptedPassword { get; set; }

    [JsonIgnore]
    public string Username => SecureHelper.Decrypt(EncryptedUsername);

    [JsonIgnore]
    public string Password => SecureHelper.Decrypt(EncryptedPassword);


    public static Server Create(string name, string ip, string username, string password)
    {
        return new Server
        {
            Name = name,
            IP = ip,
            EncryptedUsername = SecureHelper.Encrypt(username),
            EncryptedPassword = SecureHelper.Encrypt(password)
        };
    }
}

class Program
{
    static readonly string appFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ServerManager");
    static readonly string serverFilePath = Path.Combine(appFolder, "servers.json");
    static readonly string passwordFilePath = Path.Combine(appFolder, "auth.dat");
    static readonly string logFilePath = Path.Combine(appFolder, "log.txt");

    static List<Server> servers = new();

    static void Main()
    {
        Directory.CreateDirectory(appFolder);

        if (!File.Exists(passwordFilePath))
        {
            AuthHelper.SetPassword(passwordFilePath);
        }
        else
        {
            int attempts = 0;
            while (!AuthHelper.CheckPassword(passwordFilePath))
            {
                Console.WriteLine("Incorrect password. Try again.");
                attempts++;

                if (attempts >= 5)
                {
                    Console.WriteLine("Too many failed attempts. Exiting.");
                    return;
                }
            }
        }

        LoadServers();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("==== Server Manager ====");
            Console.WriteLine("1. View Servers");
            Console.WriteLine("2. Add Server");
            Console.WriteLine("3. Edit Server");
            Console.WriteLine("4. Delete Server");
            Console.WriteLine("5. Connect to Server (RDP)");
            Console.WriteLine("6. Change App Password");
            Console.WriteLine("7. Exit");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ViewServers(); break;
                case "2": AddServer(); break;
                case "3": EditServer(); break;
                case "4": DeleteServer(); break;
                case "5": ConnectServer(); break;
                case "6": AuthHelper.ChangePassword(passwordFilePath); break;
                case "7": return;
                default: Console.WriteLine("Invalid choice."); break;
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }
    }

    static void LoadServers()
    {
        if (File.Exists(serverFilePath))
        {
            var json = File.ReadAllText(serverFilePath);
            servers = JsonSerializer.Deserialize<List<Server>>(json) ?? new List<Server>();
        }
    }

    static void SaveServers()
    {
        var json = JsonSerializer.Serialize(servers, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(serverFilePath, json);
    }

    static void ViewServers()
    {
        if (servers.Count == 0)
        {
            Console.WriteLine("No servers found.");
            return;
        }

        for (int i = 0; i < servers.Count; i++)
        {
            var s = servers[i];
            Console.WriteLine($"{i + 1}. Name: {s.Name}, IP: {s.IP}, Username: {s.Username}");
        }
    }

    static void AddServer()
    {
        Console.Write("Enter server name: ");
        var name = Console.ReadLine();

        Console.Write("Enter IP: ");
        var ip = Console.ReadLine();

        Console.Write("Enter username: ");
        var user = Console.ReadLine();

        Console.Write("Enter password: ");
        var pass = Console.ReadLine();

        servers.Add(Server.Create(name, ip, user, pass));


        SaveServers();
        Console.WriteLine("Server added.");
    }

    static void EditServer()
    {
        ViewServers();
        Console.Write("Enter server number to edit: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= servers.Count)
        {
            var server = servers[index - 1];
            Console.Write($"New Name ({server.Name}): ");
            var name = Console.ReadLine();
            Console.Write($"New IP ({server.IP}): ");
            var ip = Console.ReadLine();
            Console.Write($"New Username ({server.Username}): ");
            var user = Console.ReadLine();
            Console.Write($"New Password: ");
            var pass = Console.ReadLine();

            server.Name = string.IsNullOrWhiteSpace(name) ? server.Name : name;
            server.IP = string.IsNullOrWhiteSpace(ip) ? server.IP : ip;
            if (!string.IsNullOrWhiteSpace(user))
                server.EncryptedUsername = SecureHelper.Encrypt(user);
            if (!string.IsNullOrWhiteSpace(pass))
                server.EncryptedPassword = SecureHelper.Encrypt(pass);


            SaveServers();
            Console.WriteLine("Server updated.");
        }
        else
        {
            Console.WriteLine("Invalid index.");
        }
    }

    static void DeleteServer()
    {
        ViewServers();
        Console.Write("Enter server number to delete: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= servers.Count)
        {
            servers.RemoveAt(index - 1);
            SaveServers();
            Console.WriteLine("Server deleted.");
        }
        else
        {
            Console.WriteLine("Invalid index.");
        }
    }

    static void ConnectServer()
    {
        ViewServers();
        Console.Write("Enter server number to connect: ");
        if (int.TryParse(Console.ReadLine(), out int index) && index > 0 && index <= servers.Count)
        {
            var server = servers[index - 1];

            // Store credentials
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmdkey",
                Arguments = $"/add:{server.IP} /user:{server.Username} /pass:{server.Password}",
                CreateNoWindow = true,
                UseShellExecute = false
            })?.WaitForExit();

            // Launch RDP
            Process.Start("mstsc", $"/v:{server.IP}");

            Console.WriteLine($"Opening Remote Desktop to {server.IP}...");
            Logger.Log(logFilePath, $"Connected to {server.Name} ({server.IP})");
        }
        else
        {
            Console.WriteLine("Invalid index.");
        }
    }

}
