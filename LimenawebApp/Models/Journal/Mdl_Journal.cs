using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LimenawebApp.Models.Journal
{
    public class Mdl_Journal
    {
        public class POST_Journals_api
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Journal_api> data { get; set; }
        }
        public class Journal_api
        {
            public DateTime? docDate { get; set; }
            public string comments { get; set; }
            public decimal journalTotal { get; set; }
            public string indicator { get; set; }
            public int docEntryInv { get; set; }
            public Boolean reconcileBp { get; set; }
            public string cardCode { get; set; }
            public List<Journals_details> details { get; set; }
        }
        public class Journals_details
        {
            public string accountOrCardCode { get; set; }
            public decimal debit { get; set; }
            public decimal credit { get; set; }
        }



        public class GET_Journals_api
        {
            public int code { get; set; }
            public string message { get; set; }
            public List<Journal_get> data { get; set; }
        }
        public class Journal_get
        {
            public int transId { get; set; }
            public string transType { get; set; }
            public string baseRef { get; set; }
            public DateTime refDate { get; set; }
            public string memo { get; set; }
            public string ref1 { get; set; }
            public string ref2 { get; set; }
            public int? createdBy { get; set; }
            public decimal sysTotal { get; set; }
            public DateTime dueDate { get; set; }
            public DateTime taxDate { get; set; }
            public DateTime createDate { get; set; }
            public string objType { get; set; }
            public string indicator { get; set; }
            public int? series { get; set; }
            public int? createTime { get; set; }
            public int? docEntryInv { get; set; }
            public int? journalCanceled { get; set; }
            public string status { get; set; }
            public List<Journalsget_details> details { get; set; }
        }
        public class Journalsget_details
        {
            public int? transId { get; set; }
            public int? lineNum { get; set; }
            public decimal debit { get; set; }
            public decimal credit { get; set; }
            public DateTime dueDate { get; set; }
            public string shortName { get; set; }
            public string contraAct { get; set; }
            public string lineMemo { get; set; }
            public string transType { get; set; }
            public DateTime refDate { get; set; }
            public string ref1 { get; set; }
            public string ref2 { get; set; }
            public string indicator { get; set; }
            public string objType { get; set; }
        }

    }
}