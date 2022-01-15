using NautaManager.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace NautaManager.Models
{
    public class LoginResult : NotificationResult
    {
        public AccountStatus Status { get; set; }
    }
}
