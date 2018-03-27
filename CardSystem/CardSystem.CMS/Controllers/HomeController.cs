using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Services;
using CardSystem.Utitlity;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using CardSystem.Helpers;
using Microsoft.AspNet.Identity.Owin;

namespace CardSystem.CMS.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController() : base()
        {

        }
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                //string strFromDate = "30/06/2017";
                //string strToDate = "01/07/2017";
                //var pData = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetAllRecords().ToList();
                //var pSearch = pData.Where(x => x.Created_At >= strFromDate.ToDateTime() && (x.Created_At).ToDateTime() <= strToDate.ToDateTime()).ToList();
                //var a = TripleDes.Decrypt(EcoPayKey.Scret_Key, "jujx3xnpV65KWLw8g9trIQ==");
                //var b = TripleDes.Decrypt(EcoPayKey.Scret_Key, "xo6BcxDeIUSRuzKcN/NP8A==");
                var lstBanner = _unitOfWork.GetRepositoryInstance<SuBanner>().GetListByParameter(x => x.Status == Constant.ACTIVE).Take(5).ToList();
                ViewBag.ListBanner = lstBanner;

                var lstProduct = _unitOfWork.GetRepositoryInstance<SuProduct>().GetListByParameter(x => x.Status == Constant.ACTIVE).OrderByDescending(x => x.Id).Take(4).ToList();
                ViewBag.ListProduct = lstProduct;

                var lstArticle = _unitOfWork.GetRepositoryInstance<SuArticle>().GetListByParameter(x => x.Status == Constant.ACTIVE).OrderByDescending(x => x.Id).Take(3).ToList();
                ViewBag.ListArticle = lstArticle;

                var lstAgency = _unitOfWork.GetRepositoryInstance<SuUsersInfoes>().GetListByParameter(x => x.Status == Constant.ACTIVE).OrderByDescending(x => x.Id).Take(6).ToList();
                ViewBag.ListAgency = lstAgency;
            }
            catch (Exception ex)
            {

                //throw;
            }


            return View();
        }
        public ActionResult SearchProduct(FormCollection cl)
        {
            Session["TXTSEARCH"] = cl["txtSearch"];
            return RedirectToAction("Index", "Search");
        }
        [HttpGet]
        public JsonResult loadcountCart()
        {
            var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.GetUserId() };
            var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
            var lstCartDetails = _unitOfWork.GetRepositoryInstance<UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
            return Json(lstCartDetails.Count, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult countNoti()
        {
            SuInstallmentPayment suInstallmentPayment;
            var listSuTransaction = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetListByParameter(x => x.Created_By.Equals(User.Identity.Name.ToString()) && (x.TransactionType == 4 || x.TransactionType == 5) && x.Status == Constant.ACTIVE).ToList();
            foreach (var item in listSuTransaction)
            {
                suInstallmentPayment = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefaultByParameter(x => x.Id == item.TransactionId && x.IsView == true);
                if (suInstallmentPayment != null && suInstallmentPayment.IsView == true)
                {
                    suInstallmentPayment.IsView = false;
                    _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().Update(suInstallmentPayment);
                    _unitOfWork.SaveChanges();
                }
            }

            return Json("OK", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Building()
        {
            return View();
        }
        public ActionResult Notify()
        {
            var lstTransaction = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetListByParameter(x => x.Created_By.Equals(User.Identity.Name.ToString()) && (x.TransactionType == 4 || x.TransactionType == 5) && x.Status == Constant.ACTIVE).ToList();
            var mentPayment = new List<NotifyPayment>();
            foreach (var item in lstTransaction)
            {
                SuInstallmentPayment payment = new SuInstallmentPayment();
                var notif = new NotifyPayment();
                payment = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefaultByParameter(x => x.Id == item.TransactionId);
                if(payment == null)
                {
                    return View();
                }
                else
                {
                    notif.TransactionType = payment.TransactionType.Value;
                    notif.NumberContract = payment.NumberContract;
                    notif.Updated_At = item.Updated_At;
                    mentPayment.Add(notif);
                }
                
            }
            return View(mentPayment);
        }
        public ActionResult SystemError()
        {
            return View();
        }
    }
    public class NotifyPayment
    {
        public int TransactionType { get; set; }
        public string Updated_At { get; set; }
        public string NumberContract { get; set; }
    }
}