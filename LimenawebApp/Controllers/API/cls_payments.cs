using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Payments;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static LimenawebApp.Models.Payments.Mdl_PaymentsPOSTPUT;

namespace LimenawebApp.Controllers.API
{
    public class cls_payments
    {
        private Cls_API clsapi = new Cls_API();
        public GetPayments_api GetPayments(string CardCode, int DocEntryInv, DateTime? fstart, DateTime? fend, bool date = true, bool canceled = false)
        {

            var settings = clsapi.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/Draft", Method.GET);
            //request.AddParameter("pageNumber", 1);
            //request.AddParameter("pageSize", 50);

            if (DocEntryInv != 0)
            {//Solicitamos solo para un documento
                request.AddParameter("DocEntryInv", DocEntryInv);
            }

            if (date == true)
            {
                request.AddParameter("CardCode", CardCode);
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }

            if (fstart !=null)
            {

                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }


            if (CardCode != "")
            {
                request.AddParameter("CardCode", CardCode);
            }

            request.AddParameter("Canceled", canceled);

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetPayments_api>(result.Content);

            return jsonResponse;
        }
        public GetPayments_apiv2 GetPaymentsv2()
        {

            var settings = clsapi.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/Draft/v2", Method.GET);
            //request.AddParameter("pageNumber", 1);
            //request.AddParameter("pageSize", 50);

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetPayments_apiv2>(result.Content);

            return jsonResponse;
        }



        public GetPayments_api GetPayment(int DocEntry)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/Draft/", Method.GET);
            request.AddParameter("DocEntry", DocEntry);

 
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetPayments_api>(result.Content);

            return jsonResponse;
        }

        public GetPayments_api GetPaymentOriginal(int DocEntry)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/", Method.GET);
            request.AddParameter("DocEntry", DocEntry);


            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetPayments_api>(result.Content);

            return jsonResponse;
        }
        public IRestResponse Postpayment(Payment_post newPayment)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/Draft", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(newPayment);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse PutPayment(Payment_put item, int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;


            var request = new RestRequest("/api/Payments/Draft/" + docentry, Method.PUT);

            request.RequestFormat = DataFormat.Json;

            request.AddJsonBody(item);

            result = client.Execute(request);




            return result;
        }


        public IRestResponse PutPaymentnew(Payment_putNew item, int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;


            var request = new RestRequest("/api/Payments/Draft/" + docentry, Method.PUT);

            request.RequestFormat = DataFormat.Json;

            request.AddJsonBody(item);

            result = client.Execute(request);




            return result;
        }

        public IRestResponse PutPaymentOriginal(Payment_putOriginal item, int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            IRestResponse result;


            var request = new RestRequest("/api/Payments/" + docentry, Method.PUT);

            request.RequestFormat = DataFormat.Json;

            request.AddJsonBody(item);

            result = client.Execute(request);




            return result;
        }

        public IRestResponse DeletePaymentOriginal(int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/" + docentry, Method.DELETE);

            var result = client.Execute(request);

            return result;
        }

        public GetPayments_api GetPaymentsOriginals(string CardCode, int DocEntryInv, DateTime? fstart, DateTime? fend, bool date = true)
        {

            var settings = clsapi.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 250);

            if (DocEntryInv != 0)
            {//Solicitamos solo para un documento
                request.AddParameter("DocEntryInv", DocEntryInv);
            }

            if (date == true)
            {
                request.AddParameter("CardCode", CardCode);
                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
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
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetPayments_api>(result.Content);

            return jsonResponse;
        }
        //public IRestResponse Postpayment(Payment_post newPayment)
        //{

        //    var settings = clsapi.GetAPI();
        //    //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
        //    string bearerToken = settings.token;
        //    var client = new RestClient(settings.IP);
        //    client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
        //    var request = new RestRequest("/api/Payments/Draft", Method.POST);
        //    request.RequestFormat = DataFormat.Json;
        //    request.AddJsonBody(newPayment);

        //    var result = client.Execute(request);

        //    return result;
        //}

        public IRestResponse Transform_payment(int docentry, int ID_userweb)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/" + docentry + "/" + ID_userweb, Method.POST);
            //request.AddParameter("authorizedBy", userauthorizedby);
            request.AddHeader("cache-control", "no-cache");
            var result = client.Execute(request);

            return result;
        }
        public IRestResponse DeletePayment(int docEntry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Payments/Draft/" + docEntry, Method.DELETE);

            var result = client.Execute(request);

            return result;
        }
    }
}