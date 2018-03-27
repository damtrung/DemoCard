using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardSystem.CMS.Models
{
    public class SuTransactionViewModels
    {
        public long id { get; set; }
        public string created_at { get; set; }
        public string created_by { get; set; }
        public int status { get; set; }
        public float amout { get; set; }
        public string transactionid { get; set; }
        public int transactiontype { get; set; }
        public int serviceid { get; set; }
        public string receiptnumber { get; set; }
        public string code { get; set; }
        public string serial { get; set; }
        public decimal price { get; set; }
        public decimal proccessed { get; set; }
        public string sign { get; set; }
    }
}