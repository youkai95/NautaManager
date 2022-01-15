using NautaManager.Models;
using NautaManager.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NautaManager.Handlers
{
    public interface IUserManager
    {
        UserSession ActiveSession { get; set; }
        Task<LoginResult> LoginUser(UserSession session);
        UserSession CreateSession(string username, string password);
        bool RemoveSession(string username);
        Task<LogoutResult> CloseSession();
        bool UpdateSession(string oldName, string newName);
        bool Save();
        List<UserSession> GetAll();
        bool RemoveSession(UserSession userSession);
        Task<NotificationResult> SwapSessions(UserSession newSession);
        Task<string> UpdateSessionCredit(UserSession session);
        Task<string> UpdateSessionTime(UserSession session);
    }
}
