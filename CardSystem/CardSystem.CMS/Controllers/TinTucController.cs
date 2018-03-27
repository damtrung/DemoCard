using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Utitlity;
using System.Net;

namespace CardSystem.CMS.Controllers
{
    public class TinTucController : BaseController
    {
        public TinTucController() : base()
        {

        }

        // GET: TinTuc
        public ActionResult Index()
        {
            var lstArtices = _unitOfWork.GetRepositoryInstance<SuArticle>().GetListByParameter(x => x.Status == Constant.ACTIVE).ToList();
            return View(lstArtices);
        }
        public ActionResult Detail(int id)
        {
            var obj = _unitOfWork.GetRepositoryInstance<SuArticle>().GetFirstOrDefault(id);
            if (obj == null)
            {
                return HttpNotFound();
            }
            ViewBag.ListArticleHot = _unitOfWork.GetRepositoryInstance<SuArticle>().GetListByParameter(x => x.HotNew == true && x.Status == Constant.ACTIVE).Take(6).ToList();
            return View(obj);
        }
    }
}