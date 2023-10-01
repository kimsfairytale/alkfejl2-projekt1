using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Security.Cryptography;

namespace PWDVault
{
    public class User
    {

        [Name("username")]
        public string UserName { get; set; } = string.Empty;

        [Name("password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Name("email")]
        public string Email { get; set; } = string.Empty;

        [Name("firstname")]
        public string FirstName { get; set; } = string.Empty;

        [Name("lastname")]
        public string LastName { get; set; } = string.Empty;

    }
}
