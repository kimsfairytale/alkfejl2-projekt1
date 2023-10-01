using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWDVault
{
    public class Vault
    {

        [Name("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Name("username")]
        public string WebUserName { get; set; } = string.Empty;

        [Name("password")]
        public string WebPassword { get; set; } = string.Empty;

        [Name("website")]
        public string Website { get; set; } = string.Empty;
    }
}
