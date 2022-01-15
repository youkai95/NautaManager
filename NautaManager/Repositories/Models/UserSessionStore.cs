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

        public static implicit operator UserSessionStore(UserSession session)
        {
            var model = new UserSessionStore()
            {
                Username = session.Username,
                Password = session.GetHashedPassword(),
                Status = session.Status,
                CSRFHW = session.CSRFHW,
                ATTRIBUTE_UUID = session.ATTRIBUTE_UUID,
                wlanuserip = session.wlanuserip
            };
            SaveHTTPClient(session.Username, session.CookieContainer);
            return model;
        }

        /// <summary>
        /// this function loads the CookieContainer that contains the Session Cookies
        /// </summary>
        public static CookieContainer LoadHTTPClient(string name)
        {
            if (File.Exists(name))
            {
                // LOAD
                var CookieJar = new CookieContainer();
                FileStream inStr = new FileStream(name, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                CookieJar = bf.Deserialize(inStr) as CookieContainer;
                inStr.Close();
                return CookieJar;
            }
            return null;
        }

        /// <summary>
        /// this function saves the sessioncookies to file
        /// </summary>
        /// <param name="CookieJar"></param>
        public static void SaveHTTPClient(string name, CookieContainer CookieJar)
        {
            // SAVE client Cookies
            FileStream stream = new FileStream(name, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, CookieJar);
            stream.Close();
        }
    }
}
