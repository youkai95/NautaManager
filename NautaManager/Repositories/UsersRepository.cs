using NautaManager.Factories;
using NautaManager.Interfaces;
using NautaManager.Models;
using NautaManager.Repositories.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautaManager.Repositories
{
    class UsersRepository : IUsersRepository, IAsyncInitialization
    {
        string _sessionsStorage = "sessions.json";
        Dictionary<string, UserSession> SessionsCache = new Dictionary<string, UserSession>();
        public Task Initialized { get; }

        public UsersRepository()
        {
            Initialized = Initialize();
        }

        private async Task Initialize()
        {
            if (!File.Exists(_sessionsStorage))
                return;
            using var file = new StreamReader(File.OpenRead(_sessionsStorage));
            var json = JsonConvert.DeserializeObject<Dictionary<string, UserSessionStore>>(file.ReadToEnd());
            if (json == null)
                return;
            
            foreach (var kv in json)
                SessionsCache.Add(kv.Key, await SessionFactory.LoadSessionAsync(kv.Value));
        }

        public async Task<List<UserSession>> GetAllAsync()
        {
            await Initialized;
            return SessionsCache.Values.ToList();
        }
 
        public async Task<bool> AddSession(UserSession user)
        {
            if (SessionsCache.TryAdd(user.Username, user))
            {
                user.InitClientSession();
                await user.InitSession();
                SaveChanges();
                return true;
            }
            return false;
        }

        public bool RemoveSession(string username)
        {
            if (SessionsCache.Remove(username))
            {
                if(File.Exists(username))
                    File.Delete(username);
                SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateSession(string oldName, string newName)
        {
            if (SessionsCache.TryGetValue(oldName, out var session) && !SessionsCache.ContainsKey(newName))
            {
                SessionsCache.Remove(oldName);
                if (SessionsCache.TryAdd(newName, session))
                {
                    SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public bool SaveChanges()
        {
            using var file = new StreamWriter(File.Open(_sessionsStorage, FileMode.Create));
            var dict = new Dictionary<string, UserSessionStore>();
            foreach(var session in SessionsCache.Values)
            {
                dict.Add(session.Username, SessionFactory.CastSessionToStore(session));
            }
            file.Write(JsonConvert.SerializeObject(dict));
            return true;
        }
    }
}