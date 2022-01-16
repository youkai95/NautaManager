using HtmlAgilityPack;
using NautaManager.Handlers;
using NautaManager.Repositories.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NautaManager.Models
{
    public class UserSession : INotifyPropertyChanged
    {
        const string BaseUrl = "https://secure.etecsa.net:8443";

        byte[] _password;
        bool _active = false;
        AccountStatus _status;
        long elapsedSeconds;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        HttpClientHandler handler;

        public string Username { get; set; }
        public bool IsActive
        {
            get => _active;
            set
            {
                _active = value;
                RaisePropertyChanged();
            }
        }
        CookieContainer CookieContainer { get; set; }
        public HttpClient ClientSession { get; private set; }
        public string Password
        {
            get
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(_password, null, DataProtectionScope.CurrentUser));
            }
            set
            {
                _password = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
                RaisePropertyChanged();
            }
        }
        public AccountStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }
        public string ConsumedTime
        {
            get => string.Format("{0:D2}:{1:D2}:{2:D2}", elapsedSeconds / 60 / 60, elapsedSeconds / 60 % 60, elapsedSeconds % 60);
        }
        public string CSRFHW { get; private set; }
        public string wlanuserip { get; private set; }
        public string ATTRIBUTE_UUID { get; private set; }
        public Dictionary<string, string> FormData { get; private set; }
        string LoginURL { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            App.Current?.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }

        public UserSession()
        {
            timer.Elapsed += TickSecond;
        }

        public UserSession(string _CSRFHW, string _ATTRIBUTE_UUID, string _wlanuserip) : this()
        {
            CSRFHW = _CSRFHW;
            ATTRIBUTE_UUID = _ATTRIBUTE_UUID;
            wlanuserip = _wlanuserip;
        }

        private void TickSecond(object sender, ElapsedEventArgs e)
        {
            Interlocked.Increment(ref elapsedSeconds);
            RaisePropertyChanged("ConsumedTime");
        }

        public async Task<bool> InitSession()
        {
            if (await UserManager.IsConnectedAsync())
                return false;
            Dictionary<string, string> formData = new Dictionary<string, string>();
            HttpResponseMessage response;
            try
            {
                response = await ClientSession.GetAsync(BaseUrl);
            }
            catch
            {
                return false;
            }
            HtmlDocument getRequest = new HtmlDocument();
            getRequest.LoadHtml(await response.Content.ReadAsStringAsync());
            var form = getRequest.GetElementbyId("formulario");
            var info = form.ChildNodes.Where(n => n.Name == "input").Select(n => new { Name = n.GetAttributeValue("name", null), Value = n.GetAttributeValue("value", null) });
            foreach (var input in info)
            {
                formData.Add(input.Name, input.Value);
            }
            FormData = formData;
            LoginURL = form.GetAttributeValue("action", null);
            if (formData.TryGetValue("CSRFHW", out var _csrfhw) && formData.TryGetValue("wlanuserip", out var _wlanuserip))
            {
                CSRFHW = _csrfhw;
                wlanuserip = _wlanuserip;
            }
            else
                throw new Exception();
            return true;
        }

        public void InitClientSession()
        {
            CookieContainer = LoadHTTPSession();
            handler = new HttpClientHandler() { CookieContainer = CookieContainer, UseCookies = true };
            ClientSession = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        }

        /// <summary>
        /// this function loads the CookieContainer that contains the Session Cookies
        /// </summary>
        public CookieContainer LoadHTTPSession(string name = null)
        {
            if (File.Exists(name))
            {
                // LOAD
                FileStream inStr = new FileStream(name ?? Username, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                var CookieJar = bf.Deserialize(inStr) as CookieContainer;
                inStr.Close();
                return CookieJar;
            }
            return new CookieContainer();
        }

        /// <summary>
        /// this function saves the sessioncookies to file
        /// </summary>
        /// <param name="CookieJar"></param>
        public void SaveHTTPSession(string name = null)
        {
            // SAVE client Cookies
            FileStream stream = new FileStream(name ?? Username, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, CookieContainer);
            stream.Close();
        }

        internal void LoadPassword(string serializedPassword)
        {
            _password = Convert.FromBase64String(serializedPassword);
        }

        public async Task<LoginResult> TryLogin()
        {
            if(FormData == null && !await InitSession())
            {
                return new LoginResult { Result = false, Message = "No es posible crear una sesion" };
            }
            await UpdateAccountCredit();

            var formData = new Dictionary<string, string>(FormData)
            {
                ["username"] = Username,
                ["password"] = Password
            };
            HttpContent request = new FormUrlEncodedContent(formData);
            HttpResponseMessage response;
            try
            {
                response = await ClientSession.PostAsync(LoginURL, request);
            }
            catch
            {
                return new LoginResult { Result = false, Message = "No es posible conectar con el servicio NAUTA" };
            }
            HtmlDocument parser = new HtmlDocument();
            string strResponse = await response.Content.ReadAsStringAsync();

            parser.LoadHtml(strResponse);
            var node = parser.DocumentNode.SelectSingleNode("(//html/body/script)[last()]");
            if (node != null)
            {
                var regex = Regex.Match(node.InnerText, "alert\\(['\"](.+)['\"]\\)");
                if (regex.Success)
                {
                    return new LoginResult()
                    {
                        Result = false,
                        Message = regex.Groups[1].Value
                    };
                }
            }
            var attrRegex = Regex.Match(strResponse, @"ATTRIBUTE_UUID=?(?<attrib>(\w+))&");
            if(attrRegex.Success)
            {
                ATTRIBUTE_UUID = attrRegex.Groups["attrib"].Value;
            }
            await UpdateAccountTime();
            elapsedSeconds = 0;
            if (!timer.Enabled)
                timer.Start();
            IsActive = true;
            return new LoginResult() { Result = true };
        }

        internal string GetHashedPassword()
        {
            return Convert.ToBase64String(_password);
        }

        public async Task<LogoutResult> TryLogout()
        {
            if(CSRFHW == null || ATTRIBUTE_UUID == null)
            {
                return new LogoutResult { Result = false, Message = "No existen datos de inicio de sesion para esta cuenta" };
            }
            HttpResponseMessage response;
            try
            {
                response = await ClientSession.GetAsync($"{BaseUrl}/LogoutServlet?CSRFHW={CSRFHW}&ATTRIBUTE_UUID={ATTRIBUTE_UUID}&username={Username}&wlanuserip={wlanuserip}");
            }
            catch
            {
                return new LogoutResult { Result = false, Message = "No es posible conectar con el servicio NAUTA" };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new LogoutResult() { Result = false };
            }
            var str = await response.Content.ReadAsStringAsync();
            if (!str.ToUpper().Contains("SUCCESS"))
            {
                return new LogoutResult { Result = false, Message = str };
            }
            ATTRIBUTE_UUID = null;
            if(timer.Enabled)
                timer.Stop();
            IsActive = false;
            return new LogoutResult { Result = true };
        }

        public async Task<string> UpdateAccountCredit()
        {
            if (CSRFHW == null && !await InitSession())
                return "No fue posible obtener una sesion";
            if(ATTRIBUTE_UUID != null || await UserManager.IsConnectedAsync())
            {
                return "No se puede obtener datos del credito estando conectado";
            }
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["CSRFHW"] = CSRFHW,
                ["username"] = Username,
                ["password"] = Password,
                ["wlanuserip"] = wlanuserip,
            });

            try
            {
                var response = await ClientSession.PostAsync($"{BaseUrl}/EtecsaQueryServlet", content);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(await response.Content.ReadAsStringAsync());
                var node = doc.GetElementbyId("sessioninfo")?.SelectSingleNode("//tbody/tr[2]/td[2]");
                if (node == null)
                    return "No fue posible obtener la informacion";
                Status.AvailableCredit = node.InnerText.Trim();
                return string.Empty;
            }
            catch
            {
                return "No fue posible obtener la informacion";
            }
        }

        public async Task<string> UpdateAccountTime()
        {
            if (ATTRIBUTE_UUID == null)
            {
                return "Debe haber iniciado la sesion para saber el tiempo disponible";
            }
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                ["op"] = "getLeftTime",
                ["CSRFHW"] = CSRFHW,
                ["username"] = Username,
                ["wlanuserip"] = wlanuserip,
                ["ATTRIBUTE_UUID"] = ATTRIBUTE_UUID
            });
            try
            {
                var response = await ClientSession.PostAsync($"{BaseUrl}/EtecsaQueryServlet", content);
                string timeStr = await response.Content.ReadAsStringAsync();

                string[] elements = timeStr.Split(':');
                TimeSpan ts = new TimeSpan(int.Parse(elements[0]),
                                           int.Parse(elements[1]),
                                           int.Parse(elements[2]));
                Status.RTime = ts;
                return string.Empty;
            }
            catch
            {
                return "No fue posible obtener la informacion";
            }
        }
    }
}
