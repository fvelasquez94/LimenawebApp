using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Customers;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_BusinessPartners
    {
        private Cls_API clsapi = new Cls_API();

        public GetBusinessPartners_api GetBusinessPartner(string CardCode, int SlpCode,int idSupervisor, bool FullMode = false)
        {

            var settings = clsapi.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/BusinessPartners", Method.GET);
            request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 50);

            if (idSupervisor != 0)
            {//Solicitamos solo para un documento
                request.AddParameter("idSupervisor", idSupervisor);
            }

            if (SlpCode != 0)
            {//Solicitamos para filtrar vendedor
                request.AddParameter("SlpCode", SlpCode);
            }
            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }

            request.AddParameter("FullMode", FullMode);
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetBusinessPartners_api>(result.Content);

            return jsonResponse;
        }
    }
}