using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.Session
{
    public class Cls_API
    {
        private string TOKEN = string.Format("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImYudmVsYXNxdWV6QGxpbWVuYWluYy5uZXQiLCJDb21wYW55IjoiRGlzdGlidWlkb3JhTGltZW5hSW5jIiwianRpIjoiNmM2MDExYjUtMjkxZS00YjQwLTgzMjktZDBkMWU3MzcwNTRjIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoibWFuYWdlciIsImV4cCI6MTYyMjg0ODkyOX0.WnLMpfSNe2_NcUGAcSdGTZpoknFQMRfV9KdQ6RUWqlM");
        private string IP = "http://50.194.9.195:65214";

        public class APISettings
        {
            public string token { get; set; }
            public string IP { get; set; }
        }

        public APISettings GetAPI()
        {
            APISettings settings = new APISettings();
            settings.IP = this.IP;
            settings.token = this.TOKEN;
            return settings;
        }
    }
}