using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using PWDVault;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class PasswordManager
{

    private string usersFilePath;
    private string vaultFilePath;

    public PasswordManager(string workdir)
    {
        // Ellenőrizzük, hogy a workdir mappában léteznek-e a fájlok, és ha nem, létrehozzuk őket
        usersFilePath = Path.Combine(workdir, "user.csv");
        vaultFilePath = Path.Combine(workdir, "vault.csv");

        if (!File.Exists(usersFilePath))
        {
            File.Create(usersFilePath).Close();
        }

        if (!File.Exists(vaultFilePath))
        {
            File.Create(vaultFilePath).Close();
        }
    }

    public bool RegisterUser(string username, string password, string email, string firstname, string lastname)
    {
        // Ellenőrizzük, hogy a felhasználónév már foglalt-e
        if (UserExists(username))
        {
            Console.WriteLine("A felhasználónév már foglalt.");
            return false;
        }

        // Generáljuk a jelszó hash-t
        string passwordHash = HashPassword(password);

        // Regisztráljuk az új felhasználót
        var user = new User
        {
            UserName = username,
            PasswordHash = passwordHash,
            Email = email,
            FirstName = firstname,
            LastName = lastname
        };

        bool mode = true;
        using (StreamWriter writer = new(usersFilePath, append: mode))
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };
            using CsvWriter csv = new(writer, config);
            csv.WriteRecord(user);
            csv.NextRecord();
        }

        Console.WriteLine("A regisztráció sikeres.");
        return true;
    }

    public bool AuthenticateUser(string username, string password)
    {
        // Ellenőrizzük, hogy a felhasználó létezik
        var user = GetUserByUsername(username);
        if (user == null)
        {
            return false;
        }

        // Ellenőrizzük a jelszó helyességét
        string hashedPassword = HashPassword(password);
        return user.PasswordHash == hashedPassword;
    }

    public void ListUserPasswords(string username)
    {
        // Ellenőrizzük, hogy a felhasználó létezik
        var user = GetUserByUsername(username);
        if (user == null)
        {
            Console.WriteLine("A felhasználó nem található.");
            return;
        }

        // Listázzuk a felhasználó jelszavait
        var vaultEntries = GetVaultEntriesByUserName(user.UserName);
        if (vaultEntries.Any())
        {
            Console.WriteLine($"{username} jelszavai:");

            foreach (var vaultEntry in vaultEntries)
            {
                Console.WriteLine($"- ({vaultEntry.Website}) {vaultEntry.WebUserName}: {vaultEntry.WebPassword}");
            }
        }
        else
        {
            Console.WriteLine($"{username} nincsenek tárolt jelszavak.");
        }
    }

    public bool DeleteUser(string username)
    {
        // Ellenőrizzük, hogy a felhasználó létezik
        var user = GetUserByUsername(username);
        if (user == null)
        {
            Console.WriteLine("A felhasználó nem található.");
            return false;
        }

        // Töröljük a felhasználót a listából
        List<User> users = ReadUsersFromCsv();
        users.RemoveAll(u => u.UserName == username);

        // Írjuk felül a users.csv fájlt a frissített felhasználói listával
        using (var writer = new StreamWriter(usersFilePath, false))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecords(users);
        }

        Console.WriteLine($"A felhasználó '{username}' sikeresen törölve lett.");
        return true;
    }

    public bool RegisterPassword(string username, string newUsername, string newPassword, string newWebsite)
    {

        // Keresse meg a felhasználót a felhasználónevével
        var user = GetUserByUsername(username);
        if (user == null)
        {
            Console.WriteLine("A felhasználó nem található.");
            return false;
        }

        // Generáljuk az új jelszót
        string encryptedPassword = HashPassword(newPassword);

        // Regisztráljuk az új jelszót a vault.csv fájlban
        var vaultEntry = new Vault
        {
            UserId = user.UserName,
            WebUserName = newUsername,
            WebPassword = encryptedPassword,
            Website = newWebsite
        };

        using (var writer = new StreamWriter(vaultFilePath, append: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            csv.WriteRecord(vaultEntry);
            csv.NextRecord();
        }

        Console.WriteLine("Az új jelszó sikeresen regisztrálva lett.");
        return true;
    }

    public bool DeletePassword(string username, string vaultUsername)
    {

        // Keresse meg a felhasználót a felhasználónevével
        var user = GetUserByUsername(username);
        if (user == null)
        {
            Console.WriteLine("A felhasználó nem található.");
            return false;
        }

        // Töröljük a felhasználó jelszavát a vault.csv fájlból
        List<Vault> vaultEntries = ReadVaultEntriesFromCsv();
        var entriesToDelete = vaultEntries.Where(ve => ve.WebUserName == vaultUsername).ToList();

        if (entriesToDelete.Any())
        {
            foreach (var entryToDelete in entriesToDelete)
            {
                vaultEntries.Remove(entryToDelete);
            }

            // Írjuk felül a vault.csv fájlt a frissített bejegyzésekkel
            using (var writer = new StreamWriter(vaultFilePath, false))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(vaultEntries);
            }

            Console.WriteLine($"A felhasználó '{vaultUsername}' jelszava sikeresen törölve lett.");
            return true;
        }
        else
        {
            Console.WriteLine($"A felhasználó '{username}' nem rendelkezik jelszavakkal.");
            return false;
        }
    }

    public bool DeleteAllPassword(string username)
    {

        // Keresse meg a felhasználót a felhasználónevével
        var user = GetUserByUsername(username);
        if (user == null)
        {
            Console.WriteLine("A felhasználó nem található.");
            return false;
        }

        // Töröljük a felhasználó jelszavát a vault.csv fájlból
        List<Vault> vaultEntries = ReadVaultEntriesFromCsv();
        var entriesToDelete = vaultEntries.Where(ve => ve.UserId == user.UserName).ToList();

        if (entriesToDelete.Any())
        {
            foreach (var entryToDelete in entriesToDelete)
            {
                vaultEntries.Remove(entryToDelete);
            }

            // Írjuk felül a vault.csv fájlt a frissített bejegyzésekkel
            using (var writer = new StreamWriter(vaultFilePath, false))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(vaultEntries);
            }

            Console.WriteLine($"A felhasználó '{username}' összes jelszava sikeresen törölve lett.");
            return true;
        }
        else
        {
            Console.WriteLine($"A felhasználó '{username}' nem rendelkezik jelszavakkal.");
            return false;
        }
    }


    private bool UserExists(string username)
    {
        List<User> users = ReadUsersFromCsv();
        return users.Any(u => u.UserName == username);
    }

    private string HashPassword(string password)
    {
        // Jelszó hashelése SHA-512 algoritmussal
        using (SHA512 sha512 = SHA512.Create())
        {
            byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashBytes);
        }
    }

    private User GetUserByUsername(string username)
    {
        List<User> users = ReadUsersFromCsv();
        return users.FirstOrDefault(predicate: u => u.UserName == username);
    }

    private List<Vault> GetVaultEntriesByUserName(string username)
    {
        List<Vault> vaultEntries = ReadVaultEntriesFromCsv();
        return vaultEntries.Where(ve => ve.UserId == username).ToList();
    }

    private List<User> ReadUsersFromCsv()
    {
        // Olvassa be a felhasználókat a users.csv fájlból
        using (var reader = new StreamReader(usersFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            return csv.GetRecords<User>().ToList();
        }
    }

    private List<Vault> ReadVaultEntriesFromCsv()
    {
        // Olvassa be a tárolt jelszavakat a vault.csv fájlból
        using (var reader = new StreamReader(vaultFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            return csv.GetRecords<Vault>().ToList();
        }
    }
}