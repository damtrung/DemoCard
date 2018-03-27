using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CardSystem.CMS.Models;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using OtpSharp;
using CardSystem.CMS.Helpers;
using CardSystem.Utitlity;
using Vereyon.Web;
//using CardSystem.Model.Entity;
using CardSystem.Repository;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CardSystem.CMS.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, FormCollection fr)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var ip = fr["ipaddress"] != null ? fr["ipaddress"].ToString() : string.Empty;
            var useragent = fr["userAgent"] != null ? fr["userAgent"].ToString() : string.Empty;
            var cptName = fr["computerName"] != null ? fr["computerName"].ToString() : string.Empty;
            Session["USERAGENT"] = useragent;
            Session["IP"] = ip;
            var ipLogin = string.Format("{0}:{1}", ip, useragent);
            var objAuthen = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().GetFirstOrDefaultByParameter(x => x.PhoneNumber.Equals(model.Phone));

            bool checkip = objAuthen != null && (!objAuthen.IPAddress.Equals(ip) || objAuthen.UserAgent == null || !objAuthen.UserAgent.Equals(useragent));
            //var checkpass = await SignInManager.PasswordSignInAsync(model.Phone, model.Password, model.RememberMe, shouldLockout: false);

            //if (objAuthen != null && (!objAuthen.IPAddress.Equals(ip) || objAuthen.UserAgent == null || !objAuthen.UserAgent.Equals(useragent)))
            //{
            //    SendOTP(model.Phone);
            //    Session["PHONE"] = model.Phone;
            //    Session["PASSWORD"] = model.Password;
            //    return RedirectToAction("ConfirmCode", "Account");
            //}
            //else
            //{
            var result = await SignInManager.PasswordSignInAsync(model.Phone, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        if (checkip == true)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            SendOTP(model.Phone);
                            Session["PHONE"] = model.Phone;
                            Session["PASSWORD"] = model.Password;
                            return RedirectToAction("ConfirmCode", "Account");
                        }
                        else
                        {
                            var wallet = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(model.Phone));
                            ViewBag.BALANCE = wallet.Balance;
                            ViewBag.COMISSION = wallet.Commission;
                            var user = UserManager.FindByName(model.Phone);
                            var name = user.UsersInfo.FullName;
                            ViewBag.NAME = name;

                            var usersInfoes = _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().GetFirstOrDefaultByParameter(x => x.ApplicationUserID.Equals(user.Id));
                            usersInfoes.LastLogin = DateTime.Now;
                            _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().Update(usersInfoes);
                            _unitOfWork.SaveChanges();

                            var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = user.Id };
                            var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
                            var lstCartDetails = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
                            ViewBag.TOTALITEMCART = lstCartDetails != null ? lstCartDetails.Count : 0;

                            if (objAuthen != null)
                            {
                                objAuthen.IPAddress = ip;
                                objAuthen.UserAgent = useragent;
                                objAuthen.Updated_At = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                                _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().Update(objAuthen);
                                _unitOfWork.SaveChanges();
                            }
                            else
                            {
                                objAuthen = new CardSystem.Model.Entity.SuTwoAuthenLogin();
                                objAuthen.PhoneNumber = model.Phone;
                                objAuthen.IPAddress = ip;
                                objAuthen.Updated_At = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                                objAuthen.Created_At = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                                _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().Add(objAuthen);
                                _unitOfWork.SaveChanges();
                            }
                            return RedirectToLocal(returnUrl);
                        }

                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    TempData["ErrorMessage"] = "Số điện thoại hoặc mật khẩu không đúng.";
                    return View(model);
            }
            //}
        }
        //
        // GET: /Account/ConfirmCode
        [AllowAnonymous]
        public ActionResult ConfirmCode()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmCode(FormCollection form)
        {
            var code = form["txtCode"] != null ? form["txtCode"].ToString() : string.Empty;
            var phone = Session["PHONE"].ToString();
            var pass = Session["PASSWORD"].ToString();
            var user = UserManager.FindByName(phone);
            var ip = Session["IP"].ToString();
            var usAgent = Session["USERAGENT"].ToString();
            var objAuthen = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().GetFirstOrDefaultByParameter(x => x.PhoneNumber.Equals(phone));
            byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(user.PrivateKey);
            var topt = new Totp(secretKey, step: 600);
            long timeWindowUsed;
            bool isMatch = topt.VerifyTotp(code, out timeWindowUsed, null);
            if (isMatch)
            {
                var result = await SignInManager.PasswordSignInAsync(phone, pass, isPersistent: true, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        {
                            var wallet = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(phone));
                            ViewBag.BALANCE = wallet.Balance;
                            ViewBag.COMISSION = wallet.Commission;

                            var usersInfoes = _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().GetFirstOrDefaultByParameter(x => x.ApplicationUserID.Equals(user.Id));
                            usersInfoes.LastLogin = DateTime.Now;
                            _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().Update(usersInfoes);
                            _unitOfWork.SaveChanges();

                            var name = user.UsersInfo.FullName;
                            ViewBag.NAME = name;
                            var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = user.Id };
                            var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
                            var lstCartDetails = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
                            ViewBag.TOTALITEMCART = lstCartDetails != null ? lstCartDetails.Count : 0;
                            objAuthen.IPAddress = ip;
                            objAuthen.UserAgent = usAgent;
                            objAuthen.Updated_At = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                            _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().Update(objAuthen);
                            _unitOfWork.SaveChanges();
                            return RedirectToAction("", "");
                        }
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    //case SignInStatus.RequiresVerification:
                    //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View();
                }
            }
            else
            {
                TempData["Message"] = "Mã xác nhận không đúng!";
                return View();
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public JsonResult AuthenOTPIfNeed(string phone, string ip)
        {
            var objAuthen = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuTwoAuthenLogin>().GetFirstOrDefaultByParameter(x => x.PhoneNumber.Equals(phone));
            if (objAuthen == null)
            {
                // First time at Delo.vn
                return Json("False", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (objAuthen.IPAddress.Equals(ip))
                {
                    return Json("False", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("True", JsonRequestBehavior.AllowGet);
                }
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            string strPhone = string.Empty;
            if (!Utility.PhoneValidate(model.Phone, out strPhone))
            {
                TempData["ErrorMessage"] = "Số điện thoại không đúng.";
            }
            else if (string.IsNullOrEmpty(model.Password))
            {
                TempData["ErrorMessage"] = "Chưa nhập mật khẩu.";
            }
            else if (string.IsNullOrEmpty(model.ConfirmPassword) || model.ConfirmPassword.Equals(model.Password))
            {
                TempData["ErrorMessage"] = "Nhắc lại mật khẩu không đúng.";
            }
            else if (string.IsNullOrEmpty(model.FullName))
            {
                TempData["ErrorMessage"] = "Tên đầy đủ là bắt buộc.";
            }
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Phone,
                    TwoFactorEnabled = true,
                    PrivateKey = TimeSensitivePassCode.GeneratePresharedKey()
                };
                user.PhoneNumber = model.Phone;
                user.UsersInfo = new SuUsersInfo();
                user.UsersInfo.FullName = model.FullName;
                user.UsersInfo.ApplicationUserID = user.Id;
                user.UsersInfo.Created_At = Utility.CurrentDate();
                user.UsersInfo.IsCustomer = true;
                user.UsersInfo.ParentId = -1;
                user.UsersInfo.LastLogin = DateTime.Now;
                user.UsersInfo.Status = Constant.PENDING;

                user.LockoutEnabled = true;
                user.LockoutEndDateUtc = DateTime.MaxValue;
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    var walletInfo = _unitOfWork.GetRepositoryInstance<Model.Entity.SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(user.UserName));
                    if (walletInfo == null)
                    {
                        walletInfo = new Model.Entity.SuWalletInfo();
                        walletInfo.UserName = user.UserName;
                        walletInfo.Balance = 0;
                        walletInfo.Commission = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals(SettingKeys.SETTING_GIF_MONEY_INIT_ACCOUNT)).SValue.ToIntOrZero();
                        walletInfo.UpdatedAt = Utility.CurrentDate();
                        _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuWalletInfo>().Add(walletInfo);
                        _unitOfWork.SaveChanges();
                    }
                    Session["UserName"] = user.UserName;
                    Session["PrivateKey"] = user.PrivateKey;
                    byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(user.PrivateKey);
                    var topt = new Totp(secretKey, step: 300);
                    if (SendSms(user.UserName, topt.ComputeTotp()) == 100)
                    {
                        return RedirectToAction("ActiveUser", "Account", new { _phone = user.UserName, _key = user.PrivateKey });
                    }
                    else
                    {
                        SendSms(model.Phone, topt.ComputeTotp());
                        return RedirectToAction("ActiveUser", "Account", new { _phone = user.UserName, _key = user.PrivateKey });
                    }
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        if (error.Contains("is already taken."))
                        {
                            // Tài khoản đã tồn tại
                            TempData["ErrorMessage"] = "Số điện thoại đã được đăng ký.";
                        }
                    }
                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            return View(model);
        }
        [AllowAnonymous]
        public async Task<ActionResult> ActiveUser(ActivateBindingModel model)
        {
            ApplicationUser user = null;
            if (Request.QueryString["_phone"] != null)
            {
                model.UserName = Request.QueryString["_phone"].ToString();
            }
            if (Request.QueryString["_key"] != null)
            {
                model.SecretKey = Request.QueryString["_key"].ToString();
            }
            if (model.UserName != null)
            {
                user = UserManager.FindByName(model.UserName);
                SendOTP(model.UserName);
                string privateKey = Constant.PrivateKeyForAnonymous;
                if (user != null)
                {
                    privateKey = user.PrivateKey;
                    byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(privateKey);
                    Session["PrivateKey"] = privateKey;
                    var topt = new Totp(secretKey, step: 600);
                    Common.CommonUtils.SendSms(user.UserName, topt.ComputeTotp());
                    Session["USER_ACTIVE"] = user;
                }
                else
                {

                }

            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult ActiveAccount(string otp, string InviteNumber)
        {
            int parrentId = -1;
            string userName = string.Empty;
            string strSecretKey = string.Empty;
            string strResult = string.Empty;
            ApplicationUser user = null;

            if (Session["USER_ACTIVE"] != null)
            {
                user = Session["USER_ACTIVE"] as ApplicationUser;
            }
            else
            {
                // TODO
            }
            if (Utility.PhoneValidate(InviteNumber))
            {
                var inviteUser = UserManager.FindByName(InviteNumber);
                var walletInviteUser = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(inviteUser.UserName));
                if (inviteUser == null)
                {
                    // TODO
                    // Ko tim thay nguoi gioi thieu 
                    strResult = "Không tìm thấy người bảo lãnh trên hệ thống hoặc người bảo lãnh không đủ điều kiện bảo lãnh!";
                }
                else
                {
                    parrentId = inviteUser.UsersInfo.Id;
                }
            }
            else
            {
                // TODO
                strResult = "Định dạng số điện thoại không đúng.Vui lòng nhập đúng số điện thoại!";
            }
            byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(user.PrivateKey);
            var topt = new Totp(secretKey, step: 600);
            long timeWindowUsed;
            bool isMatch = topt.VerifyTotp(otp, out timeWindowUsed, null);
            if (isMatch)
            {
                if (user != null)
                {
                    var result = UserManager.SetLockoutEnabledAsync(user.Id, false);
                    if (result.Result.Succeeded)
                    {
                        var suUsersInfo = _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().GetFirstOrDefaultByParameter(x => x.ApplicationUserID.Equals(user.Id));
                        suUsersInfo.Status = 1;
                        suUsersInfo.ParentId = parrentId;
                        _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().Update(suUsersInfo);
                        _unitOfWork.SaveChanges();
                        //TO DO
                        // Kích hoạt thành công
                        strResult = "Success";
                    }
                    else
                    {
                        //TO DO
                        // Kích hoạt không thành công
                        strResult = "Lỗi khi kích hoạt tài khoản!";
                    }
                }
                else
                {
                    strResult = "Số điện thoại chưa được đăng ký!";
                }
            }
            else
            {
                strResult = "Mã xác thực không đúng.Vui lòng nhập lại hoặc nhận lại mã xác thực nếu mã xác thực hết hạn!";
            }
            return Json(strResult, JsonRequestBehavior.AllowGet); ;
        }
        [HttpPost]
        [AllowAnonymous]
        public JsonResult SendOTP(string phone)
        {
            try
            {
                var userInfo = UserManager.FindByName(phone);
                string privateKey = Constant.PrivateKeyForAnonymous;
                if (userInfo != null)
                {
                    privateKey = userInfo.PrivateKey;
                }
                byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(privateKey);
                Session["PrivateKey"] = privateKey;
                var topt = new Totp(secretKey, step: 600);
                Common.CommonUtils.SendSms(userInfo.UserName, topt.ComputeTotp());
                return Json("Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Lỗi không thể nhận mã xác thực OTP,vui lòng quay lại sau.", JsonRequestBehavior.AllowGet);
            }

        }
        public int SendSms(string phone, string otp)
        {
            string APIKey = "A753D2DB0B63FB6F694D277A698A9B";//Login to eSMS.vn to get this";//Dang ky tai khoan tai esms.vn de lay key//Register account at esms.vn to get key
            string SecretKey = "353319E3FAB9D208019EBAED3F0F3B";//Login to eSMS.vn to get this";

            // Create URL, method 1:
            string URL = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone=" + phone + "&Content=" + otp + "&ApiKey=" + APIKey + "&SecretKey=" + SecretKey + "&IsUnicode=0&SmsType=4";
            //-----------------------------------
            //-----------------------------------
            string result = SendGetRequest(URL);
            JObject ojb = JObject.Parse(result);
            int CodeResult = (int)ojb["CodeResult"];//100 is successfull

            string SMSID = (string)ojb["SMSID"];//id of SMS
            return CodeResult;
        }
        private string SendGetRequest(string RequestUrl)
        {
            Uri address = new Uri(RequestUrl);
            HttpWebRequest request;
            HttpWebResponse response = null;
            StreamReader reader;
            if (address == null) { throw new ArgumentNullException("address"); }
            try
            {
                request = WebRequest.Create(address) as HttpWebRequest;
                request.UserAgent = ".NET Platform";
                request.KeepAlive = false;
                request.Timeout = 15 * 1000;
                response = request.GetResponse() as HttpWebResponse;
                if (request.HaveResponse == true && response != null)
                {
                    reader = new StreamReader(response.GetResponseStream());
                    string result = reader.ReadToEnd();
                    result = result.Replace("</string>", "");
                    return result;
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        Console.WriteLine(
                            "The server returned '{0}' with the status code {1} ({2:d}).",
                            errorResponse.StatusDescription, errorResponse.StatusCode,
                            errorResponse.StatusCode);
                    }
                }
            }
            finally
            {
                if (response != null) { response.Close(); }
            }
            return null;
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        public async Task<ActionResult> ForgotPassword(FormCollection form)
        {
            //if (ModelState.IsValid)
            //{
            //    var user = await UserManager.FindByNameAsync(model.Email);
            //    if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            //    {
            //        // Don't reveal that the user does not exist or is not confirmed
            //        return View("ForgotPasswordConfirmation");
            //    }

            //    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            //    // Send an email with this link
            //    // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            //    // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
            //    // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
            //    // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            //}

            //// If we got this far, something failed, redisplay form
            //return View(model);
            var phone = form["txtphone"] != null ? form["txtphone"].ToString() : string.Empty;
            if (Utility.PhoneValidate(phone))
            {
                var password = Utility.CreatePassword(8);
                var userInfo = _unitOfWork.GetRepositoryInstance<CardSystem.Model.Entity.AspNetUsers>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(phone));
                if (userInfo != null)
                {
                    string code = await UserManager.GeneratePasswordResetTokenAsync(userInfo.Id);
                    IdentityResult result = await UserManager.ResetPasswordAsync(userInfo.Id, code, password);
                    if (!result.Succeeded)
                    {
                        TempData["Message"] = "Nhận lại mật khẩu không thành công";
                        return View();
                    }
                    else
                    {
                        TempData["Message"] = "Mật khẩu đã được gửi về số điện thoại";
                        SendSms(phone, password);
                        //return RedirectToAction("ForgotPasswordConfirmation");
                        return View();
                    }
                }
                else
                {
                    TempData["Message"] = "Số điện thoại không tồn tại trên hệ thống";
                    return View();
                }
            }
            else
            {
                TempData["Message"] = "Số điện thoại nhập vào không đúng";
                return View();
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
    public class ActivateBindingModel
    {
        //[Required]
        public string UserName { get; set; }
        //[Required]
        public string Otp { get; set; }
        //[Required]
        public string SecretKey { get; set; }
        public string InviteNumber { get; set; }
    }
    public class JsonActive
    {
        public string otp { get; set; }
        public string InviteNumber { get; set; }
    }
}