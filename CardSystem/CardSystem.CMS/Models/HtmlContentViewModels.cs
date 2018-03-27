using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Models
{
    public class HtmlContentViewModels
    {
        [AllowHtml]
        public string Description { get; set; }
        public int CateId { get; set; }
        public List<SelectListItem> ListCate { get; set; }
        public string CateName { get; set; }
        public int Id { get; set; }
    }
}