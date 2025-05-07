using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRdpManager
{
    public static class AuthHelper
    {
        public static string Hash(string input) => Convert.ToBase64String(
            System.Security.Cryptography.SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(input)));

        public static void SetPassword(string filePath)
        {
            Console.Write("Set new app password: ");
            var pass = Console.ReadLine();
            File.WriteAllText(filePath, Hash(pass));
            Console.WriteLine("Password saved.");
        }

        public static bool CheckPassword(string filePath)
        {
            Console.Write("Enter app password: ");
            var pass = ReadPassword();
            var storedHash = File.ReadAllText(filePath);
            return Hash(pass) == storedHash;
        }

        public static void ChangePassword(string filePath)
        {
            Console.Write("Enter current password: ");
            var current = Console.ReadLine();
            if (Hash(current) == File.ReadAllText(filePath))
            {
                Console.Write("Enter new password: ");
                var newPass = Console.ReadLine();
                File.WriteAllText(filePath, Hash(newPass));
                Console.WriteLine("Password changed.");
            }
            else
            {
                Console.WriteLine("Incorrect current password.");
            }
        }

        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password[..^1];
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password += key.KeyChar;
                    Console.Write("");
                }
            }

            return password;
        }

    }
}
