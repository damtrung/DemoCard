using CardSystem.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Models
{
    public class SuArticlesViewModels
    {
        public SuArticle articlesInfo { get; set; }
        public string ImageTitle { get; set; }
        public bool HotNew { get; set; }
        public bool Annoucement { get; set; }
        public bool Promotion { get; set; }
        public int status { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public List<SelectListItem> ListCategory { get; set; }
        public int categoryId { get; set; }
        public List<SuCategory> ListParentCategoryInfo { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        [AllowHtml]
        public string Summary { get; set; }
    }
}