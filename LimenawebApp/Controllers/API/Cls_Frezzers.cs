using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Frezzers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Frezzers
    {
        private Cls_API clsapi = new Cls_API();

        public GetFrezzers_api GetFrezzers()
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Frezzers", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 30);


            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetFrezzers_api>(result.Content);

            return jsonResponse;
        }

        public GetFrezzer_api GetFrezze(string DocEntry)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Frezzers/" + DocEntry, Method.GET); //mandar iddocentry por url


            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetFrezzer_api>(result.Content);

            return jsonResponse;
        }
    }
}