using NautaManager.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NautaManager.Repositories
{
    public interface IUsersRepository
    {
        List<UserSession> GetAll();
        bool AddSession(UserSession user);
        bool RemoveSession(string username);
        bool UpdateSession(string oldName, string newName);
        bool SaveChanges();
    }
}
