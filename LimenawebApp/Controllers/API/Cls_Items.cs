using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Items;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Items
    {
        private Cls_API clsapi = new Cls_API();

        public GetUOM_api GetUom_Item(string itemcode)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Items/Uom/" + itemcode, Method.GET);
            request.AddHeader("cache-control", "no-cache");

            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetUOM_api>(result.Content);

            return jsonResponse;
        }
    }
}