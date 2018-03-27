using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Utitlity;
using Microsoft.AspNet.Identity;

namespace CardSystem.CMS.Controllers
{
    public class SanPhamController : BaseController
    {
       public SanPhamController() : base()
        {

        }

        // GET: SanPham
        public ActionResult Index()
        {
            var lstProduct = _unitOfWork.GetRepositoryInstance<SuProduct>().GetListByParameter(x => x.Status == Constant.ACTIVE).ToList();
            return View(lstProduct);
        }
        public ActionResult Detail(int id)
        {
            var userID = User.Identity.GetUserId();
            var obj = _unitOfWork.GetRepositoryInstance<SuProduct>().GetFirstOrDefault(id);
            ViewBag.ListProduct = _unitOfWork.GetRepositoryInstance<SuProduct>().GetListByParameter(x => x.IsSale == true && x.Status == Constant.ACTIVE).Take(6).ToList();
            if (userID != null)
            {
                var objCart = _unitOfWork.GetRepositoryInstance<SuCart>().GetFirstOrDefaultByParameter(x => x.ProductId == id && x.CartStatusId.Equals(Constant.CART_ADDED) && x.UserId.Equals(userID));
                ViewBag.isExistCart = objCart != null ? true : false;
            }
            else
            {
                ViewBag.isExistCart = false;
            }
            return View(obj);
        }
        public ActionResult RelatedProducts(SuProduct pr)
        {
            return PartialView("RelatedProducts");
        }
    }
}