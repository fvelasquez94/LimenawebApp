using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.Session
{
    public class Cls_API_secondary
    {
        private string TOKEN = string.Format("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImYudmVsYXNxdWV6QGxpbWVuYWluYy5uZXQiLCJDb21wYW55IjoiRGlzdGlidWlkb3JhTGltZW5hSW5jIiwianRpIjoiZjdiYjBjOGUtODNiYi00NDhhLThjMmMtODEwNWU2ODc4M2I5IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoibWFuYWdlciIsImV4cCI6MTYyODk2MDc3Nn0.lg6748xLpeEV-PyHipy4jkUU9CyQnOXv1cHlpXGlYVU");
        private string IP = ConfigurationManager.AppSettings["API_sec"]; //externa

        public class APISettings
        {
            public string token { get; set; }
            public string IP { get; set; }
        }

        public class loguser
        {
            public string userName { get; set; }
            public string email { get; set; }
            public string password { get; set; }
        }

        public class newToken
        {
            public string token { get; set; }
            public DateTime expiration { get; set; }

        }

        public APISettings GetAPI()
        {
            APISettings settings = new APISettings();
            settings.IP = this.IP;
            settings.token = "";

            return settings;

            //loguser user = new loguser();
            //user.userName = "Desarrollo";
            //user.email = "f.velasquez@limenainc.net";
            //user.password = "Fr@nDLI*8";

            //Evaluar token
            //Consultar si token no ha  expirado
            //var tokenCookies = HttpContext.Current.Request.Cookies["token"];

            //if (tokenCookies != null)
            //{
            //    settings.token = tokenCookies.Value;
            //    return settings;
            //}
            //else
            //{
            //    var client = new RestClient(settings.IP);
            //    var request = new RestRequest("/api/Users/login", Method.POST);

            //    request.RequestFormat = DataFormat.Json;
            //    request.AddJsonBody(user);

            //    var result = client.Execute(request);
            //    var jsonResponse = JsonConvert.DeserializeObject<newToken>(result.Content);

            //    settings.token = jsonResponse.token;


            //    var c = new System.Web.HttpCookie("token");
            //    c.Value = jsonResponse.token;
            //    c.Expires = DateTime.Now.AddMinutes(4);
            //    HttpContext.Current.Response.Cookies.Add(c);


            //    return settings;

            //}


        }
    }
}