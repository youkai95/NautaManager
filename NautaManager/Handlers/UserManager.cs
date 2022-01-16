using NautaManager.Models;
using NautaManager.Models.Abstractions;
using NautaManager.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NautaManager.Handlers
{
    public class UserManager : IUserManager
    {
        public UserSession ActiveSession { get; set; }
        IUsersRepository Repository { get; }
        const string CheckUrl = "http://www.cubadebate.cu/";
        public UserManager(IUsersRepository _repo)
        {
            Repository = _repo;
        }

        public async Task<LoginResult> LoginUser(UserSession session)
        {
            if(ActiveSession == null && await IsConnectedAsync())
            {
                return new LoginResult { Result = false, Message = "Ya hay una sesion abierta" };
            }
            var r = await session.TryLogin();
            if (r)
            {
                ActiveSession = session;
                Repository.SaveChanges();
            }
            return r;
        }

        public UserSession CreateSession(string username, string password)
        {
            var model = new UserSession()
            {
                Username = username,
                Password = password,
            };
            if(Repository.AddSession(model))
                return model;
            return null;
        }

        public bool RemoveSession(string username)
        {
            return Repository.RemoveSession(username);
        }

        public async Task<LogoutResult> CloseSession()
        {
            if (ActiveSession != null)
            {
                var r = await ActiveSession.TryLogout();
                if (r)
                {
                    ActiveSession = null;
                    Repository.SaveChanges();
                }
                return r;
            }
            return new LogoutResult { Result = false, Message = "No existe sesion abierta" };
        }

        public bool UpdateSession(string oldName, string newName)
        {
            if (Repository.UpdateSession(oldName, newName))
            {
                if(File.Exists(oldName))
                    File.Move(oldName, newName);
                return true;
            }
            return false;
        }

        public bool Save() => Repository.SaveChanges();

        public List<UserSession> GetAll()
        {
            var sessions = Repository.GetAll();
            foreach (var item in sessions)
            {
                if (item.ATTRIBUTE_UUID != null)
                    ActiveSession = item;
            }
            return sessions;
        }

        public static async Task<bool> IsConnectedAsync()
        {
            try
            {
                HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
                var response = await client.GetAsync(CheckUrl);
                var str = await response.Content.ReadAsStringAsync();
                if (str.Contains("https://secure.etecsa.net:8443"))
                    return false;
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public static bool IsConnected()
        {
            try
            {
                HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
                var response = client.GetAsync(CheckUrl).Result;
                var str = response.Content.ReadAsStringAsync().Result;
                if (str.Contains("https://secure.etecsa.net:8443"))
                    return false;
                return true;
            }
            catch (AggregateException)
            {
                return true;
            }
        }

        public bool RemoveSession(UserSession userSession)
        {
            return RemoveSession(userSession.Username);
        }

        public async Task<NotificationResult> SwapSessions(UserSession newSession)
        {
            if (newSession == null || newSession.Equals(ActiveSession))
                return new SwapResult() { Result = false, Message = "Se necesita una sesion diferente a la activa" };

            NotificationResult logoutResult = await CloseSession();
            for (int i = 0; i < 2; i++)
            {
                if (logoutResult.Result)
                    break;
                logoutResult = await CloseSession();
            }

            if (!logoutResult)
                return logoutResult;

            if (await IsConnectedAsync())
                return new SwapResult() { Result = false, Message = "La cuenta se ha cerrado pero aun hay conexion?!" };

            var loginResult = await newSession.TryLogin();
            if (loginResult.Result)
                ActiveSession = newSession;
            
            return loginResult;
        }

        public Task<string> UpdateSessionCredit(UserSession session)
        {
            return session.UpdateAccountCredit();
        }
        public Task<string> UpdateSessionTime(UserSession session)
        {
            return session.UpdateAccountTime();
        }
    }
}
