using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.SalesOrders;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_SalesOrders
    {
        private Cls_API_secondary clsapi = new Cls_API_secondary();
        public IRestResponse Reconciliation(Post_priceChange priceChange)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            //client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;

            var request = new RestRequest("/api/salesorders/changeprice", Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(priceChange);

            result = client.Execute(request);




            return result;
        }


        public IRestResponse AddtoToRoute(SendSOAPI salesorder)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            //client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;

            var request = new RestRequest("/api/salesorders/inroute", Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(salesorder);

            result = client.Execute(request);




            return result;
        }

        public IRestResponse DeletefromRoute(int docentry)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
           // client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/salesorders/outroute/" + docentry, Method.DELETE);

            var result = client.Execute(request);

            return result;
        }
    }
}