using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Transactions
{
    public class Mdl_Transactions
    {
    }
    public class GetTransactions_api
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Transactions_api> data { get; set; }
    }

    public class Transactions_api
    {
        public string idTransaction { get; set; }
        public string transactionType { get; set; }
        public int docEntry { get; set; }
        public DateTime docDate { get; set; }
        public int docTime { get; set; }
        public string cardCode { get; set; }
        public string paymentType { get; set; }
        public decimal total { get; set; }
        public int docEntryInv { get; set; }
        public int docNumInv { get; set; }
        public string userWeb { get; set; }
        public int idRoute { get; set; }
        public string remark { get; set; }
        public bool applied { get; set; }
        public string customer { get; set; }
        public string mainEmail { get; set; }
        public string mainTel { get; set; }
    }
}