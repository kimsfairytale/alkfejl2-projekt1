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
                // Visszafejthetjük a jelszavakat a felhasználói jelszóval
                string decryptedPassword = DecryptPassword(vaultEntry.WebPassword, user.PasswordHash);
                Console.WriteLine($"- {vaultEntry.UserId}: {decryptedPassword}");
            }
        }
        else
        {
            Console.WriteLine($"{username} nincsenek tárolt jelszavak.");
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

    private string DecryptPassword(string encryptedPassword, string userPasswordHash)
    {
        using (Aes aesAlg = Aes.Create())
        {
            // Az userPasswordHash értékét használjuk kulcsként
            aesAlg.Key = Encoding.UTF8.GetBytes(userPasswordHash);

            // Az első 16 byte a IV (Inicializációs Vektor) része
            byte[] iv = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(encryptedPassword.Substring(0, 16)), iv, 16);
            aesAlg.IV = iv;

            // A többi rész tartalmazza az enkriptált jelszót
            byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword.Substring(16));

            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
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