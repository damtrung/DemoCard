using CardSystem.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Model.Entity;
using System.Net;
using System.Data.SqlClient;
using Microsoft.AspNet.Identity;
using System.Data.Entity.SqlServer;
using Newtonsoft.Json;
using CardSystem.Services;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using CardSystem.Utitlity;
using System.Web.Configuration;

namespace CardSystem.CMS.Controllers
{
    public class YourProfileController : BaseController
    {
        public YourProfileController() : base()
        {

        }
        // GET: NguoiDung
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult LichSuGiaoDich()
        {
            List<Ad_SuTransaction_Full_Result> lstTransaction = new List<Ad_SuTransaction_Full_Result>();
            var fromDate = Request.Form["txtTransactionDate"] != null ? Request.Form["txtTransactionDate"].ToString() : "";
            Session["FROMDATE"] = fromDate;
            var toDate = Request.Form["txtTransactionDateTo"] != null ? Request.Form["txtTransactionDateTo"].ToString() : "";
            var toUser = Request.Form["txtphone"] != null ? Request.Form["txtphone"].ToString() : "";
            var seri = Request.Form["txtSeri"] != null ? Request.Form["txtSeri"].ToString() : "";
            var code = Request.Form["txtCode"] != null ? Request.Form["txtCode"].ToString() : "";
            var price = Request.Form["txtPrice"] != null ? Request.Form["txtPrice"] : string.Empty;
            var serviceCode = Request.Form["txtServiceCode"] != null ? Request.Form["txtServiceCode"].ToString() : "";

            float ToPrice;
            if (price != "")
            {
                ToPrice = float.Parse(price);
            }
            else
            {
                ToPrice = 0;
            }
            fromDate = " " + fromDate.Trim();
            toDate = " " + toDate.Trim();
            var fromTime = Request.Form["txtTime"] != null ? Request.Form["txtTime"].ToString() : "";
            var toTime = Request.Form["txtTimeTo"] != null ? Request.Form["txtTimeTo"].ToString() : "";
            string fromDateTime = string.Concat(fromTime, fromDate);
            string toDateTime = string.Concat(toTime, toDate);
            var type = int.Parse(Request.Form["service"] != null ? Request.Form["service"] : "0");
            var sqlUser = new SqlParameter("Created_By", System.Data.SqlDbType.NVarChar) { Value = User.Identity.Name.ToString() };
            //var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = "0972102666" };
            var sqlFromDate = new SqlParameter("FromDate", System.Data.SqlDbType.NVarChar) { Value = fromDateTime };
            var sqlToDate = new SqlParameter("ToDate", System.Data.SqlDbType.NVarChar) { Value = toDateTime };
            var sqlToUser = new SqlParameter("ToUser", System.Data.SqlDbType.NVarChar) { Value = toUser };
            var sqltype = new SqlParameter("TransactionType", System.Data.SqlDbType.Int) { Value = type };
            var sqlSeri = new SqlParameter("Serial", System.Data.SqlDbType.NVarChar) { Value = seri };
            var sqlCode = new SqlParameter("Code", System.Data.SqlDbType.NVarChar) { Value = code };
            var sqlPrice = new SqlParameter("ToPrice", System.Data.SqlDbType.Float) { Value = ToPrice };
            var sqlServiceCode = new SqlParameter("ServiceCode", System.Data.SqlDbType.NVarChar) { Value = serviceCode };

            var sqlFromPrice = new SqlParameter("FromPrice", System.Data.SqlDbType.Float) { Value = DBNull.Value };
            var sqlServiId = new SqlParameter("ServiceID", System.Data.SqlDbType.Int) { Value = DBNull.Value };
            var sqlStatus = new SqlParameter("Status", System.Data.SqlDbType.Int) { Value = DBNull.Value };
            var sqlFromUpdatedAt = new SqlParameter("FromUpdated_At", System.Data.SqlDbType.NVarChar) { Value = "" };
            var sqlToUpdatedAt = new SqlParameter("ToUpdated_At", System.Data.SqlDbType.NVarChar) { Value = "" };

            var sortBy = new SqlParameter("SortBy", System.Data.SqlDbType.Int) { Value = 1 };
            var sortDirect = new SqlParameter("SortDirect", System.Data.SqlDbType.Int) { Value = 1 };
            var pageIndex = new SqlParameter("PageIndex", System.Data.SqlDbType.Int) { Value = 1 };
            var pageSize = new SqlParameter("PageSize", System.Data.SqlDbType.Int) { Value = 100 };
            lstTransaction = _unitOfWork.GetRepositoryInstance<Ad_SuTransaction_Full_Result>().GetResultBySqlProcedure("Ad_SuTransaction_Full @FromDate,@ToDate,@Created_By,@Status,@TransactionType,@ServiceID,@Code,@Serial,@ServiceCode,@FromPrice,@ToPrice,@ToUser,@FromUpdated_At,@ToUpdated_At,@SortBy,@SortDirect,@PageIndex,@PageSize", sqlFromDate, sqlToDate, sqlUser, sqlStatus, sqltype, sqlServiId, sqlCode, sqlSeri,sqlServiceCode, sqlFromPrice, sqlPrice, sqlToUser, sqlFromUpdatedAt, sqlToUpdatedAt, sortBy, sortDirect, pageIndex, pageSize).ToList();
            return View(lstTransaction.OrderByDescending(x => x.Id));
        }

