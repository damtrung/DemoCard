using CardSystem.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Models
{
    public class SuCategoriesViewModels
    {
        public SuCategory categoryInfo { get; set; }
        public bool isMenu { get; set; }
        //public List<SuCategory> childSuCategory { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public int status { get; set; }
        public List<SelectListItem> ListParentCategory { get; set; }
        public int parentCategoryId { get; set; }
        public List<SuCategory> ListParentCategoryInfo { get; set; }
        //public string CategoryName { get; set; }
        [AllowHtml]
        public string Description { get; set; }
    }
}