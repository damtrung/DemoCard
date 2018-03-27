using CardSystem.CMS.Models;
using CardSystem.Model.Entity;
using CardSystem.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Controllers
{
    public class HelperController : BaseController
    {
        public HelperController() : base()
        {

        }
        // GET: Helper
        public ActionResult Index(int cateId)
        {
            SuHtml htmlContent = _unitOfWork.GetRepositoryInstance<SuHtml>().GetFirstOrDefaultByParameter(x => x.PageId == cateId);
            if (htmlContent == null)
            {
                return HttpNotFound();
            }
            var viewModel = new HtmlContentViewModels();
            viewModel.Description = htmlContent.Description;
            viewModel.CateId = htmlContent.PageId.Value;
            viewModel.Id = htmlContent.Id;
            viewModel.CateName = _unitOfWork.GetRepositoryInstance<SuCategory>().GetFirstOrDefault(htmlContent.PageId.Value).Name;
            return View(viewModel);
        }
    }
}