        [Authorize]
        public ActionResult CapNhatProfile()
        {
            SuUsersInfoes SuUsersInfoes = new SuUsersInfoes();
            AspNetUsers aspNetUser = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(o => o.UserName == User.Identity.Name);
            if (aspNetUser != null)
            {
                SuUsersInfoes = _unitOfWork.GetRepositoryInstance<SuUsersInfoes>().GetFirstOrDefaultByParameter(o => o.Id == aspNetUser.UsersInfo_Id);
            }

            if (SuUsersInfoes == null)
            {
                return HttpNotFound();
            }

            return View(SuUsersInfoes);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CapNhatProfile(FormCollection form)
        {
            SuUsersInfoes SuUsersInfoes = new SuUsersInfoes();
            AspNetUsers aspNetUser = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(o => o.UserName == User.Identity.Name);
            SuUsersInfoes = _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().GetFirstOrDefaultByParameter(o => o.Id == aspNetUser.UsersInfo_Id);
            string message = "";
            if (Request.Files.Count > 0)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    int length = file.ContentLength;
                    string type = file.ContentType;
                    string filename = DateTime.Now.ToString("hh_ss_dd_MM_yyy_") + file.FileName;
                    if (length > 0)
                    {
                        Utility.UploadImage(ref message, file, filename, Constant.USER_ORIGINALS, Constant.USER_THUMBNAILS, Server);
                        if (message.Length > 0)
                        {
                            break;
                        }
                        //viewModel.articlesInfo.ImageTitle = filename;
                        string publicRootPath = WebConfigurationManager.AppSettings["FileServer"].ToString();
                        SuUsersInfoes.Picture = publicRootPath + "Users/Thumbnails/" + filename;
                    }
                }
            }
            //SuUsersInfoes.Picture = form["fUploadAvata"];
            SuUsersInfoes.FullName = form["txtfullname"];
            SuUsersInfoes.CMND = form["txtcmnn"];
            SuUsersInfoes.Address = form["txtaddress"];
            SuUsersInfoes.BirthDay = form["txtbirhday"];
            _unitOfWork.GetRepositoryInstance<Model.Entity.SuUsersInfoes>().Update(SuUsersInfoes);
            _unitOfWork.SaveChanges();
            return RedirectToAction("CapNhatProfile");
        }

