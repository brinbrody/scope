using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScopeServer
{
    public class EmailSettings
    {
        public const string Email = "Email";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 25;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string AlertAddress { get; set; } = string.Empty;
    }
}
