using System;
using System.Collections.Generic;
using System.Text;

namespace NautaManager.Models.Abstractions
{
    public abstract class NotificationResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }

        public static implicit operator bool(NotificationResult r) => r.Result;
    }
}