        [Authorize]
        public ActionResult DoiMatKhau()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DoiMatKhau(FormCollection form)
        {
            AspNetUsers aspNetUser = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(o => o.UserName == User.Identity.Name);
            IdentityResult result = await UserManager.ChangePasswordAsync(aspNetUser.Id, form["txtOldPassword"].ToString(), form["txtNewPassWord"].ToString());
            if (result.Succeeded)
            {
                TempData["Message"] = "Mật khẩu thay đổi thành công";
                return View(); 
            }
            else
            {
                TempData["Message"] = "Mật khẩu cũ chưa đúng, vui lòng xem lại";
                return View();
            }
            //if (form["txtOldPassword"] == aspNetUser.PasswordHash)
            //{
            //    aspNetUser.PasswordHash = form["txtNewPassWord"];
            //    _unitOfWork.GetRepositoryInstance<Model.Entity.AspNetUsers>().Update(aspNetUser);
            //    _unitOfWork.SaveChanges();
            //}
        }
        [Authorize]
        public ActionResult DoiMaPin()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> DoiMaPin(FormCollection form)
        {
            AspNetUsers aspNetUser = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(o => o.UserName == User.Identity.Name);
            IdentityResult result = await UserManager.ChangePasswordAsync(aspNetUser.Id, form["txtOldPassword"].ToString(), form["txtNewPassWord"].ToString());
            if (result.Succeeded)
            {
                TempData["Message"] = "Mã pin thay đổi thành công";
                return View();
            }
            else
            {
                TempData["Message"] = "Mã pin cũ chưa đúng, vui lòng xem lại";
                return View();
            }
        }
        [Authorize]
        public ActionResult PhuongThucBaoMat()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult PhuongThucBaoMat(FormCollection form)
        {
            return View();
        }
        public ActionResult TransactionDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SuTransaction transactionDetail = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetFirstOrDefault(id.Value);
            if (transactionDetail == null)
            {
                return HttpNotFound();
            }
            return View(transactionDetail);
        }
        public ActionResult PaymentDetail(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SuInstallmentPayment mentPayment = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefault(id.Value);
            if (mentPayment == null)
            {
                return HttpNotFound();
            }
            return View(mentPayment);
        }
        [Authorize]
        public ActionResult BaoCaoDoanhThu()
        {
            var userInfo = UserManager.FindByName(User.Identity.Name.ToString());
            var lstUser = _unitOfWork.GetRepositoryInstance<SuUsersInfoes>().GetListByParameter(x => x.ParentId == userInfo.UsersInfo.Id);
            List<Profit_Full_Ad_Result> doanhthuDetails = new List<Profit_Full_Ad_Result>();
            if (Request.Form["txtTransactionDate"] != null && Request.Form["txtTransactionDateTo"] != null)
            {
                string date = Request.Form["txtTransactionDate"] != null ? Request.Form["txtTransactionDate"].ToString() : "";
                string dateTo = Request.Form["txtTransactionDateTo"] != null ? Request.Form["txtTransactionDateTo"].ToString() : "";
                date = " " + date.Trim();
                dateTo = " " + dateTo.Trim();
                var fromTime = Request.Form["txtTime"] != null ? Request.Form["txtTime"].ToString() : "";
                var toTime = Request.Form["txtTimeTo"] != null ? Request.Form["txtTimeTo"].ToString() : "";
                string fromDateTime = string.Concat(fromTime, date);
                string toDateTime = string.Concat(toTime, dateTo);
                var type = int.Parse(Request.Form["service"] != null ? Request.Form["service"] : "0");
                var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.Name.ToString() };
                //var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = "0972102666" };
                var sqlFromDate = new SqlParameter("fromDate", System.Data.SqlDbType.NVarChar) { Value = fromDateTime };
                var sqlToDate = new SqlParameter("toDate", System.Data.SqlDbType.NVarChar) { Value = toDateTime };
                var sqlParentId = new SqlParameter("parentId", System.Data.SqlDbType.Int) { Value = userInfo.UsersInfo.Id };
                var sqlType = new SqlParameter("type", System.Data.SqlDbType.Int) { Value = type };
                //var sqlParentId = new SqlParameter("parentId", System.Data.SqlDbType.Int) { Value = 65 };
                //if (Request.Form["profit_grade"] != null && Request.Form["profit_grade"] == "true")
                //{
                //    doanhthuDetails = _unitOfWork.GetRepositoryInstance<Profit_Full_Web_Result>().GetResultBySqlProcedure("Profit_Grade_Lower @parentId, @fromDate, @toDate", sqlParentId, sqlFromDate, sqlToDate).ToList();
                //}
                //else
                //{
                //    doanhthuDetails = _unitOfWork.GetRepositoryInstance<Profit_Full_Web_Result>().GetResultBySqlProcedure("Profit_Full_Web @userId, @fromDate, @toDate", sqlUser, sqlFromDate, sqlToDate).ToList();
                //}
                doanhthuDetails = _unitOfWork.GetRepositoryInstance<Profit_Full_Ad_Result>().GetResultBySqlProcedure("Profit_Full_Web @userId, @fromDate, @toDate,@type", sqlUser, sqlFromDate, sqlToDate, sqlType).ToList();
            }
            else
            {
                return View();
            }
            return View(doanhthuDetails);
        }

        public ActionResult BaoCaoDoanhSoDaiLy()
        {
            var userInfo = UserManager.FindByName(User.Identity.Name.ToString());
            var lstUser = _unitOfWork.GetRepositoryInstance<SuUsersInfoes>().GetListByParameter(x => x.ParentId == userInfo.UsersInfo.Id);
            List<Profit_Full_Web_Result> doanhthuDetails = new List<Profit_Full_Web_Result>();
            if (Request.Form["txtTransactionDate"] != null && Request.Form["txtTransactionDateTo"] != null)
            {
                string date = Request.Form["txtTransactionDate"] != null ? Request.Form["txtTransactionDate"].ToString() : "";
                string dateTo = Request.Form["txtTransactionDateTo"] != null ? Request.Form["txtTransactionDateTo"].ToString() : "";
                var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.Name.ToString() };
                var sqlFromDate = new SqlParameter("fromDate", System.Data.SqlDbType.NVarChar) { Value = date };
                var sqlToDate = new SqlParameter("toDate", System.Data.SqlDbType.NVarChar) { Value = dateTo };
                var sqlParentId = new SqlParameter("parentId", System.Data.SqlDbType.Int) { Value = userInfo.UsersInfo.Id };
                //var sqlParentId = new SqlParameter("parentId", System.Data.SqlDbType.Int) { Value = 65 };
                doanhthuDetails = _unitOfWork.GetRepositoryInstance<Profit_Full_Web_Result>().GetResultBySqlProcedure("Profit_Grade_Lower @parentId, @fromDate, @toDate", sqlParentId, sqlFromDate, sqlToDate).ToList();
            }
            else
            {
                return View();
            }
            return View(doanhthuDetails);
        }

        public ActionResult LichSuSoDu()
        {
            List<Transaction_TransferMoney_Result> lstTransaction = new List<Transaction_TransferMoney_Result>();
            List<Profit_CurrentBalance_Result> startDate = new List<Profit_CurrentBalance_Result>();
            List<Profit_CurrentBalance_Result> endDate = new List<Profit_CurrentBalance_Result>();
            var fromDate = Request.Form["txtTransactionDate"] != null ? Request.Form["txtTransactionDate"].ToString() : "";
            var toDate = Request.Form["txtTransactionDateTo"] != null ? Request.Form["txtTransactionDateTo"].ToString() : "";
            fromDate = " " + fromDate.Trim();
            toDate = " " + toDate.Trim();
            
            var fromTime = "00:00:00";
        
            string fromDateTime = string.Concat(fromTime, fromDate);


            var sqlUser = new SqlParameter("user", System.Data.SqlDbType.NVarChar) { Value = User.Identity.Name.ToString() };
            //var sqlUser = new SqlParameter("user", System.Data.SqlDbType.NVarChar) { Value = "0972102666" };
            var sqlTypeStartsDate = new SqlParameter("type", System.Data.SqlDbType.Int) { Value = 1 };
            
            var sqlFromDateTime = new SqlParameter("date", System.Data.SqlDbType.NVarChar) { Value = fromDateTime };
            

            startDate = _unitOfWork.GetRepositoryInstance<Profit_CurrentBalance_Result>().GetResultBySqlProcedure("Profit_CurrentBalance @user, @date , @type", sqlUser, sqlFromDateTime, sqlTypeStartsDate).ToList();
            
            foreach(var item in startDate)
            {
                Session["CURRENT_STARTDATE"] = item.CurrentBalance != null ? item.CurrentBalance : 0;
            }
            var frTime = Request.Form["txtTime"] != null ? Request.Form["txtTime"].ToString() : "";
            var toTime = Request.Form["txtTimeTo"] != null ? Request.Form["txtTimeTo"].ToString() : "";
            string frDateTime = string.Concat(frTime, fromDate);
            string toDateTime = string.Concat(toTime, toDate);
            var sqlUserId = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.Name.ToString() };
            //var sqlUserId = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = 0972102666 };
            var sqlFromDate = new SqlParameter("fromDate", System.Data.SqlDbType.NVarChar) { Value = frDateTime };
            var sqlToDate = new SqlParameter("toDate", System.Data.SqlDbType.NVarChar) { Value = toDateTime };
            lstTransaction = _unitOfWork.GetRepositoryInstance<Transaction_TransferMoney_Result>().GetResultBySqlProcedure("Transaction_TransferMoney @userId, @fromDate, @toDate", sqlUserId, sqlFromDate, sqlToDate).ToList();
            return View(lstTransaction.OrderByDescending(x => x.Id));
        }
        public ActionResult PrintCard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SuTransaction transaction = new SuTransaction();
            transaction = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetFirstOrDefault(id.Value);
            var listPay = new List<PayModel>();
            if (transaction != null && transaction.Info != null)
            {
                dynamic dataInfo = JsonConvert.DeserializeObject(transaction.Info);
                if (dataInfo != null)
                {
                    foreach (var item in dataInfo)
                    {
                        var viewmodel = new PayModel();
                        var code = TripleDes.Decrypt(EcoPayKey.Scret_Key, item["Code"].Value);
                        var serial = item["Serial"];
                        var expriedDate = item["ExpriedDate"];
                        var price = item["Price"];
                        //viewModel.Code = code;
                        viewmodel.Code = code.Insert(4, "-").Insert(10, "-");
                        viewmodel.Serial = serial;
                        viewmodel.ExpriedDate = expriedDate;
                        viewmodel.Price = price;
                        viewmodel.Phone = transaction.Created_By;
                        if (transaction.ServiceCode.Contains("VTC"))
                        {
                            viewmodel.ServiceCode = "VTC";
                            viewmodel.CSKH = "1800 6682";
                        }
                        else if (transaction.ServiceCode.Contains("VT"))
                        {
                            viewmodel.ServiceCode = "Viettel";
                            viewmodel.CSKH = "1800 8098";
                        }
                        else if (transaction.ServiceCode.Contains("MB"))
                        {
                            viewmodel.ServiceCode = "Mobifone";
                            viewmodel.CSKH = "1800 1090";
                        }
                        else if (transaction.ServiceCode.Contains("VNM"))
                        {
                            viewmodel.ServiceCode = "Vietnamobile";
                            viewmodel.CSKH = "0922.789.789";
                        }
                        else if (transaction.ServiceCode.Contains("VNG"))
                        {
                            viewmodel.ServiceCode = "Zing";
                            viewmodel.CSKH = "1900 561558";
                        }
                        else if (transaction.ServiceCode.Contains("VN"))
                        {
                            viewmodel.ServiceCode = "Vinaphone";
                            viewmodel.CSKH = "1800 1091";
                        }
                        else if (transaction.ServiceCode.Contains("GATE"))
                        {
                            viewmodel.ServiceCode = "Gate";
                            viewmodel.CSKH = "1900 6611";
                        }
                        else if (transaction.ServiceCode.Contains("GTEL"))
                        {
                            viewmodel.ServiceCode = "Gtel";
                            viewmodel.CSKH = "1800 6682";
                        }
                        else if (transaction.ServiceCode.Contains("GARENA"))
                        {
                            viewmodel.ServiceCode = "Sò";
                            viewmodel.CSKH = "1900 1282";
                        }
                        else if (transaction.ServiceCode.Contains("VCARD"))
                        {
                            viewmodel.ServiceCode = "Vip Rik";
                            viewmodel.CSKH = "1900 6117";
                        }
                        else if (transaction.ServiceCode.Contains("ONCASH"))
                        {
                            viewmodel.ServiceCode = "ONCASH";
                            viewmodel.CSKH = "1800 6682";
                        }
                        else if (transaction.ServiceCode.Contains("BIT"))
                        {
                            viewmodel.ServiceCode = "BIT";
                            viewmodel.CSKH = "1900 7189";
                        }
                        else if (transaction.ServiceCode.Contains("MEGA"))
                        {
                            viewmodel.ServiceCode = "MEGA";
                            viewmodel.CSKH = "1900 1282";
                        }
                        else if (transaction.ServiceCode.Contains("K"))
                        {
                            viewmodel.ServiceCode = "K+";
                            viewmodel.CSKH = "1900 1592";
                        }
                        listPay.Add(viewmodel);
                    }
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View();
            }
            return View(listPay);
        }

        public ActionResult PrintPayment(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SuInstallmentPayment menPayment = new SuInstallmentPayment();
            menPayment = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefault(id.Value);
            return View(menPayment);
        }
    }
}