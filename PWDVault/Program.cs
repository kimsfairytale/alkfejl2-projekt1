using CsvHelper.Configuration;
using CsvHelper;
using System.Diagnostics;
using System.Globalization;

namespace PWDVault
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            // Ellenőrizzük, hogy legalább egy argumentumot megadtak-e
            if (args.Length == 0)
            {
                Console.WriteLine("Nincsenek parancssori argumentumok.");
                return;
            }

            // Az első argumentum tartalmazza a munkakönyvtár (workdir) elérési útját
            string workdir = args[0];

            // Most példányosíthatjuk a PasswordManager osztályt
            var passwordManager = new PasswordManager(workdir);

            // Ellenőrizzük, hogy melyik parancsot adták meg, és végezzük el a megfelelő műveleteket
            if (args[1] == "register")
            {
                // Regisztrációs parancs
                if (args.Length < 7)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];
                string email = args[4];
                string firstname = args[5];
                string lastname = args[6];

                // Regisztráljuk a felhasználót
                passwordManager.RegisterUser(username, password, email, firstname, lastname);
            }
            else if (args[1] == "list")
            {
                // Listázási parancs
                if (args.Length < 4)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];

                // Ellenőrizzük a felhasználói azonosítást
                if (passwordManager.AuthenticateUser(username, password))
                {
                    // Listázzuk a felhasználó jelszavait
                    passwordManager.ListUserPasswords(username);
                }
                else
                {
                    Console.WriteLine("Hibás felhasználónév vagy jelszó.");
                }
            }
            else if (args[1] == "delete")
            {
                // Listázási parancs
                if (args.Length < 4)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];

                // Ellenőrizzük a felhasználói azonosítást
                if (passwordManager.AuthenticateUser(username, password))
                {
                    // Listázzuk a felhasználó jelszavait
                    passwordManager.DeleteUser(username);
                }
                else
                {
                    Console.WriteLine("Hibás felhasználónév vagy jelszó.");
                }
            }
            else if (args[1] == "addpassword")
            {
                // Listázási parancs
                if (args.Length < 4)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];
                string newUsername = args[4];
                string newPassword = args[5];
                string newWebsite = args[6];

                // Ellenőrizzük a felhasználói azonosítást
                if (passwordManager.AuthenticateUser(username, password))
                {
                    // Listázzuk a felhasználó jelszavait
                    passwordManager.RegisterPassword(username, newUsername, newPassword, newWebsite);
                }
                else
                {
                    Console.WriteLine("Hibás felhasználónév vagy jelszó.");
                }
            }
            else if (args[1] == "deletepassword")
            {
                // Listázási parancs
                if (args.Length < 4)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];
                string vaultUsername = args[4];

                // Ellenőrizzük a felhasználói azonosítást
                if (passwordManager.AuthenticateUser(username, password))
                {
                    // Listázzuk a felhasználó jelszavait
                    passwordManager.DeletePassword(username, vaultUsername);
                }
                else
                {
                    Console.WriteLine("Hibás felhasználónév vagy jelszó.");
                }
            }
            else if (args[1] == "deletuserpasswords")
            {
                // Listázási parancs
                if (args.Length < 4)
                {
                    Console.WriteLine("Hiányzó felhasználónév vagy jelszó.");
                    return;
                }

                string username = args[2];
                string password = args[3];

                // Ellenőrizzük a felhasználói azonosítást
                if (passwordManager.AuthenticateUser(username, password))
                {
                    // Listázzuk a felhasználó jelszavait
                    passwordManager.DeleteAllPassword(username);
                }
                else
                {
                    Console.WriteLine("Hibás felhasználónév vagy jelszó.");
                }
            }
            else
            {
                Console.WriteLine("Ismeretlen parancs: " + args[1]);
            }
        }
    }
}