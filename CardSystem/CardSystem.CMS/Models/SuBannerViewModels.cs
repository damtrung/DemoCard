using CardSystem.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Models
{
    public class SuBannerViewModels
    {
        public SuBanner bannerInfo { get; set; }
        public string Picture { get; set; }
        public int Status { get; set; }
        public List<SelectListItem> StatusList { get; set; }
        public int position { get; set; }
        public List<SelectListItem> PositionList { get; set; }
    }
}