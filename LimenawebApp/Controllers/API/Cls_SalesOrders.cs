using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.SalesOrders;
using Newtonsoft.Json;
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
        private Cls_API clsapiPrincipal = new Cls_API();
        private Cls_budget clsapiBudget = new Cls_budget();
        public IRestResponse Reconciliation(Post_priceChange priceChange)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            //client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;

            var request = new RestRequest("/api/salesorders/changeprice", Method.PUT);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(priceChange);

            result = client.Execute(request);




            return result;
        }

        public IRestResponse CancelSO(int docentry)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            // client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/salesorders/Cancel/" + docentry, Method.PUT);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse DeleteRowfromOrder(details_priceChangeDelete priceChange)
        {
            var settings = clsapi.GetAPI();

            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            //client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;

            var request = new RestRequest("/api/salesorders/rows", Method.DELETE);

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

            var request = new RestRequest("/api/salesorders/inroute", Method.PUT);

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
            var request = new RestRequest("/api/salesorders/outroute/" + docentry, Method.PUT);

            var result = client.Execute(request);

            return result;
        }


        //Sales orders

        public GetSalesOrders_api GetSalesOrders(string CardCode, int DocEntry, int DocNum, DateTime? fstart, DateTime? fend, int SlpCode, bool FullMode = false)
        {

            var settings = clsapiPrincipal.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/SalesOrders", Method.GET);
            request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);

            if (DocEntry != 0)
            {//Solicitamos solo para un documento
                request.AddParameter("DocEntry", DocEntry);
            }
            if (DocNum != 0)
            {//Solicitamos solo para un documento
                request.AddParameter("DocNum", DocNum);
            }
            if (SlpCode != 0)
            {//Solicitamos para filtrar vendedor
                request.AddParameter("SlpCode", SlpCode);
            }

            if (fstart != null)
            {
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }


            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }

            request.AddParameter("FullMode", FullMode);
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetSalesOrders_api>(result.Content);

            return jsonResponse;
        }

        //Budget

        public GetBudget_api GetBudget(string CardCode,int slpCode, DateTime? fstart, DateTime? fend, bool OnlyActives = false)
        {

            var settings = clsapiBudget.GetAPI();
            //string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            //client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Budgets", Method.GET);
            request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);



            if (fstart != null)
            {
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }


            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }
            if (slpCode != 0)
            {
                request.AddParameter("SlpCode", slpCode);
            }

            request.AddParameter("OnlyActives", OnlyActives);
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetBudget_api>(result.Content);

            return jsonResponse;
        }
    }
}