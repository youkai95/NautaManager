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
        /// <summary>
        /// Current active/connected session
        /// </summary>
        UserSession ActiveSession { get; }

        /// <summary>
        /// Login a Nauta session to access internet (com.cu) or national network (co.cu)
        /// </summary>
        /// <param name="session">The session to log in</param>
        /// <returns>The result status of the login process, including some user information</returns>
        Task<LoginResult> LoginUser(UserSession session);
        /// <summary>
        /// Create and store a new user session
        /// </summary>
        /// <param name="username">Session email (username)</param>
        /// <param name="password">Session password</param>
        /// <returns>The new object that represent the new created session or null in case of the session is already created</returns>
        UserSession CreateSession(string username, string password);
        /// <summary>
        /// Removes an existing session from the application
        /// </summary>
        /// <param name="username">The email (username) of the account to remove</param>
        /// <returns>Returns true if the account exists and were removed, otherwise false</returns>
        bool RemoveSession(string username);
        /// <summary>
        /// Removes an existing session from the application
        /// </summary>
        /// <param name="userSession">Session object to be removed</param>
        /// <returns>true if the session exists and were removed, false otherwise</returns>
        bool RemoveSession(UserSession userSession);
        /// <summary>
        /// Logs out from the active session
        /// </summary>
        /// <returns>The result status of the logout process</returns>
        Task<LogoutResult> CloseSession();
        /// <summary>
        /// Stores the updated information from the session with <paramref name="oldName"/> as email (username)
        /// </summary>
        /// <param name="oldName">The current account email (username)</param>
        /// <param name="newName">The new account email (username). Could be the same as <paramref name="oldName"/></param>
        /// <returns>true if the update was successful, false if <paramref name="oldName"/> dont exists or <paramref name="newName"/> already exists</returns>
        bool UpdateSession(string oldName, string newName);
        /// <summary>
        /// Save the current status of the sessions stored
        /// </summary>
        /// <returns></returns>
        bool Save();
        /// <summary>
        /// Retrieve all the sessions contained in the application
        /// </summary>
        /// <returns>The session list</returns>
        List<UserSession> GetAll();
        /// <summary>
        /// Disconnect the active account and connect <paramref name="newSession"/>
        /// </summary>
        /// <param name="newSession">Account to change to</param>
        /// <returns>The result status of the swap process</returns>
        Task<NotificationResult> SwapSessions(UserSession newSession);
        /// <summary>
        /// Tries to update <paramref name="session"/> available credit. This procedure must be executed with no active session.
        /// </summary>
        /// <param name="session">The session to check</param>
        /// <returns>An string representation of the available credit</returns>
        Task<string> UpdateSessionCredit(UserSession session);
        /// <summary>
        /// Tries to update <paramref name="session"/> available time. This procedure must be executed with an active session.
        /// </summary>
        /// <param name="session">The session to check</param>
        /// <returns>An string representation of the available time</returns>
        Task<string> UpdateSessionTime(UserSession session);
    }
}
