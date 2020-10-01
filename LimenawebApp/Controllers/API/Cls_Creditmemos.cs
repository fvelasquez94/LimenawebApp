using LimenawebApp.Models.Creditmemos_api;
using LimenawebApp.Controllers.Session;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Creditmemos
    {
        private Cls_API clsapi = new Cls_API();
        public IRestResponse PostCreditmemo(PostCreditmemos_api newCreditmemo)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos/Draft", Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(newCreditmemo);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse PutCreditmemo(PutDetailsCreditmemos_api item, int docentry, int delCreditMemo = 0)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;
            if (delCreditMemo == 0)
            {
                var request = new RestRequest("/api/CreditMemos/Draft/" + docentry, Method.PUT);

                List<PutDetailsCreditmemos_api> lsttopost = new List<PutDetailsCreditmemos_api>();
                lsttopost.Add(item);

                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(lsttopost);

                result = client.Execute(request);
            }
            else
            {
                var resultDel = DeleteCreditmemoComplete(docentry);

                result = resultDel;
            }



            return result;
        }

        public IRestResponse CancelCreditmemoDetail(PutDetailsCreditmemos_apiNOSHOW item, int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;
 
                var request = new RestRequest("/api/CreditMemos/Draft/" + docentry, Method.PUT);

            List<PutDetailsCreditmemos_apiNOSHOW> lsttopost = new List<PutDetailsCreditmemos_apiNOSHOW>();
            lsttopost.Add(item);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(lsttopost);



            //request.RequestFormat = DataFormat.Json;
            //    request.AddJsonBody(item);

                result = client.Execute(request);




            return result;
        }


        public IRestResponse DeleteCreditmemoComplete(int DocentryCreditMemo)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos/Draft/" + DocentryCreditMemo, Method.DELETE);

            //request.AddParameter("docEntry ", DocentryCreditMemo);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse TransformCreditMemo(int DocentryCreditMemo, int userauthorizedby)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos/" + DocentryCreditMemo + "?authorizedBy=" + userauthorizedby, Method.POST);
            //request.AddParameter("authorizedBy", userauthorizedby);
            request.AddHeader("cache-control", "no-cache");
            var result = client.Execute(request);

            return result;
        }

        public GetCreditmemos_api GetCreditMemos(int DocentryInv, string CardCode, DateTime? fstart, DateTime? fend, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos/Draft", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);

            if (DocentryInv != 0)
            {
                request.AddParameter("DocEntryInv", DocentryInv);
            }
            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }
            if (fstart != null) {
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }



            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetCreditmemos_api>(result.Content);

            return jsonResponse;
        }

        public GetCreditmemos_api GetCreditMemo(int DocEntry, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos/Draft", Method.GET);
            request.AddParameter("DocEntry", DocEntry);

            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetCreditmemos_api>(result.Content);

            return jsonResponse;
        }



        public GetCreditmemos_api GetCreditMemosOriginals(int DocentryInv, string CardCode, DateTime? fstart, DateTime? fend, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);

            if (DocentryInv != 0)
            {
                request.AddParameter("DocEntryInv", DocentryInv);
            }
            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }
            if (fstart != null)
            {
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }



            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetCreditmemos_api>(result.Content);

            return jsonResponse;
        }

        public GetCreditmemos_api GetCreditMemoOriginal(int DocEntry, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/CreditMemos", Method.GET);
            request.AddParameter("DocEntry", DocEntry);

            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetCreditmemos_api>(result.Content);

            return jsonResponse;
        }
    }
}