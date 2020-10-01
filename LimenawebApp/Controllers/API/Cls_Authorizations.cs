using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Authorizations;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Authorizations
    {
        private Cls_API clsapi = new Cls_API();
        public IRestResponse PostAuthorization(PostAuthorization_api newAuthorization)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Authorizations", Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(newAuthorization);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse PutAuthorization(PutAuthorization_api item, string idAuthorization)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;

            var request = new RestRequest("/api/Authorizations/" + idAuthorization, Method.PUT);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(item);
            result = client.Execute(request);

            return result;
        }

        public IRestResponse DeleteAuthorization(int idAuthorization)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Authorizations/" + idAuthorization, Method.DELETE);

            var result = client.Execute(request);

            return result;
        }


        public GetAuthorizations_api GetAuthorizations(int DocentryInv, string CardCode, string Iddriver, DateTime? fstart, DateTime? fend)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Authorizations", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);

            if (DocentryInv != 0)
            {
                request.AddParameter("DocNum", DocentryInv);
            }
            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }
            if (Iddriver != "")
            {
                request.AddParameter("IdDriver", Iddriver);
            }
            if (fstart != null)
            {
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetAuthorizations_api>(result.Content);

            return jsonResponse;
        }

        public GetAuthorizations_api GetAuthorization(int DocEntry)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Authorizations/" + DocEntry, Method.GET); //mandar iddocentry por url
            request.AddParameter("DocEntry", DocEntry);

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetAuthorizations_api>(result.Content);

            return jsonResponse;
        }
        public GetAuthorizationsSingle_api GetAuthorizationbyID(string idauthorization)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Authorizations/" + idauthorization, Method.GET); //mandar iddocentry por url
    

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetAuthorizationsSingle_api>(result.Content);

            return jsonResponse;
        }
    }
}