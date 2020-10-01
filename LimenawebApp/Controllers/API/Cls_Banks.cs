using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Payments;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Banks
    {
        private Cls_API clsapi = new Cls_API();

        public GetBanks_api GetBanks()
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Banks", Method.GET);
            //request.AddParameter("pageNumber", 1);
            //request.AddParameter("pageSize", 50);

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetBanks_api>(result.Content);

            return jsonResponse;
        }
    }
}