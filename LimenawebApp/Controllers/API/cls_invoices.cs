using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Invoices;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class cls_invoices
    {
        private Cls_API clsapi = new Cls_API();
        public GetInvoices_api GetInvoices(string iddriver, string cardcode,string Id_Route,int InvoiceStatus,  bool onlyAR, DateTime? fstart, DateTime? fend, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Invoices", Method.GET);
            //request.AddParameter("pageNumber", 1);
            request.AddParameter("pageSize", 50);


            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            if (iddriver != "")
            {
                request.AddParameter("Id_Driver", iddriver);
            }
            if (Id_Route != "")
            {
                request.AddParameter("Id_Route", Id_Route);
            }
            if (InvoiceStatus != 0)
            {
                request.AddParameter("InvoiceStatus", InvoiceStatus);
            }
            if (cardcode != "")
            {
                request.AddParameter("CardCode", cardcode);
            }
            if (fstart != null)
            {
                request.AddParameter("StartDeliveryDate", Convert.ToDateTime(fstart).ToShortDateString());
                request.AddParameter("EndDeliveryDate", Convert.ToDateTime(fend).ToShortDateString());
            }

            //verificamos
            request.AddParameter("IsAR", onlyAR);


            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetInvoices_api>(result.Content);

            return jsonResponse;
        }
        public IRestResponse PutInvoice(int docentry, PUT_Invoices_api model)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Invoices/" + docentry, Method.PUT);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(model);

            var result = client.Execute(request);

            return result;
        }
        public GetInvoices_api GetInvoice(int DocEntry,int docnum, Boolean includeDetails = false)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Invoices", Method.GET);

            if (DocEntry != 0) { request.AddParameter("DocEntry", DocEntry); }
            if (docnum != 0) { request.AddParameter("DocNum", docnum); }
        

            if (includeDetails == true)
            {
                request.AddParameter("FullMode", includeDetails);
            }
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetInvoices_api>(result.Content);

            return jsonResponse;
        }

    }
}