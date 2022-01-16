using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NautaManager.Repositories.Models
{
    public class UserSessionStore
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ATTRIBUTE_UUID { get; set; }
        public string CSRFHW { get; set; }
        public string wlanuserip { get; set; }

        public AccountStatus Status { get; set; }
    }
}
