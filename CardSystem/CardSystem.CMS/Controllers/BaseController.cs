using CardSystem.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Model.Entity;
using System.Data.SqlClient;
using CardSystem.Utitlity;
using System.Web.Routing;

namespace CardSystem.CMS.Controllers
{
    public class BaseController : Controller
    {
        // GET: Base
        public GenericUnitOfWork _unitOfWork;
        public ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        protected override void Initialize(RequestContext requestContext)
        {
            _unitOfWork = new GenericUnitOfWork();
            base.Initialize(requestContext);
            if (requestContext.HttpContext.User != null && requestContext.HttpContext.User.Identity.Name != "")
            {
                var userName = requestContext.HttpContext.User.Identity.Name;
                var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(userName));
                if (wallet != null)
                {
                    ViewBag.BALANCE = wallet.Balance;
                    ViewBag.COMISSION = wallet.Commission;
                }
                var user = UserManager.FindByName(userName);
                var name = user.UsersInfo.FullName;
                ViewBag.NAME = name;
                var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.GetUserId() };
                var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
                var lstCartDetails = _unitOfWork.GetRepositoryInstance<UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
                ViewBag.TOTALITEMCART = lstCartDetails != null ? lstCartDetails.Count : 0;

                List<SuInstallmentPayment> listNotify = new List<SuInstallmentPayment>();
                SuInstallmentPayment notify;
                int count = 0;
                var lstTransaction = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetListByParameter(x => x.Created_By.Equals(User.Identity.Name.ToString()) && (x.TransactionType == 4 || x.TransactionType == 5) && x.Status == Constant.ACTIVE).ToList();
                foreach (var item in lstTransaction)
                {
                    notify = new SuInstallmentPayment();
                    notify = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefaultByParameter(x => x.Id == item.TransactionId && x.IsView == true);
                    if (notify == null)
                    {
                        count += 0;
                    }
                    else
                    {
                        count += 1;
                    }
                    //listNotify.Add(notify);
                }

                //ViewBag.NOTIFYPAYMENT = lstTransaction != null ? lstTransaction.Count : 0;
                //if (listNotify == null)
                //{
                //    ViewBag.NOTIFYPAYMENT = 0;
                //}
                //else
                //{
                //    ViewBag.NOTIFYPAYMENT = listNotify.Count;
                //}
                ViewBag.NOTIFYPAYMENT = count;
                // ViewBag.NOTIFYPAYMENT = listNotify != null ? listNotify.Count : 0;
            }
            else { ViewBag.TOTALITEMCART = 0; }
            var settingValue = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_ALL"));
            if (settingValue.SValue.ToIntOrZero() > 0)
            {
                ViewBag.SETTING_DISCOUNT_ALL = settingValue.SValue;
            }
            else
            {
                ViewBag.SETTING_DISCOUNT_ALL = 1;
            }
        }
        protected override void Execute(System.Web.Routing.RequestContext requestContext)
        {
            var currentUser = requestContext.HttpContext.User;
        }

        public BaseController()
        {
            //_unitOfWork = new GenericUnitOfWork();
        }
    }
}