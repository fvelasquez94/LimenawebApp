using LimenawebApp.Controllers.Session;
using LimenawebApp.Models.Transactions;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Controllers.API
{
    public class Cls_transactions
    {

        private Cls_API clsapi = new Cls_API();
        public GetTransactions_api GetTransactions(string DocEntryInv, string UserWeb, string IdRoute, string DocNumInv, string CardCode, string TransactionType, DateTime? fstart, DateTime? fend, int pagesize, int pagenumber)
        {

            var settings = clsapi.GetAPI();
            //Si se necesitan DETALLES de las invoices,hay que activar FullMode=true

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/Transactions", Method.GET);
            if (pagenumber != 0)
            {
                request.AddParameter("pageNumber", pagenumber);
            }
            if (pagesize != 0)
            {
                request.AddParameter("pageSize", pagesize);
            }



            if (DocEntryInv != "") { request.AddParameter("DocEntryInv", DocEntryInv); }
            if (UserWeb != "") { request.AddParameter("UserWeb", UserWeb); }
            if (IdRoute != "") { request.AddParameter("IdRoute", IdRoute); }
            if (DocNumInv != "") { request.AddParameter("DocNumInv", DocNumInv); }
            if (CardCode != "") { request.AddParameter("CardCode", CardCode); }
            if (TransactionType != "") { request.AddParameter("TransactionType", TransactionType); }
            if (fstart != null)
            { //siempre va conjunto a fend
                request.AddParameter("StartDate", Convert.ToDateTime(fstart).ToShortDateString());
                request.AddParameter("EndDate", Convert.ToDateTime(fend).ToShortDateString());
            }

            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GetTransactions_api>(result.Content);

            return jsonResponse;
        }

        //public Response_global AddTransaction(Sd_transactions newtransaction)
        //{
        //    Response_global response = new Response_global();
        //    //Definimos 2 tipos de Transacciones : 1- Autorizaciones | 2- Transaccion

        //    try
        //    {
        //        db.Sd_transactions.Add(newtransaction);
        //        db.SaveChanges();
        //        response.flag = true;
        //        response.message = "Success";
        //    }
        //    catch (Exception ex){
        //        response.flag = false;
        //        response.message = ex.Message;
        //    }

        //    return response;
        //}

    }
}