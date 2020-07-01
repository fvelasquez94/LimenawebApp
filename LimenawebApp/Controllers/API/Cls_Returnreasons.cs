using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Returnreasons_api;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Returnreasons
    {
        private Cls_API clsapi = new Cls_API();
        public GetReturnreason_api GetReturnreasons()
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/ReturnReasons", Method.GET);

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetReturnreason_api>(result.Content);

            return jsonResponse;
        }
    }
}