using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Payments
{
    public class Mdl_PaymentsPOSTPUT
    {
        public class PutPayment_api
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Payment_put> data { get; set; }
        }

        public class Payment_putOriginal
        {
            public string remarks { get; set; }
            public string journalRemarks { get; set; }
            public int hasIssue { get; set; }
            public int webStatus { get; set; }


        }
        public class Payment_post
        {
            public int docEntry { get; set; }
            public int series { get; set; }
            public int invoiceType { get; set; }
            public DateTime docDate { get; set; }
            public DateTime docDueDate { get; set; }
            public DateTime vatDate { get; set; }
            public DateTime taxDate { get; set; }
            public string cardCode { get; set; }
            public int paymentType { get; set; }
            public decimal sumApplied { get; set; }
            public string userWeb { get; set; }
            public string remarks { get; set; }
            public string journalRemarks { get; set; }
            public PaymentPost_cash cash { get; set; }
            public PaymentPost_check_moneyorder check { get; set; }
            public PaymentPost_check_moneyorder moneyOrder { get; set; }
            public PaymentPost_creditCard creditCard { get; set; }
            public PaymentPost_ach_wire ach { get; set; }
            public PaymentPost_ach_wire wire { get; set; }

        }

        public class Payment_put
        {
            public int? paymentType { get; set; }
            public DateTime? docDate { get; set; }
            public string remarks { get; set; }
            public string journalRemarks { get; set; }
            public int hasIssue { get; set; }
            public int webStatus { get; set; }
            public decimal? sumApplied { get; set; }
            public PaymentPost_cash cash { get; set; }
            public PaymentPost_check_moneyorder check { get; set; }
            public PaymentPost_check_moneyorder moneyOrder { get; set; }
            public PaymentPost_creditCard creditCard { get; set; }
            public PaymentPost_ach_wire ach { get; set; }
            public PaymentPost_ach_wire wire { get; set; }

        }


        public class Payment_putNew
        {
            public int? paymentType { get; set; }
            public PaymentPost_cash cash { get; set; }
            public PaymentPost_check_moneyorder check { get; set; }
            public PaymentPost_check_moneyorder moneyOrder { get; set; }
            public PaymentPost_creditCard creditCard { get; set; }
            public PaymentPost_ach_wire ach { get; set; }
            public PaymentPost_ach_wire wire { get; set; }

        }

        public class PaymentPost_cash
        {

            public string cashAccount { get; set; }
            public string denominations { get; set; }
            /*
             [{
      "id": n,
      "Denomination: n,
      "Value": n
    }]
             */
        }

        public class PaymentPost_check_moneyorder
        {
            public string bankCode { get; set; }
            public Boolean trnsfrable { get; set; }
            public string checkAccount { get; set; }
            public Boolean endorse { get; set; }
            public string fullName { get; set; }
            public int checkNumber { get; set; }
        }

        public class PaymentPost_creditCard
        {
            //public int creditCard { get; set; }
            //public string creditCardNumber { get; set; }
            //public DateTime cardValidUntil { get; set; }
            public string voucherNum { get; set; }
            public string transaction { get; set; }

        }
        public class PaymentPost_ach_wire
        {
            public string fullName { get; set; }
            public string transferAccount { get; set; }
            public string idTransaction { get; set; }


        }
    }
}