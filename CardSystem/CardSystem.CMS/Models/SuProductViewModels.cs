using CardSystem.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardSystem.CMS.Models
{
    public class SuProductViewModels
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImagePath1 { get; set; }
        public string ImagePath2 { get; set; }
        public string ImagePath3 { get; set; }
        public string ImagePath4 { get; set; }
        public string ImagePath5 { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public string CategeoryName { get; set; }
        public int Status { get; set; }
        public Nullable<double> NewPrice { get; set; }
    }
    public class CartViewModel
    {
        public SuCategory category;
        public SuCart cart;
    }
}