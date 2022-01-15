using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NautaManager.Repositories
{
    public interface IUsersRepository
    {
        /// <summary>
        /// Retrieve all the sessions contained in the application. The sessions are only loaded from HDD when application starts up.
        /// </summary>
        /// <returns>The session list</returns>
        List<UserSession> GetAll();
        /// <summary>
        /// Create and store a new user session
        /// </summary>
        /// <param name="user">Session representation to store</param>
        /// <returns>true if the account was successfully created and stored, false otherwise</returns>
        bool AddSession(UserSession user);
        /// <summary>
        /// Removes an existing session from the application
        /// </summary>
        /// <param name="username">The email (username) of the account to remove</param>
        /// <returns>Returns true if the account exists and were removed, otherwise false</returns>
        bool RemoveSession(string username);
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
        bool SaveChanges();
    }
}
