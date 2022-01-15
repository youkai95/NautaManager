using System;
using System.Threading.Tasks;
using RestSharp;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.Text;

namespace NautaManager
{
    class LoginResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public AccountStatus Status { get; set; }
    }

    class AccountStatus
    {
        public TimeSpan RemainingTime { get; set; }
        public int AvailableCredit { get; set; }
    }

    class LogoutResult
    {

    }

    class UserSession
    {
        public string Username { get; set; }
        byte[] _password;
        public string Password
        {
            get
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(_password, null, DataProtectionScope.CurrentUser));
            }
            set
            {
                _password = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.CurrentUser);
            }
        }
        public UserSession()
        {
        }

        public async Task<LoginResult> TryLogin(HttpClient client, string url, Dictionary<string, string> body)
        {
            body["username"] = Username;
            body["password"] = Password;
            HttpContent request = new FormUrlEncodedContent(body);
            var response = await client.PostAsync(url, request);
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
            return new LoginResult() { Result = true };
        }

        public async Task<LogoutResult> TryLogout()
        {
            return new LogoutResult();
        }

        public async Task<AccountStatus> TryGetAccountStatus()
        {
            return new AccountStatus();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //RestClient client = new RestClient("https://secure.etecsa.net:8443/");
            HttpClient client = new HttpClient();
            Dictionary<string, string> formData = new Dictionary<string, string>();
            HtmlWeb parser = new HtmlWeb();
            var html = parser.Load("https://secure.etecsa.net:8443/");
            var form = html.GetElementbyId("formulario");
            var info = form.ChildNodes.Where(n => n.Name == "input").Select(n => new { Name = n.GetAttributeValue("name", null), Value = n.GetAttributeValue("value", null) });
            foreach(var input in info)
            {
                formData.Add(input.Name, ParseValue(input.Name, input.Value));
            }
            var r = DoLogin(client, form.GetAttributeValue("action", null), formData).Result;
        }
    }
}
