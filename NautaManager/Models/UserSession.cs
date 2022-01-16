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
        public string Username { get; set; }
        bool _active = false;
        public bool IsActive
        {
            get => _active;
            set
            {
                _active = value;
                RaisePropertyChanged();
            }
        }
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();
        HttpClientHandler handler;
        HttpClient ClientSession { get; set; }


        byte[] _password;
        AccountStatus _status;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string name = null)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
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
        long elapsedSeconds;
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        public string ConsumedTime
        {
            get => string.Format("{0:D2}:{1:D2}:{2:D2}", elapsedSeconds / 60 / 60, elapsedSeconds / 60 % 60, elapsedSeconds % 60);
        }
        public string CSRFHW { get; private set; }
        public string wlanuserip { get; private set; }
        public string ATTRIBUTE_UUID { get; private set; }

        public Dictionary<string, string> FormData { get; set; }
        string LoginURL { get; set; }

        public UserSession()
        {
            Status = new AccountStatus();
            timer.Elapsed += TickSecond;
        }

        private void TickSecond(object sender, ElapsedEventArgs e)
        {
            Interlocked.Increment(ref elapsedSeconds);
            RaisePropertyChanged("ConsumedTime");
        }

        public bool InitSession()
        {
            if (UserManager.IsConnected())
                return false;
            Dictionary<string, string> formData = new Dictionary<string, string>();
            HttpResponseMessage response;
            try
            {
                response = ClientSession.GetAsync(BaseUrl).Result;
            }
            catch
            {
                return false;
            }
            HtmlDocument getRequest = new HtmlDocument();
            getRequest.LoadHtml(response.Content.ReadAsStringAsync().Result);
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

        public static implicit operator UserSession(UserSessionStore store)
        {
            var model = new UserSession()
            {
                Username = store.Username,
                Status = store.Status ?? new AccountStatus(),
                CSRFHW = store.CSRFHW,
                ATTRIBUTE_UUID = store.ATTRIBUTE_UUID,
                wlanuserip = store.wlanuserip,
                IsActive = store.ATTRIBUTE_UUID != null
            };
            model.InitClientSession();
            if(model.ATTRIBUTE_UUID == null)
                model.InitSession();
            model.LoadPassword(store.Password);
            return model;
        }

        public void InitClientSession()
        {
            CookieContainer = UserSessionStore.LoadHTTPClient(Username) ?? CookieContainer;
            handler = new HttpClientHandler() { CookieContainer = CookieContainer, UseCookies = true };
            ClientSession = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        }

        internal void LoadPassword(string serializedPassword)
        {
            _password = Convert.FromBase64String(serializedPassword);
        }

        public async Task<LoginResult> TryLogin()
        {
            if(FormData == null && !InitSession())
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
            if (CSRFHW == null && !InitSession())
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
