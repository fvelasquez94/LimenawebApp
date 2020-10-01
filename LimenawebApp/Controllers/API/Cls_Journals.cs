using LimenawebApp.Controllers.Session;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static LimenawebApp.Models.Journal.Mdl_Journal;

namespace LimenawebApp.Controllers.API
{
    public class Cls_Journals
    {
        private Cls_API clsapi = new Cls_API();
        public IRestResponse PostJournal(Journal_api newJournal)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/JournalEntries", Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(newJournal);

            var result = client.Execute(request);

            return result;
        }

        public IRestResponse DeleteJournal(int docentry)
        {
            var settings = clsapi.GetAPI();

            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/JournalEntries/" + docentry, Method.DELETE);

            var result = client.Execute(request);

            return result;
        }


        public GET_Journals_api Getjournals( DateTime? fstart, DateTime? fend, bool ShowCanceled = true)
        {

            var settings = clsapi.GetAPI();
            string bearerToken = settings.token;
            var client = new RestClient(settings.IP);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(bearerToken, "Bearer");
            var request = new RestRequest("/api/JournalEntries", Method.GET);
            //request.AddParameter("pageNumber", 1);
           request.AddParameter("pageSize", 250);


            if (fstart != null)
            {

                request.AddParameter("StartDate", fstart.Value.ToShortDateString());
                request.AddParameter("EndDate", fend.Value.ToShortDateString());
            }

                request.AddParameter("ShowCanceled", ShowCanceled);
            
            request.AddHeader("cache-control", "no-cache");


            var result = client.Execute(request);
            var jsonResponse = JsonConvert.DeserializeObject<GET_Journals_api>(result.Content);

            return jsonResponse;
        }
    }
}