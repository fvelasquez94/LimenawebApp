using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Authorizations
{
    public class GetAuthorizations_api
    {
        public int? code { get; set; }
        public string message { get; set; }
        public List<Authorizations_api> data { get; set; }
    }
    public class GetAuthorizationsSingle_api
    {
        public int? code { get; set; }
        public string message { get; set; }
        public Authorizations_api data { get; set; }
    }

    public class Authorizations_api
    {
        public string idAuthorization { get; set; }
        public string name { get; set; }
        public int? idType { get; set; }
        public int? idReason { get; set; }
        public string reason { get; set; }
        public string idDriver { get; set; }
        public string comments { get; set; }
        public string idFinanceUser { get; set; }
        public string commentsFinance { get; set; }
        public int? status { get; set; }
        public string docNum { get; set; }
        public string idRoute { get; set; }
        public string cardCode { get; set; }
        public string cardName { get; set; }
        public System.DateTime? date { get; set; }

    }


    public class PostAuthorization_api
    {
        public int idType { get; set; }
        public int idReason { get; set; }
        public string reason { get; set; }
        public string idDriver { get; set; }
        public string comments { get; set; }
        public string idFinanceUser { get; set; }
        public string commentsFinance { get; set; }
        public int status { get; set; }
        public string docNum { get; set; }
        public string idRoute { get; set; }
        public string cardCode { get; set; }


    }
    public class PutAuthorization_api
    {
        public int? idType { get; set; }
        public int? idReason { get; set; }
        public string reason { get; set; }
        public string idDriver { get; set; }
        public string comments { get; set; }
        public string idFinanceUser { get; set; }
        public string commentsFinance { get; set; }
        public int status { get; set; }
        public string docNum { get; set; }
        public string idRoute { get; set; }
        public string idCustomer { get; set; }
    }
    public class Mdl_Authorizations
    {
    }
}