using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Services;
using CardSystem.Model.Entity;
using CardSystem.Utitlity;
using CardSystem.Repository;
using OtpSharp;
using Vereyon.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CardSystem.CMS.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace CardSystem.CMS.Controllers
{
    [Authorize]
    [RoutePrefix("dich-vu")]
    public class PayController : BaseController
    {
        public PayController() : base()
        {

        }
        // GET: Pay
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NapThe()
        {
            var settingValue = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_SYSTEM_ERROR"));
            var settingPromotion = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_PROMOTION"));
            ViewBag.PROMOTION = settingPromotion.SValue;
            if (settingValue.SValue.ToIntOrZero() == 1)
            {
                return RedirectToAction("SystemError", "Home");
            }
            else
            {
                var art = _unitOfWork.GetRepositoryInstance<SuCategory>().GetFirstOrDefaultByParameter(x => x.Id == 25);
                if (art != null)
                {
                    TempData["MessagePay"] = art.Description;
                }
                else
                {
                    TempData["MessagePay"] = "";
                }
                return View();
            }
        }
        public ActionResult MuaThe()
        {
            //var settingValue = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_SYSTEM_ERROR"));
            //if (settingValue.SValue.ToIntOrZero() == 1)
            //{
            //    return RedirectToAction("SystemError", "Home");
            //}
            //else
            //{
            //    var art = _unitOfWork.GetRepositoryInstance<SuCategory>().GetFirstOrDefaultByParameter(x => x.Id == 25);
            //    if (art != null)
            //    {
            //        TempData["MessagePay"] = art.Description;
            //    }
            //    else
            //    {
            //        TempData["MessagePay"] = "";
            //    }
            //    return View();
            //}
            return View();
        }

        public ActionResult TraGop()
        {
            return View();
        }
        public ActionResult PaymentService()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MuaThe(FormCollection form)
        {
            return View();
        }
        public ActionResult ChuyenTien()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ChuyenTien(FormCollection form)
        {
            return View();
        }
        /// <summary>
        ///  TopUp  function
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PushPriceCard(FormCollection form)
        {
            string strMess = string.Empty;
            int bResult = 0;

            //double subtractValue = 0;
            var phone = form["txtphone"] != null ? form["txtphone"].ToString() : string.Empty;
            var phoneType = form["hf-Type"] != null ? form["hf-Type"].ToString() : string.Empty;
            var serviceCode = form["hf-ddlCardCode"] != null ? form["hf-ddlCardCode"].ToString() : string.Empty;
            var agency = string.Empty;
            bool isValid = Utility.PhoneValidate(phone, out agency);
            var content = form["txtnoidungnapthe"] != null ? form["txtnoidungnapthe"].ToString() : string.Empty;
            var price = form["hf-ddlCardCode"] != null ? form["hf-ddlCardCode"].ToString() : string.Empty;
            var bonus = form["txtBonus"] != null ? form["txtBonus"].ToString().ToIntOrZero() : 0;
            var pricePostpaid = form["txtPostpaid"] != null ? form["txtPostpaid"].ToString() : string.Empty;
            int discount = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_ALL")).SValue.ToIntOrZero();

            CardModelBinding cardInfo = new CardModelBinding();
            cardInfo.ReceiptNumber = Guid.NewGuid().ToString();
            if (agency.Equals("Viettel"))
            {
                serviceCode = phoneType == "1" ? EcoPayServiceCode.VTEL_PREPAID : EcoPayServiceCode.VTEL_POSTPAID;
            }
            else if (agency.Equals("Mobifone"))
            {
                serviceCode = phoneType == "1" ? EcoPayServiceCode.MOBI_PREPAID : EcoPayServiceCode.MOBI_POSTPAID;

            }
            else if (agency.Equals("Vinaphone"))
            {
                serviceCode = phoneType == "1" ? EcoPayServiceCode.VINA_PREPAID : EcoPayServiceCode.VINA_POSTPAID;

            }
            else if (agency.Equals("VNM"))
            {
                //  TODO
                serviceCode = phoneType == "1" ? EcoPayServiceCode.VNMB_PREPAID : EcoPayServiceCode.VNMB_POSTPAID;
            }
            else
            {
                TempData["ErrorMessage"] = "Hệ thống không nhận diện được số điện thoại cần nạp,vui lòng thử lại.";
                return RedirectToAction("NapThe", "Pay");
            }
            cardInfo.ServiceCode = new List<string> { serviceCode };
            cardInfo.PhoneNumber = phone;
            if (price == "" && pricePostpaid == "")
            {
                TempData["ErrorMessage"] = "Không thực hiện được giao dịch,vui lòng thử lại.";
                return RedirectToAction("NapThe", "Pay");
            }
            else
            {
                cardInfo.Price = float.Parse(pricePostpaid);
            }
            cardInfo.Amount = 1;
            try
            {
                var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                if (wallet.Balance < cardInfo.Price || wallet == null)
                {
                    TempData["ErrorMessage"] = "Số dư của bạn không đủ để thực hiện giao dịch, vui lòng nạp thêm tài khoản.";
                    return RedirectToAction("NapThe", "Pay");
                }
                cardInfo.ReceiptNumber = Guid.NewGuid().ToString();
                var ecoResponse = EcoPayServices.TopUpCard(cardInfo.ReceiptNumber, cardInfo.ServiceCode[0], cardInfo.PhoneNumber, cardInfo.Price);

                if (ecoResponse != null)
                    try
                    {
                        #region Can check code
                        var rspTran = ecoResponse.Data.Transaction;
                        if (rspTran != null)
                        {
                            //rspTran.Status = 2;
                            switch (rspTran.Status)
                            {
                                case 0:
                                    {
                                        #region Status 0
                                        bResult = 0;
                                        var transaction = new SuTransaction();
                                        transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                        transaction.Created_By = User.Identity.Name;
                                        transaction.ServiceCode = cardInfo.ServiceCode[0];
                                        transaction.Status = 0;
                                        transaction.Amout = cardInfo.Amount;
                                        transaction.BonusClient = bonus;
                                        //transaction.Code = ecoResponse.Data.Transaction.Code;
                                        transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                        transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                        transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                        transaction.TransactionType = 2;
                                        transaction.Price = cardInfo.Price;
                                        transaction.ToUser = cardInfo.PhoneNumber;
                                        transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                        transaction.Description = content;
                                        strMess = "Lỗi xảy ra khi thực hiện được giao dịch, vui lòng quay lại sau.";
                                        _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                        _unitOfWork.SaveChanges();
                                        #endregion
                                        break;
                                    }
                                case 1:
                                    {
                                        #region Status 1,2
                                        bResult = 2;
                                        strMess = "Giao dịch đang được xác nhận.Quý khách vui lòng đợi trong ít phút.";
                                        TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                                        try
                                        {
                                            var transaction = new SuTransaction();
                                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                            transaction.Created_By = User.Identity.Name;
                                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                                            transaction.Status = ecoResponse.Data.Transaction.Status;
                                            transaction.Amout = cardInfo.Amount;
                                            transaction.BonusClient = bonus;
                                            //transaction.Code = ecoResponse.Data.Transaction.Code;
                                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                            transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                            transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                            transaction.TransactionType = 2;
                                            transaction.Price = cardInfo.Price;
                                            transaction.ToUser = cardInfo.PhoneNumber;
                                            transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                            transaction.Description = content;
                                            if (transaction.ServiceCode.Contains("VTEL"))
                                            {
                                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("MOBI"))
                                            {
                                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VNMB"))
                                            {
                                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VINA"))
                                            {
                                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("GTEL"))
                                            {
                                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else
                                            {
                                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                            }
                                            // Update wallet info
                                            if (wallet != null)
                                            {
                                                transaction.CurrentBalance = wallet.Balance;
                                                var realCharge = transaction.Price - transaction.SysDiscount;
                                                transaction.SysChargePrice = realCharge;
                                                wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                                ViewBag.BALANCE = wallet.Balance;
                                                ViewBag.COMISSION = wallet.Commission;
                                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                                _unitOfWork.SaveChanges();
                                            }
                                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                            _unitOfWork.SaveChanges();
                                            TempData["TRANSACTION_WAITING"] = transaction;

                                        }
                                        catch (Exception)
                                        {
                                            // Retry to get transaction detail
                                            var transaction = new SuTransaction();
                                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                            transaction.Created_By = User.Identity.Name;
                                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                                            transaction.Status = 1;
                                            transaction.Amout = cardInfo.Amount;
                                            transaction.BonusClient = bonus;
                                            //transaction.Code = ecoResponse.Data.Transaction.Code;
                                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                            transaction.TransactionType = 2;
                                            transaction.Price = cardInfo.Price;
                                            transaction.ToUser = cardInfo.PhoneNumber;
                                            transaction.Description = content;
                                            if (transaction.ServiceCode.Contains("VTEL"))
                                            {
                                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("MOBI"))
                                            {
                                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VNMB"))
                                            {
                                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VINA"))
                                            {
                                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("GTEL"))
                                            {
                                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else
                                            {
                                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                            }
                                            // Update wallet info
                                            if (wallet != null)
                                            {
                                                transaction.CurrentBalance = wallet.Balance;
                                                var realCharge = transaction.Price - transaction.SysDiscount;
                                                transaction.SysChargePrice = realCharge;
                                                wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                                ViewBag.BALANCE = wallet.Balance;
                                                ViewBag.COMISSION = wallet.Commission;
                                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                                _unitOfWork.SaveChanges();
                                            }
                                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                            _unitOfWork.SaveChanges();
                                            TempData["TRANSACTION_WAITING"] = transaction;
                                        }
                                        #endregion
                                        break;
                                    }
                                case 2:
                                    {
                                        #region Status 1,2
                                        bResult = 2;
                                        strMess = "Giao dịch đang được xác nhận.Quý khách vui lòng đợi trong ít phút.";
                                        TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                                        try
                                        {
                                            var transaction = new SuTransaction();
                                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                            transaction.Created_By = User.Identity.Name;
                                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                                            transaction.Status = ecoResponse.Data.Transaction.Status;
                                            transaction.Amout = cardInfo.Amount;
                                            transaction.BonusClient = bonus;
                                            //transaction.Code = ecoResponse.Data.Transaction.Code;
                                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                            transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                            transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                            transaction.TransactionType = 2;
                                            transaction.Price = cardInfo.Price;
                                            transaction.ToUser = cardInfo.PhoneNumber;
                                            transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                            transaction.Description = content;
                                            if (transaction.ServiceCode.Contains("VTEL"))
                                            {
                                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("MOBI"))
                                            {
                                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VNMB"))
                                            {
                                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VINA"))
                                            {
                                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("GTEL"))
                                            {
                                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else
                                            {
                                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                            }
                                            // Update wallet info
                                            if (wallet != null)
                                            {
                                                transaction.CurrentBalance = wallet.Balance;
                                                var realCharge = transaction.Price - transaction.SysDiscount;
                                                transaction.SysChargePrice = realCharge;
                                                wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                                ViewBag.BALANCE = wallet.Balance;
                                                ViewBag.COMISSION = wallet.Commission;
                                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                                _unitOfWork.SaveChanges();
                                            }
                                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                            _unitOfWork.SaveChanges();
                                            TempData["TRANSACTION_WAITING"] = transaction;

                                        }
                                        catch (Exception)
                                        {
                                            // Retry to get transaction detail
                                            var transaction = new SuTransaction();
                                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                            transaction.Created_By = User.Identity.Name;
                                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                                            transaction.Status = 2;
                                            transaction.Amout = cardInfo.Amount;
                                            transaction.BonusClient = bonus;
                                            //transaction.Code = ecoResponse.Data.Transaction.Code;
                                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                            transaction.TransactionType = 2;
                                            transaction.Price = cardInfo.Price;
                                            transaction.ToUser = cardInfo.PhoneNumber;
                                            transaction.Description = content;
                                            if (transaction.ServiceCode.Contains("VTEL"))
                                            {
                                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("MOBI"))
                                            {
                                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VNMB"))
                                            {
                                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("VINA"))
                                            {
                                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else if (transaction.ServiceCode.Contains("GTEL"))
                                            {
                                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                            }
                                            else
                                            {
                                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                            }
                                            // Update wallet info
                                            if (wallet != null)
                                            {
                                                transaction.CurrentBalance = wallet.Balance;
                                                var realCharge = transaction.Price - transaction.SysDiscount;
                                                transaction.SysChargePrice = realCharge;
                                                wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                                ViewBag.BALANCE = wallet.Balance;
                                                ViewBag.COMISSION = wallet.Commission;
                                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                                _unitOfWork.SaveChanges();
                                            }
                                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                            _unitOfWork.SaveChanges();
                                            TempData["TRANSACTION_WAITING"] = transaction;
                                        }
                                        #endregion
                                        break;
                                    }
                                case 3:
                                    {
                                        #region Status 3
                                        bResult = 3;
                                        var transaction = new SuTransaction();
                                        transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                        transaction.Created_By = User.Identity.Name;
                                        transaction.ServiceCode = cardInfo.ServiceCode[0];
                                        transaction.Status = ecoResponse.Data.Transaction.Status;
                                        transaction.Amout = cardInfo.Amount;
                                        transaction.BonusClient = bonus;
                                        //transaction.Code = ecoResponse.Data.Transaction.Code;
                                        transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                        transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                        transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                        transaction.TransactionType = 2;
                                        transaction.Price = cardInfo.Price;
                                        transaction.ToUser = cardInfo.PhoneNumber;
                                        transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                        transaction.Description = content;
                                        if (transaction.ServiceCode.Contains("VTEL"))
                                        {
                                            var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                            transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * transaction.Price;
                                        }
                                        else if (transaction.ServiceCode.Contains("MOBI"))
                                        {
                                            var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                            transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * transaction.Price;
                                        }
                                        else if (transaction.ServiceCode.Contains("VNMB"))
                                        {
                                            var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                            transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * transaction.Price;
                                        }
                                        else if (transaction.ServiceCode.Contains("VINA"))
                                        {
                                            var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                            transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * transaction.Price;
                                        }
                                        else if (transaction.ServiceCode.Contains("GTEL"))
                                        {
                                            var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                            transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * transaction.Price;
                                        }
                                        else
                                        {
                                            transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * transaction.Price;
                                        }
                                        // Update wallet info
                                        if (wallet != null)
                                        {
                                            transaction.CurrentBalance = wallet.Balance;
                                            var realCharge = transaction.Price - transaction.SysDiscount;
                                            transaction.SysChargePrice = realCharge;
                                            wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                            ViewBag.BALANCE = wallet.Balance;
                                            ViewBag.COMISSION = wallet.Commission;
                                            _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                            _unitOfWork.SaveChanges();
                                        }
                                        _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                        _unitOfWork.SaveChanges();
                                        #endregion
                                        break;
                                    }
                                default: { break; }
                            }
                        }
                        else
                        {
                            #region Status 1,2
                            bResult = 2;
                            strMess = "Giao dịch đang được xác nhận.Quý khách vui lòng đợi trong ít phút.";
                            TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                            try
                            {
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.Created_By = User.Identity.Name;
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Status = 1;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                //transaction.Code = ecoResponse.Data.Transaction.Code;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                //transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                //transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                transaction.TransactionType = 2;
                                transaction.Price = cardInfo.Price;
                                transaction.ToUser = cardInfo.PhoneNumber;
                                //transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                transaction.Description = content;
                                if (transaction.ServiceCode.Contains("VTEL"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("MOBI"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VNMB"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VINA"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                }
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = transaction.Price - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    _unitOfWork.SaveChanges();
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                TempData["TRANSACTION_WAITING"] = transaction;

                            }
                            catch (Exception)
                            {
                                // Retry to get transaction detail
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.Created_By = User.Identity.Name;
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Status = 1;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                //transaction.Code = ecoResponse.Data.Transaction.Code;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                transaction.TransactionType = 2;
                                transaction.Price = cardInfo.Price;
                                transaction.ToUser = cardInfo.PhoneNumber;
                                transaction.Description = content;
                                if (transaction.ServiceCode.Contains("VTEL"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("MOBI"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VNMB"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VINA"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                }
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = transaction.Price - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    _unitOfWork.SaveChanges();
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                TempData["TRANSACTION_WAITING"] = transaction;
                            }
                            #endregion
                        }
                        #endregion
                    }
                    catch (Exception)
                    {
                        if (ecoResponse != null)
                        {
                            #region Have response
                            if (ecoResponse.Code == EcoPayResponseCode.ERROR_SYSTEM_TIMEOUT
                                || ecoResponse.Code == EcoPayResponseCode.ERROR_SYSTEM_TRANSACTION_EXITS
                                || ecoResponse.Code == EcoPayResponseCode.ERROR_SYSTEM
                                || ecoResponse.Code == EcoPayResponseCode.ERROR_SYSTEM_PENDING)
                            {
                                #region Status 1,2
                                bResult = 2;
                                strMess = "Giao dịch đang được xác nhận.Quý khách vui lòng đợi trong ít phút.";
                                TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                                try
                                {
                                    var transaction = new SuTransaction();
                                    transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                    transaction.Created_By = User.Identity.Name;
                                    transaction.ServiceCode = cardInfo.ServiceCode[0];
                                    transaction.Status = ecoResponse.Data.Transaction.Status;
                                    transaction.Amout = cardInfo.Amount;
                                    transaction.BonusClient = bonus;
                                    //transaction.Code = ecoResponse.Data.Transaction.Code;
                                    transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                    transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                    transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                    transaction.TransactionType = 2;
                                    transaction.Price = cardInfo.Price;
                                    transaction.ToUser = cardInfo.PhoneNumber;
                                    transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                    transaction.Description = content;
                                    if (transaction.ServiceCode.Contains("VTEL"))
                                    {
                                        var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("MOBI"))
                                    {
                                        var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("VNMB"))
                                    {
                                        var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("VINA"))
                                    {
                                        var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("GTEL"))
                                    {
                                        var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else
                                    {
                                        transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                    }
                                    // Update wallet info
                                    if (wallet != null)
                                    {
                                        transaction.CurrentBalance = wallet.Balance;
                                        var realCharge = transaction.Price - transaction.SysDiscount;
                                        transaction.SysChargePrice = realCharge;
                                        wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                        ViewBag.BALANCE = wallet.Balance;
                                        ViewBag.COMISSION = wallet.Commission;
                                        _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                        _unitOfWork.SaveChanges();
                                    }
                                    _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                    _unitOfWork.SaveChanges();
                                    TempData["TRANSACTION_WAITING"] = transaction;

                                }
                                catch (Exception)
                                {
                                    TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                                    // Retry to get transaction detail
                                    var transaction = new SuTransaction();
                                    transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                    transaction.Created_By = User.Identity.Name;
                                    transaction.ServiceCode = cardInfo.ServiceCode[0];
                                    transaction.Status = 1;
                                    transaction.Amout = cardInfo.Amount;
                                    transaction.BonusClient = bonus;
                                    //transaction.Code = ecoResponse.Data.Transaction.Code;
                                    transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                    transaction.TransactionType = 2;
                                    transaction.Price = cardInfo.Price;
                                    transaction.ToUser = cardInfo.PhoneNumber;
                                    transaction.Description = content;
                                    if (transaction.ServiceCode.Contains("VTEL"))
                                    {
                                        var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("MOBI"))
                                    {
                                        var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("VNMB"))
                                    {
                                        var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("VINA"))
                                    {
                                        var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else if (transaction.ServiceCode.Contains("GTEL"))
                                    {
                                        var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                        transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                    }
                                    else
                                    {
                                        transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                    }
                                    // Update wallet info
                                    if (wallet != null)
                                    {
                                        transaction.CurrentBalance = wallet.Balance;
                                        var realCharge = transaction.Price - transaction.SysDiscount;
                                        transaction.SysChargePrice = realCharge;
                                        wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                        ViewBag.BALANCE = wallet.Balance;
                                        ViewBag.COMISSION = wallet.Commission;
                                        _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                        _unitOfWork.SaveChanges();
                                    }
                                    _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                    _unitOfWork.SaveChanges();
                                    TempData["TRANSACTION_WAITING"] = transaction;
                                }
                                #endregion
                            }
                            else
                            {
                                #region Status 0
                                bResult = 0;
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.Created_By = User.Identity.Name;
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Status = 0;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                //transaction.Code = ecoResponse.Data.Transaction.Code;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                transaction.TransactionType = 2;
                                transaction.Price = cardInfo.Price;
                                transaction.ToUser = cardInfo.PhoneNumber;
                                transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                transaction.Description = content;
                                strMess = "Lỗi xảy ra khi thực hiện được giao dịch, vui lòng quay lại sau.";
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region Status 1,2
                            bResult = 2;
                            strMess = "Giao dịch đang được xác nhận.Quý khách vui lòng đợi trong ít phút.";
                            TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                            try
                            {
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.Created_By = User.Identity.Name;
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Status = 2;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                //transaction.Code = ecoResponse.Data.Transaction.Code;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                //transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                                //transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                                transaction.TransactionType = 2;
                                transaction.Price = cardInfo.Price;
                                transaction.ToUser = cardInfo.PhoneNumber;
                                //transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                                transaction.Description = "Không có phản hồi từ Ecopay";
                                if (transaction.ServiceCode.Contains("VTEL"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("MOBI"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VNMB"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VINA"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                }
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = transaction.Price - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    _unitOfWork.SaveChanges();
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                TempData["TRANSACTION_WAITING"] = transaction;

                            }
                            catch (Exception ex)
                            {
                                TempData["RECEIPT_NUMBER"] = cardInfo.ReceiptNumber;
                                // Retry to get transaction detail
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.Created_By = User.Identity.Name;
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Status = 1;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                //transaction.Code = ecoResponse.Data.Transaction.Code;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                transaction.TransactionType = 2;
                                transaction.Price = cardInfo.Price;
                                transaction.ToUser = cardInfo.PhoneNumber;
                                transaction.Description = "Không có phản hồi Ecopay: " + ex.InnerException.Message;
                                if (transaction.ServiceCode.Contains("VTEL"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("MOBI"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VNMB"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("VINA"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * cardInfo.Price;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * cardInfo.Price;
                                }
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = transaction.Price - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    _unitOfWork.SaveChanges();
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                TempData["TRANSACTION_WAITING"] = transaction;
                            }
                            #endregion
                        }

                    }
                TempData["ErrorMessage"] = strMess;
                if (TempData["RECEIPT_NUMBER"] != null)
                {
                    return RedirectToAction("WatingConfirm", "Pay");
                }
                else if (bResult == 3)
                {
                    return RedirectToAction("Success", "Pay");
                }
                else
                {
                    return RedirectToAction("NapThe", "Pay");
                }
            }
            catch (Exception ex)
            {
                SaveTransaction(cardInfo, -3, "Lỗi dữ liệu:" + ex.InnerException.Message);
                TempData["ErrorMessage"] = "Lỗi dữ liệu xảy ra khi thực hiện được giao dịch, vui lòng quay lại sau.";
                return RedirectToAction("NapThe", "Pay");
            }
        }
        public JsonResult GetTransactionDetail(string receiptNumber)
        {
            try
            {
                var ecoResponse = EcoPayServices.GetTransactionDetail(receiptNumber);
                if (ecoResponse != null)
                {
                    if (ecoResponse.Data != null && ecoResponse.Data.Transaction != null)
                    {
                        if (ecoResponse.Data.Transaction.Status == 3)
                        {
                            if (TempData["TRANSACTION_WAITING"] != null)
                            {
                                var trans = (SuTransaction)TempData["TRANSACTION_WAITING"];
                                trans.Status = 3;
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Update(trans);
                                _unitOfWork.SaveChanges();
                            }
                            return Json("Done", JsonRequestBehavior.AllowGet);
                        }
                        else if (ecoResponse.Data.Transaction.Status == 0)
                        {
                            // Refund money
                            if (TempData["TRANSACTION_WAITING"] != null)
                            {
                                var trans = (SuTransaction)TempData["TRANSACTION_WAITING"];
                                trans.Status = 0;
                                var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));

                                if (wallet != null)
                                {
                                    wallet.Balance = wallet.Balance + trans.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    _unitOfWork.SaveChanges();
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Update(trans);
                                _unitOfWork.SaveChanges();
                            }
                            return Json("Done", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json("", JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(ex.ToString(), JsonRequestBehavior.AllowGet);
            }

        }
        private void SaveTransaction(CardModelBinding cardInfo, int status, string message)
        {
            var transaction = new SuTransaction();
            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
            transaction.Created_By = User.Identity.Name;
            transaction.ServiceCode = cardInfo.ServiceCode[0];
            transaction.Amout = cardInfo.Amount;
            //transaction.BonusClient = bonus;
            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
            transaction.TransactionType = 2;
            transaction.Price = cardInfo.Price;
            transaction.ToUser = cardInfo.PhoneNumber;
            transaction.Description = message;
            transaction.Status = -3;   // Response Null
            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
            _unitOfWork.SaveChanges();
        }
        /// <summary>
        /// Buy card function
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BuyCardNumber(FormCollection form)
        {
            try
            {
                double subtractValue = 0;
                var serviceCode = form["hf-ddlCardCode"] != null ? form["hf-ddlCardCode"].ToString() : string.Empty;
                var network = form["hf-network"] != null ? form["hf-network"].ToString() : string.Empty;
                var agency = form["hf-ddlagency"] != null ? form["hf-ddlagency"].ToString() : string.Empty;
                var amount = form["txtquantity"] != null ? form["txtquantity"].ToString() : string.Empty;
                var content = form["txtnoidungnapthe"] != null ? form["txtnoidungnapthe"].ToString() : string.Empty;
                Session["SERVICECODE"] = serviceCode.Substring(0, 2);
                int discount = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_ALL")).SValue.ToIntOrZero();
                var lstDataPrice = GetListCode(network);
                var bonus = form["txtBonus"] != null ? form["txtBonus"].ToString().ToIntOrZero() : 0;
                var price = string.Empty;
                foreach (var item in lstDataPrice)
                {
                    if (item.Code.Equals(serviceCode))
                    {
                        price = item.Value;
                        break;
                    }
                }
                CardModelBinding cardInfo = new CardModelBinding();
                cardInfo.ReceiptNumber = Guid.NewGuid().ToString();
                cardInfo.ServiceCode = new List<string> { serviceCode };
                cardInfo.Price = float.Parse(price);
                cardInfo.Amount = int.Parse(amount);
                try
                {
                    var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (wallet.Balance < cardInfo.Price * cardInfo.Amount)
                    {
                        TempData["ErrorMessage"] = "Số dư của bạn không đủ để thực hiện giao dịch, vui lòng nạp thêm tài khoản.";
                        return RedirectToAction("MuaThe", "Pay");
                    }
                    cardInfo.ReceiptNumber = Guid.NewGuid().ToString();
                    var ecoResponse = EcoPayServices.BuyCard(cardInfo.ReceiptNumber, cardInfo.ServiceCode[0], cardInfo.Price * cardInfo.Amount, cardInfo.Amount);
                    if (ecoResponse != null)
                    {
                        if (ecoResponse.Code == EcoPayResponseCode.SUCCESS_CODE)
                        {
                            #region Success Code
                            var transaction = new SuTransaction();
                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                            transaction.Created_By = User.Identity.Name;
                            transaction.Amout = cardInfo.Amount;
                            transaction.BonusClient = bonus;
                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                            try
                            {
                                if (ecoResponse.Data.Transaction.Info != null)
                                {
                                    dynamic dataInfo = JsonConvert.DeserializeObject(ecoResponse.Data.Transaction.Info);
                                    if (dataInfo != null)
                                    {
                                        int count = Enumerable.Count(dataInfo);
                                        string strSerial = string.Empty;
                                        string strCode = string.Empty;
                                        for (int i = 0; i < count; i++)
                                        {

                                            if (i == 0)
                                            {
                                                strSerial = dataInfo[i]["Serial"].ToString();
                                                strCode = TripleDes.Decrypt(EcoPayKey.Scret_Key, dataInfo[i]["Code"].Value);
                                            }
                                            else
                                            {
                                                strSerial = strSerial + "; " + dataInfo[i]["Serial"].ToString();
                                                strCode = strCode + "; " + TripleDes.Decrypt(EcoPayKey.Scret_Key, dataInfo[i]["Code"].Value);
                                            }
                                        }
                                        transaction.Serial = strSerial;
                                        transaction.Code = strCode;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }

                            transaction.Info = ecoResponse.Data.Transaction.Info;
                            transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                            transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                            transaction.TransactionType = 1;
                            //transaction.Sign = 
                            transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                            subtractValue = transaction.Amout.Value * cardInfo.Price;
                            if (transaction.ServiceCode.Contains("VT"))
                            {
                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VIETELL")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("MB"))
                            {
                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_MOBI")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VNM"))
                            {
                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VNM")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VNG"))
                            {
                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VN"))
                            {
                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VINA")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("GTEL"))
                            {
                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_GTEL")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else
                            {
                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                            }
                            transaction.Description = content;
                            transaction.Price = subtractValue;
                            transaction.Status = ecoResponse.Data.Transaction.Status;

                            if (ecoResponse.Data.Transaction.Status != 0)
                            {
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = subtractValue - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                }
                            }
                            else
                            {
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                Session["ID"] = transaction.Id;
                                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                                return RedirectToAction("MuaThe", "Pay");
                            }
                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                            _unitOfWork.SaveChanges();
                            Session["ID"] = transaction.Id;
                            return RedirectToAction("SuccessStatusPayment", "Pay");
                            #endregion
                        }
                        else if (ecoResponse.Code == EcoPayResponseCode.ERROR_SYSTEM_TRANSACTION_EXITS)
                        {
                            var ecoResponseDetail = EcoPayServices.GetTransactionDetail(cardInfo.ReceiptNumber);
                            if (ecoResponseDetail.Code == 0)
                            {
                                #region Insert GD
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Created_By = User.Identity.Name;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                //string jsonIno = "{}"
                                transaction.Info = ecoResponseDetail.Data.Transaction.Info;
                                transaction.ServiceID = ecoResponseDetail.Data.Transaction.ServiceID;
                                transaction.Proccessed = ecoResponseDetail.Data.Transaction.Proccessed;
                                transaction.TransactionType = 1;
                                //transaction.Sign = 
                                transaction.TransactionId = ecoResponseDetail.Data.Transaction.TransactionID;
                                subtractValue = transaction.Amout.Value * cardInfo.Price;
                                if (transaction.ServiceCode.Contains("VT"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("MB"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VNM"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VNG"))
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VN"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                                }
                                //transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                                transaction.Description = content;
                                transaction.Price = subtractValue;
                                transaction.Status = 3;
                                // Update wallet info
                                if (ecoResponseDetail.Data.Transaction.Status != 0)
                                {
                                    // Update wallet info
                                    if (wallet != null)
                                    {
                                        transaction.CurrentBalance = wallet.Balance;
                                        var realCharge = subtractValue - transaction.SysDiscount;
                                        transaction.SysChargePrice = realCharge;
                                        wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                        ViewBag.BALANCE = wallet.Balance;
                                        ViewBag.COMISSION = wallet.Commission;
                                        _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                    }
                                }
                                else
                                {
                                    _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                    _unitOfWork.SaveChanges();
                                    Session["ID"] = transaction.Id;
                                    TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                                    return RedirectToAction("MuaThe", "Pay");
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                Session["ID"] = transaction.Id;
                                return RedirectToAction("SuccessStatusPayment", "Pay");
                                #endregion
                            }
                            else
                            {
                                #region Insert GD
                                var transaction = new SuTransaction();
                                transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                                transaction.ServiceCode = cardInfo.ServiceCode[0];
                                transaction.Created_By = User.Identity.Name;
                                transaction.Amout = cardInfo.Amount;
                                transaction.BonusClient = bonus;
                                transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                                transaction.TransactionType = 1;
                                //transaction.Sign = 
                                subtractValue = transaction.Amout.Value * cardInfo.Price;
                                if (transaction.ServiceCode.Contains("VT"))
                                {
                                    var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VIETELL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("MB"))
                                {
                                    var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_MOBI")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VNM"))
                                {
                                    var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VNM")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VNG"))
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("VN"))
                                {
                                    var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VINA")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else if (transaction.ServiceCode.Contains("GTEL"))
                                {
                                    var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_GTEL")).SValue.ToIntOrZero();
                                    transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * subtractValue;
                                }
                                else
                                {
                                    transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                                }
                                transaction.Price = subtractValue;
                                transaction.Status = 1;
                                // Update wallet info
                                if (wallet != null)
                                {
                                    transaction.CurrentBalance = wallet.Balance;
                                    var realCharge = subtractValue - transaction.SysDiscount;
                                    transaction.SysChargePrice = realCharge;
                                    wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                    ViewBag.BALANCE = wallet.Balance;
                                    ViewBag.COMISSION = wallet.Commission;
                                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                }
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                Session["ID"] = transaction.Id;
                                return RedirectToAction("SuccessStatusPayment", "Pay");
                                #endregion
                            }
                        }
                        else
                        {
                            #region Insert GD
                            var transaction = new SuTransaction();
                            transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                            transaction.ServiceCode = cardInfo.ServiceCode[0];
                            transaction.Created_By = User.Identity.Name;
                            transaction.Amout = cardInfo.Amount;
                            transaction.BonusClient = bonus;
                            transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                            transaction.TransactionType = 1;
                            //transaction.Sign = 
                            subtractValue = transaction.Amout.Value * cardInfo.Price;
                            if (transaction.ServiceCode.Contains("VT"))
                            {
                                var discountVT = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VIETELL")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVT > 0 ? (discountVT / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("MB"))
                            {
                                var discountMB = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_MOBI")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountMB > 0 ? (discountMB / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VNM"))
                            {
                                var discountVNM = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VNM")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVNM > 0 ? (discountVNM / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VNG"))
                            {
                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("VN"))
                            {
                                var discountVN = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_VINA")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountVN > 0 ? (discountVN / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else if (transaction.ServiceCode.Contains("GTEL"))
                            {
                                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_DISCOUNT_BUY_GTEL")).SValue.ToIntOrZero();
                                transaction.SysDiscount = (discountGTEL > 0 ? (discountGTEL / 100.0) : (discount / 100.0)) * subtractValue;
                            }
                            else
                            {
                                transaction.SysDiscount = (discount > 0 ? (discount / 100.0) : 0) * subtractValue;
                            }
                            transaction.Price = subtractValue;
                            transaction.Status = -3;
                            // Update wallet info
                            if (wallet != null)
                            {
                                transaction.CurrentBalance = wallet.Balance;
                                var realCharge = subtractValue - transaction.SysDiscount;
                                transaction.SysChargePrice = realCharge;
                                wallet.Balance = wallet.Balance - transaction.SysChargePrice;
                                ViewBag.BALANCE = wallet.Balance;
                                ViewBag.COMISSION = wallet.Commission;
                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                            }
                            _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                            _unitOfWork.SaveChanges();
                            #endregion
                            TempData["ErrorMessage"] = "Hệ thống đang nâng cấp, quý khách vui lòng quay lại sau!";
                            return RedirectToAction("MuaThe", "Pay");
                        }
                    }
                    else
                    {
                        SaveTransaction(cardInfo, -5, "response null");
                        TempData["ErrorMessage"] = "Hệ thống đang nâng cấp, quý khách vui lòng quay lại sau!";
                        return RedirectToAction("MuaThe", "Pay");
                    }
                }
                catch (Exception ex)
                {
                    SaveTransaction(cardInfo, -4, ex.Message);
                    TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                    return RedirectToAction("MuaThe", "Pay");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                return RedirectToAction("MuaThe", "Pay");
            }
        }

        [HttpPost]
        public ActionResult TransferMoney(FormCollection form)
        {
            var phone = form["txtphone"] != null ? form["txtphone"].ToString() : string.Empty;
            var price = form["txtmonney"] != null ? form["txtmonney"].ToString() : string.Empty;
            var content = form["txtnoidungchuyentien"] != null ? form["txtnoidungchuyentien"].ToString() : string.Empty;
            SharedBudgetInfo sharedInfo = new SharedBudgetInfo() { PhoneNumber = phone, Price = float.Parse(price), Content = content };

            //Session["SharedBudgetInfo"] = obj;
            //var userInfo = _unitOfWork.GetRepositoryInstance<AspNetUser>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
            //string privateKey = Constant.PrivateKeyForAnonymous;
            //if (userInfo != null)
            //{
            //    privateKey = userInfo.PrivateKey;
            //}
            //byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(privateKey);
            //Session["PrivateKey"] = privateKey;
            //var topt = new Totp(secretKey, step: 60);
            //   if (Common.CommonUtils.SendSms(userInfo.UserName, topt.ComputeTotp()) == 100)
            //{
            //    return RedirectToAction("ConfirmTransferMoney");
            //}
            //  else
            //{
            //    return View();
            //}
            try
            {
                var toUser = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(sharedInfo.PhoneNumber));
                if (toUser != null)
                {
                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(toUser);
                    var transactionShared = new SuTransaction();
                    transactionShared.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                    transactionShared.Created_By = User.Identity.Name;
                    transactionShared.Status = Constant.ACTIVE;
                    transactionShared.Description = sharedInfo.Content;
                    transactionShared.Price = sharedInfo.Price;
                    transactionShared.TransactionType = 3;
                    transactionShared.ToUser = sharedInfo.PhoneNumber;
                    transactionShared.BonusClient = 0;
                    // Update wallet info
                    var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (wallet != null && wallet.Balance >= sharedInfo.Price)
                    {
                        toUser.Balance = toUser.Balance + sharedInfo.Price;
                        transactionShared.CurrentBalance = wallet.Balance;
                        wallet.Balance = wallet.Balance - sharedInfo.Price;
                        ViewBag.BALANCE = wallet.Balance;
                        ViewBag.COMISSION = wallet.Commission;
                        _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                        _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transactionShared);
                        _unitOfWork.SaveChanges();
                        return RedirectToAction("Success", "Pay");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Số dư không đủ để thực hiện giao dịch này";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Người nhận không tồn tại trong hệ thống.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
            }
            return RedirectToAction("ChuyenTien", "Pay");
        }
        public ActionResult ConfirmPayment(int type)
        {
            ViewBag.Type = type;
            if (Session["ToupCardInfo"] == null && Session["CardBuyInfo"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }
        public ActionResult ConfirmTransferMoney()
        {
            return View();
        }
        public ActionResult SuccessStatusPayment()
        {
            SuTransaction transaction = new SuTransaction();
            var id = Session["ID"] != null ? int.Parse(Session["ID"].ToString()) : 0;
            //var id = 639;
            transaction = _unitOfWork.GetRepositoryInstance<SuTransaction>().GetFirstOrDefault(id);
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
        public ActionResult ChuyenTienLienNganHangThanhCong()
        {

            var receiptID = Session["ReceiptID_ChuyentienLienNganHang"];
            var suBankTransferHistory = _unitOfWork.GetRepositoryInstance<SuBankTransferHistory>().GetFirstOrDefaultByParameter(x => x.ReceiptID.ToString() == receiptID.ToString());
            return View(suBankTransferHistory);
        }
        public ActionResult SuccessPayment()
        {
            SuInstallmentPayment mentPayment = new SuInstallmentPayment();
            var id = Session["ID_TRA_GOP"] != null ? int.Parse(Session["ID_TRA_GOP"].ToString()) : 0;
            mentPayment = _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().GetFirstOrDefault(id);
            return View(mentPayment);
        }
        public ActionResult Success()
        {
            return View();
        }
        public ActionResult WatingConfirm(SuTransaction trans)
        {
            return View(trans);
        }
        [HttpPost]
        public JsonResult SubmitTransferMoney(string otp)
        {
            try
            {
                var sharedInfo = (SharedBudgetInfo)Session["SharedBudgetInfo"];
                if (sharedInfo == null || string.IsNullOrEmpty(otp))
                {
                    return Json("Lỗi dữ liệu khi thực hiện giao dịch, vui lòng thực hiện lại.");
                }
                if (Session["PrivateKey"] == null)
                {
                    var userInfo = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (userInfo == null)
                    {
                        return Json("Vui lòng đăng nhập để thực hiện giao dịch.");
                    }
                    else
                    {
                        Session["PrivateKey"] = userInfo.PrivateKey;
                    }
                }
                byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(Session["PrivateKey"].ToString());
                var topt = new Totp(secretKey, step: 60);
                long timeWindowUsed;
                bool isMatch = topt.VerifyTotp(otp, out timeWindowUsed, new VerificationWindow(1, 1));
                if (isMatch)
                {
                    var toUser = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(sharedInfo.PhoneNumber));
                    if (toUser != null)
                    {
                        toUser.Balance = toUser.Balance + sharedInfo.Price;
                        _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(toUser);
                        var transactionShared = new SuTransaction();
                        transactionShared.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                        transactionShared.Created_By = User.Identity.Name;
                        transactionShared.Status = Constant.ACTIVE;
                        transactionShared.Description = sharedInfo.Content;
                        transactionShared.Price = sharedInfo.Price;
                        transactionShared.TransactionType = 3;
                        transactionShared.ToUser = sharedInfo.PhoneNumber;
                        // Update wallet info
                        var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                        if (wallet != null)
                        {
                            transactionShared.CurrentBalance = wallet.Balance;
                            wallet.Balance = wallet.Balance - sharedInfo.Price;
                            _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                        }
                        _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transactionShared);
                        _unitOfWork.SaveChanges();
                        return Json("Successfully");
                    }
                    else
                    {
                        return Json("Người nhận không tồn tại trong hệ thống.");
                    }
                }
                else
                {
                    // OTP incorrect
                    return Json("Mã xác thực OTP không đúng,vui lòng nhận lại mã OTP để thực hiện giao dịch.");
                }
            }
            catch (Exception ex)
            {
                return Json("Lỗi xảy ra khi thực hiện giao dịch,vui lòng thực hiện lại giao dịch hoặc quay lại sau.");
            }
        }

        [HttpPost]
        public JsonResult SubmitPayment(string otpCode, int? type)
        {
            try
            {
                if (string.IsNullOrEmpty(otpCode) || type == null)
                {
                    return Json("Lỗi dữ liệu khi thực hiện giao dịch, vui lòng thực hiện lại.");
                }
                CardModelBinding cardInfo = new CardModelBinding();
                string description = string.Empty;
                double subtractValue = 0;
                if (Session["PrivateKey"] == null)
                {
                    var userInfo = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (userInfo == null)
                    {
                        return Json("Vui lòng đăng nhập để thực hiện giao dịch.");
                    }
                    else
                    {
                        Session["PrivateKey"] = userInfo.PrivateKey;
                    }
                }
                byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(Session["PrivateKey"].ToString());
                var topt = new Totp(secretKey, step: 60);
                long timeWindowUsed;
                bool isMatch = topt.VerifyTotp(otpCode, out timeWindowUsed, new VerificationWindow(1, 1));
                if (isMatch)
                {
                    if (type == 1)
                    {
                        cardInfo = (CardModelBinding)Session["ToupCardInfo"];
                        description = TempData["CardBuyContent"] != null ? TempData["CardBuyContent"].ToString() : string.Empty;
                    }
                    else if (type == 2)
                    {
                        cardInfo = (CardModelBinding)Session["CardBuyInfo"];
                        description = TempData["ToupContent"] != null ? TempData["ToupContent"].ToString() : string.Empty;
                    }
                    cardInfo.ReceiptNumber = Guid.NewGuid().ToString();
                    var ecoResponse = type == 1 ? EcoPayServices.TopUpCard(cardInfo.ReceiptNumber, cardInfo.ServiceCode[0], cardInfo.PhoneNumber, cardInfo.Price) : EcoPayServices.BuyCard(cardInfo.ReceiptNumber, cardInfo.ServiceCode[0], cardInfo.Price, cardInfo.Amount);
                    if (ecoResponse != null && ecoResponse.Code == 0)
                    {
                        var transaction = new SuTransaction();
                        transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                        transaction.Created_By = User.Identity.Name;
                        transaction.Status = Constant.ACTIVE;
                        transaction.Amout = cardInfo.Amount;
                        //transaction.Code = ecoResponse.Data.Transaction.Code;
                        transaction.ReceiptNumber = cardInfo.ReceiptNumber;
                        dynamic dataInfo = JsonConvert.DeserializeObject(ecoResponse.Data.Transaction.Info);
                        if (dataInfo != null)
                        {
                            transaction.Serial = dataInfo[0]["Serial"].ToString();
                        }
                        transaction.ServiceID = ecoResponse.Data.Transaction.ServiceID;
                        transaction.Proccessed = ecoResponse.Data.Transaction.Proccessed;
                        transaction.TransactionType = ecoResponse.Data.Transaction.TransactionType;
                        //transaction.Sign = 
                        transaction.Price = cardInfo.Price;

                        transaction.TransactionId = ecoResponse.Data.Transaction.TransactionID;
                        transaction.Description = description;
                        subtractValue = transaction.Amout.Value * transaction.Price.Value;

                        // Update wallet info
                        var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                        if (wallet != null)
                        {
                            transaction.CurrentBalance = wallet.Balance;
                            wallet.Balance = wallet.Balance - subtractValue;
                            _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                        }
                        _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                        _unitOfWork.SaveChanges();
                        return Json("Successfully", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // OTP incorrect
                    return Json("Mã xác thực OTP không đúng,vui lòng nhận lại mã OTP để thực hiện giao dịch.", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json("Lỗi không thể thực hiện được giao dịch,vui lòng quay lại sau.");
            }

        }
        [HttpPost]
        public JsonResult SendSMS()
        {
            try
            {
                var userInfo = _unitOfWork.GetRepositoryInstance<AspNetUsers>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                string privateKey = Constant.PrivateKeyForAnonymous;
                if (userInfo != null)
                {
                    privateKey = userInfo.PrivateKey;
                }
                byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(privateKey);
                Session["PrivateKey"] = privateKey;
                var topt = new Totp(secretKey, step: 60);
                Common.CommonUtils.SendSms(userInfo.UserName, topt.ComputeTotp());
                return Json("Successfully", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("Lỗi không thể nhận mã xác thực OTP,vui lòng quay lại sau.", JsonRequestBehavior.AllowGet);
            }

        }
        [HttpGet]
        public JsonResult LoadServiceCodeByType(string type)
        {
            string strCode = "";
            string pSpecifier = "N0";
            List<ServiceCode> listServiceCode = GetListCode(type);
            foreach (var item in listServiceCode)
            {
                strCode += "<button type = \"button\" style=\"font-size:14px !important\" onClick =\"ClickServiceCode('" + item.Code + "')\" data-value=\"" + item.Value + "\" class =\"btn btn-default col-md-3 btn-primary-pay\" id=\"" + item.Code + "\" value=\"" + item.Code + "\">" + float.Parse(item.Value).ToString(pSpecifier) + "&nbsp;VNĐ</button>";
            }
            return Json(strCode, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult LoadCardByType(string type)
        {
            string strCode = "";
            List<ServiceCode> listServiceCode = GetListCode(type);
            strCode += "<select class=\"form-control\" id=\"ddlCardCode\" name=\"ddlCardCode\">";
            strCode += "<option value=\"0\">------ Chọn ------</option>";
            foreach (var item in listServiceCode)
            {
                strCode += "<option value=\"" + item.Code + "\">" + item.Value + "&nbsp;VNĐ</option>";
            }
            strCode += "</select>";

            return Json(strCode, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetFullNameByPhone(string phone)
        {
            var userInfo = UserManager.FindByName(phone);
            if (userInfo != null)
            {
                return Json(userInfo.UsersInfo.FullName != null ? userInfo.UsersInfo.FullName : string.Empty, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            }
        }
        public List<ServiceCode> GetListCode(string type)
        {
            List<ServiceCode> lstCode = new List<ServiceCode>();
            switch (type)
            {
                case "VT":
                    List<string> lstCodeVT = new List<string>() { "VT10", "VT20", "VT30", "VT50", "VT100", "VT200", "VT300", "VT500"/*, "VT1000"*/ };
                    List<string> lstValueVT = new List<string>()
                        {
                            "10000","20000","30000","50000","100000","200000","300000","500000"//,"1000000"
                        };

                    for (int i = 0; i < lstCodeVT.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVT[i], Value = lstValueVT[i] });
                    }
                    break;
                case "MB":
                    List<string> lstCodeMB = new List<string>() { "MB10","MB20",
                        "MB30","MB50","MB100","MB200","300000","MB500"};
                    List<string> lstValueMB = new List<string>()
                        {
                            "10000","20000","30000","50000","100000","200000","300000","500000"
                        };
                    for (int i = 0; i < lstCodeMB.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeMB[i], Value = lstValueMB[i] });
                    }

                    break;
                case "VN":
                    List<string> lstCodeVN = new List<string>() { "VN10", "VN20", "VN30", "VN50", "VN100", "VN200", "VN300", "VN500" };
                    List<string> lstValueVN = new List<string>()
                        {
                            "10000","20000","30000","50000","100000","200000","300000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeVN.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVN[i], Value = lstValueVN[i] });
                    }
                    break;
                case "VNM":
                    List<string> lstCodeVNM = new List<string>() { "VNM10", "VNM20", "VNM50", "VNM100", "VNM200", "VNM300", "VNM500" };
                    List<string> lstValueVNM = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","300000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeVNM.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVNM[i], Value = lstValueVNM[i] });
                    }
                    break;
                case "GATE":
                    List<string> lstCodeGATE = new List<string>() { "GATE10", "GATE20", "GATE50", "GATE100", "GATE200", "GATE300", "GATE500", "GATE1000", "GATE2000", "GATE5000" };
                    List<string> lstValueGATE = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","300000","500000" ,"1000000","2000000","5000000" ,
                        };

                    for (int i = 0; i < lstCodeGATE.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeGATE[i], Value = lstValueGATE[i] });
                    }
                    break;
                case "GARENA":
                    List<string> lstCodeGARENA = new List<string>() { "GARENA10", "GARENA20", "GARENA30", "GARENA50", "GARENA100", "GARENA200", "GARENA300", "GARENA500" };
                    List<string> lstValueGARENA = new List<string>()
                        {
                            "10000","20000","30000","50000","100000","200000","300000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeGARENA.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeGARENA[i], Value = lstValueGARENA[i] });
                    }
                    break;
                case "GTEL":
                    List<string> lstCodeGTEL = new List<string>() { "GTEL10", "GTEL20", "GTEL50", "GTEL100", "GTEL200", "GTEL300", "GTEL500" };
                    List<string> lstValueGTEL = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","300000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeGTEL.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeGTEL[i], Value = lstValueGTEL[i] });
                    }
                    break;
                case "VTC":
                    List<string> lstCodeVTC = new List<string>() { "VTC10", "VTC20", "VTC50", "VTC100", "VTC200", "VTC300", "VTC500" };
                    List<string> lstValueVTC = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","300000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeVTC.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVTC[i], Value = lstValueVTC[i] });
                    }
                    break;
                case "VCARD":
                    List<string> lstCodeVCARD = new List<string>() { "VCARD10", "VCARD20", "VCARD50", "VCARD100", "VCARD200", "VCARD500", "VCARD1000", "VCARD2000" };
                    List<string> lstValueVCARD = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","500000","1000000","2000000"  ,
                        };

                    for (int i = 0; i < lstCodeVCARD.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVCARD[i], Value = lstValueVCARD[i] });
                    }
                    break;
                case "ONCASH":
                    List<string> lstCodeONCASH = new List<string>() { "ONCASH20", "ONCASH50", "ONCASH100", "ONCASH200", "ONCASH500" };
                    List<string> lstValueONCASH = new List<string>()
                        {
                            "20000","50000","100000","200000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeONCASH.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeONCASH[i], Value = lstValueONCASH[i] });
                    }
                    break;
                case "BIT":
                    List<string> lstCodeBIT = new List<string>() { "BIT50", "BIT100", "BIT200", "BIT500", "BIT1000", "BIT2000", "BIT5000" };
                    List<string> lstValueBIT = new List<string>()
                        {
                           "50000","100000","200000","500000"  ,"1000000","2000000","5000000"
                        };

                    for (int i = 0; i < lstCodeBIT.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeBIT[i], Value = lstValueBIT[i] });
                    }
                    break;
                case "MEGA":
                    List<string> lstCodeMEGA = new List<string>() { "MEGA10", "MEGA20", "MEGA50", "MEGA100", "MEGA200", "MEGA300", "MEGA500", "MEGA1000", "MEGA2000", "MEGA3000", "MEGA5000" };
                    List<string> lstValueMEGA = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","300000","500000" ,"1000000" ,"2000000" ,"3000000" , "5000000" ,
                        };

                    for (int i = 0; i < lstCodeMEGA.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeMEGA[i], Value = lstValueMEGA[i] });
                    }
                    break;
                case "VNG":
                    List<string> lstCodeVNG = new List<string>() { "VNG10", "VNG20", "VNG50", "VNG100", "VNG200", "VNG500" };
                    List<string> lstValueVNG = new List<string>()
                        {
                            "10000","20000","50000","100000","200000","500000"  ,
                        };

                    for (int i = 0; i < lstCodeVNG.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeVNG[i], Value = lstValueVNG[i] });
                    }
                    break;
                //case "RIKVIP":
                //    List<string> lstCodeRIK = new List<string>() { "RIKVIP10", "RIKVIP20", "RIKVIP30", "RIKVIP50", "RIKVIP100", "RIKVIP200", "RIKVIP300", "RIKVIP500" };
                //    List<string> lstValueRIK = new List<string>()
                //        {
                //            "10000","20000","30000","50000","100000","200000","300000","500000"  ,
                //        };

                //    for (int i = 0; i < lstCodeRIK.Count; i++)
                //    {
                //        lstCode.Add(new ServiceCode() { Code = lstCodeRIK[i], Value = lstValueRIK[i] });
                //    }
                //    break;
                case "K+":
                    List<string> lstCodeK = new List<string>() { "K125", "K375", "K750", "K1500" };
                    List<string> lstValueK = new List<string>()
                        {
                            "125000","375000","750000","1500000",
                        };

                    for (int i = 0; i < lstCodeK.Count; i++)
                    {
                        lstCode.Add(new ServiceCode() { Code = lstCodeK[i], Value = lstValueK[i] });
                    }
                    break;
            }

            return lstCode;
        }
        public List<ListProvider> GetListNganHang()
        {
            List<ListProvider> lstProvider = new List<ListProvider>();
            var lstSuProvider = _unitOfWork.GetRepositoryInstance<SuProvider>().GetListByParameter(x => x.ProviderType == 2);
            foreach (var item in lstSuProvider)
            {
                lstProvider.Add(new ListProvider() { ProviderName = item.ProviderName, Discount = item.Discount.Value, ProviderIcon = item.ProviderIcon });
            }
            return lstProvider;
        }
        public List<ListProvider> GetListProvider()
        {
            List<ListProvider> lstProvider = new List<ListProvider>();
            var lstSuProvider = _unitOfWork.GetRepositoryInstance<SuProvider>().GetListByParameter(x => x.ProviderType == 0);
            foreach (var item in lstSuProvider)
            {
                lstProvider.Add(new ListProvider() { ProviderName = item.ProviderName, Discount = item.Discount.Value, ProviderIcon = item.ProviderIcon });
            }
            return lstProvider;
        }
        public List<ListProvider> GetListDienNuoc()
        {
            List<ListProvider> lstProvider = new List<ListProvider>();
            var lstSuProvider = _unitOfWork.GetRepositoryInstance<SuProvider>().GetListByParameter(x => x.ProviderType == 1);
            foreach (var item in lstSuProvider)
            {
                lstProvider.Add(new ListProvider() { ProviderName = item.ProviderName, Discount = item.Discount.Value, ProviderIcon = item.ProviderIcon });
            }
            return lstProvider;
        }
        [HttpPost]
        public ActionResult TraGop(FormCollection form)
        {
            try
            {
                double giavon = 0;
                var numberContract = form["txtNumberContract"] != null ? form["txtNumberContract"].ToString() : string.Empty;
                var phone = form["txtPhone"] != null ? form["txtPhone"].ToString() : string.Empty;
                var name = form["txtName"] != null ? form["txtName"].ToString() : string.Empty;
                var money = form["txtMoney"] != null ? form["txtMoney"].ToString() : string.Empty;
                var provider = form["hf-ddlagency"] != null ? form["hf-ddlagency"].ToString() : string.Empty;
                TraGopModels tgInfor = new TraGopModels();
                tgInfor.Name = name;
                tgInfor.Phone = phone;
                tgInfor.NumberContract = numberContract;
                tgInfor.Money = float.Parse(money);
                tgInfor.Provider = provider;
                try
                {
                    var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (wallet == null || wallet.Balance < tgInfor.Money)
                    {
                        TempData["ErrorMessage"] = "Số điện thoại không đúng hoặc Số dư của bạn không đủ để thực hiện giao dịch, vui lòng kiểm tra lại.";
                        return RedirectToAction("TraGop", "Pay");
                    }
                    if (wallet != null)
                    {
                        var transaction = new SuTransaction();
                        transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                        transaction.Created_By = User.Identity.Name;
                        transaction.Status = Constant.PENDING;
                        //string someJson = "{\"NumberContract\": \"" + numberContract + "\",\"Phone\": \"" + phone + "\"}";
                        dynamic dataInfo = JsonConvert.SerializeObject(tgInfor);
                        transaction.Info = dataInfo;
                        transaction.TransactionType = 4;
                        var mentPayment = new SuInstallmentPayment();
                        mentPayment.NumberContract = numberContract;
                        mentPayment.Phone = phone;
                        mentPayment.Provider = provider;
                        mentPayment.Money = float.Parse(money);
                        mentPayment.Name = name;
                        mentPayment.IsView = false;
                        mentPayment.TransactionType = transaction.TransactionType;
                        transaction.TransactionId = mentPayment.Id;
                        var discountProvider = _unitOfWork.GetRepositoryInstance<SuProvider>().GetFirstOrDefaultByParameter(x => x.ProviderName.Equals(provider));
                        if (tgInfor.Money < discountProvider.ProviderMoney)
                        {
                            TempData["ErrorMessage"] = "Số tiền trả góp quá ít!";
                            return RedirectToAction("TraGop", "Pay");
                        }
                        else
                        {
                            if (wallet.Balance > tgInfor.Money)
                            {
                                transaction.CurrentBalance = wallet.Balance;
                                wallet.Balance = wallet.Balance - (tgInfor.Money - tgInfor.Money * (discountProvider.Discount > 0 ? (discountProvider.Discount / 100.0) : 0)) + (discountProvider.ProviderMoney > 0 ? discountProvider.ProviderMoney : 0);
                                ViewBag.BALANCE = wallet.Balance;
                                ViewBag.COMISSION = wallet.Commission;
                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().Add(mentPayment);
                                transaction.TransactionId = mentPayment.Id;
                                transaction.Price = float.Parse(money);
                                transaction.SysDiscount = discountProvider.ProviderMoney > 0 ? discountProvider.ProviderMoney : 0;
                                giavon = transaction.Price.Value - transaction.SysDiscount.Value;
                                transaction.SysChargePrice = giavon;
                                transaction.BonusClient = 0;
                                transaction.Amout = 1;
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                Session["ID_TRA_GOP"] = mentPayment.Id;
                                return RedirectToAction("SuccessPayment", "Pay");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Số dư không đủ để thực hiện giao dịch này";
                                return RedirectToAction("TraGop", "Pay");
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                        return RedirectToAction("TraGop", "Pay");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                    return RedirectToAction("TraGop", "Pay");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                return RedirectToAction("TraGop", "Pay");
            }
        }
        [HttpPost]
        public ActionResult PaymentService(FormCollection form)
        {
            try
            {
                double giavon = 0;
                var money = form["txtMoney"] != null ? form["txtMoney"].ToString() : string.Empty;
                var phone = form["txtPhone"] != null ? form["txtPhone"].ToString() : string.Empty;
                var name = form["txtName"] != null ? form["txtName"].ToString() : string.Empty;
                var numberCustomer = form["numberCustomer"] != null ? form["numberCustomer"].ToString() : string.Empty;
                var service = form["hf-ddlagency"] != null ? form["hf-ddlagency"].ToString() : string.Empty;
                DichVuModels dvInfor = new DichVuModels();
                dvInfor.Name = name;
                dvInfor.Phone = phone;
                dvInfor.NumberCustomer = numberCustomer;
                dvInfor.Services = service;
                dvInfor.Money = float.Parse(money);
                try
                {
                    var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (wallet == null || wallet.Balance < dvInfor.Money)
                    {
                        TempData["ErrorMessage"] = "Số điện thoại không đúng hoặc Số dư của bạn không đủ để thực hiện giao dịch, vui lòng kiểm tra lại";
                        return RedirectToAction("PaymentService", "Pay");
                    }
                    if (wallet != null)
                    {
                        var transaction = new SuTransaction();
                        transaction.Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy");
                        transaction.Created_By = User.Identity.Name;
                        transaction.Status = Constant.PENDING;
                        //string someJson = "{\"NumberContract\": \"" + numberContract + "\",\"Phone\": \"" + phone + "\"}";
                        dynamic dataInfo = JsonConvert.SerializeObject(dvInfor);
                        transaction.Info = dataInfo;
                        transaction.TransactionType = 5;
                        var mentPayment = new SuInstallmentPayment();
                        mentPayment.NumberContract = numberCustomer;
                        mentPayment.Phone = phone;
                        mentPayment.Provider = service;

                        mentPayment.Money = float.Parse(money);
                        mentPayment.Name = name;
                        mentPayment.IsView = false;
                        mentPayment.TransactionType = transaction.TransactionType;
                        var discountProvider = _unitOfWork.GetRepositoryInstance<SuProvider>().GetFirstOrDefaultByParameter(x => x.ProviderName.Equals(service));
                        if (dvInfor.Money < discountProvider.ProviderMoney)
                        {
                            TempData["ErrorMessage"] = "Số tiền trả góp quá ít!";
                            return RedirectToAction("PaymentService", "Pay");
                        }
                        else
                        {
                            if (wallet.Balance > dvInfor.Money)
                            {
                                transaction.CurrentBalance = wallet.Balance;
                                wallet.Balance = wallet.Balance - (dvInfor.Money - dvInfor.Money * (discountProvider.Discount > 0 ? (discountProvider.Discount / 100.0) : 0)) + (discountProvider.ProviderMoney > 0 ? discountProvider.ProviderMoney : 0);
                                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                                _unitOfWork.GetRepositoryInstance<SuInstallmentPayment>().Add(mentPayment);
                                transaction.TransactionId = mentPayment.Id;
                                transaction.Price = float.Parse(money);
                                transaction.SysDiscount = discountProvider.ProviderMoney > 0 ? discountProvider.ProviderMoney : 0;
                                giavon = transaction.Price.Value - transaction.SysDiscount.Value;
                                transaction.SysChargePrice = giavon;
                                transaction.BonusClient = 0;
                                transaction.Amout = 1;
                                _unitOfWork.GetRepositoryInstance<SuTransaction>().Add(transaction);
                                _unitOfWork.SaveChanges();
                                Session["ID_TRA_GOP"] = mentPayment.Id;
                                return RedirectToAction("SuccessPayment", "Pay");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Số dư không đủ để thực hiện giao dịch này !";
                                return RedirectToAction("PaymentService", "Pay");
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Hệ thống đang nâng cấp, quý khách vui lòng quay lại sau.";
                        return RedirectToAction("PaymentService", "Pay");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                    return RedirectToAction("PaymentService", "Pay");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                return RedirectToAction("PaymentService", "Pay");
            }
        }
        public JsonResult LoadFeeTransferValue(float money)
        {
            float strCode = GetFeeTransfer(money);
            return Json(strCode, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadListNganHang()
        {
            string strCode = "";
            List<ListProvider> listProvider = GetListNganHang();
            foreach (var item in listProvider)
            {
                strCode += "<button type = \"button\" style=\"float: left;list-style: none;position: relative;margin-right: 10px;\" data-value=\"" + item.Discount + "\"  class =\"btn btn-default\" id=\"" + item.ProviderName + "\" value=\"" + item.ProviderName + "\">" + "<img class=\"delo-supplier\" src=\"" + item.ProviderIcon + "\" />" + "</button>";
            }
            return Json(strCode, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadListProvider()
        {
            string strCode = "";
            List<ListProvider> listProvider = GetListProvider();
            foreach (var item in listProvider)
            {
                strCode += "<button type = \"button\" style=\"float: left;list-style: none;position: relative;margin-right: 10px;\" data-value=\"" + item.Discount + "\"  class =\"btn btn-default\" id=\"" + item.ProviderName + "\" value=\"" + item.ProviderName + "\">" + "<img class=\"delo-supplier\" src=\"" + item.ProviderIcon + "\" />" + "</button>";
            }
            return Json(strCode, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadListDienNuoc()
        {
            string strCode = "";
            List<ListProvider> lstProvider = GetListDienNuoc();
            foreach (var item in lstProvider)
            {
                strCode += "<button type = \"button\" style=\"float: left;list-style: none;position: relative;margin-right: 10px;\" data-value=\"" + item.Discount + "\"  class =\"btn btn-default\" id=\"" + item.ProviderName + "\" value=\"" + item.ProviderName + "\">" + "<img class=\"delo-supplier\" src=\"" + item.ProviderIcon + "\" />" + "</button>";
            }
            return Json(strCode, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChuyenTienLienNganHang()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChuyenTienLienNganHang(FormCollection form)
        {
            try
            {

                var txtAccountNumber = form["txtAccountNumber"] != null ? form["txtAccountNumber"].ToString() : string.Empty;
                var txtAccountName = form["txtAccountName"] != null ? form["txtAccountName"].ToString() : string.Empty;
                var txtFeeTransfer = form["txtFeeTransfer"] != null ? form["txtFeeTransfer"].ToString() : string.Empty;
                var txtNoiDung = form["txtNoiDung"] != null ? form["txtNoiDung"].ToString() : string.Empty;
                var txtSendName = form["txtSendName"] != null ? form["txtSendName"].ToString() : string.Empty;
                var txtSendPhone = form["txtSendPhone"] != null ? form["txtSendPhone"].ToString() : string.Empty;
                var money = form["txtMoney"] != null ? form["txtMoney"].ToString() : string.Empty;
                var provider = form["hf-ddlagency"] != null ? form["hf-ddlagency"].ToString() : string.Empty;
                //TraGopModels tgInfor = new TraGopModels();

                var suBankTransfer = new SuBankTransferHistory
                {
                    AccountName = txtAccountName,
                    AccountNumber = txtAccountNumber,
                    BankName = provider,
                    Created_At = DateTime.Now.ToString("HH:mm:ss.ffff dd/MM/yyyy"),
                    Detail = txtNoiDung,
                    SendBy = User.Identity.Name,
                    SendName = txtSendName,
                    SendPhone = txtSendPhone,
                    Money = float.Parse(money),
                    ReceiptID = Guid.NewGuid()
                };
                suBankTransfer.FeeTransfer = GetFeeTransfer(suBankTransfer.Money);
                suBankTransfer.Status = Constant.PENDING;
                try
                {
                    var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
                    if (wallet == null || wallet.Balance < suBankTransfer.FeeTransfer + suBankTransfer.Money)
                    {
                        TempData["ErrorMessage"] = "Số dư của bạn không đủ để thực hiện giao dịch, vui lòng kiểm tra lại.";
                        return RedirectToAction("ChuyenTienLienNganHang", "Pay");
                    }
                    if (suBankTransfer.Money > 300000000)
                    {
                        TempData["ErrorMessage"] = "Số tiền chuyển vượt quá định mức vui lòng chuyển tiền làm nhiều lần.";
                        return RedirectToAction("ChuyenTienLienNganHang", "Pay");
                    }
                    if (suBankTransfer.Money < 50000)
                    {
                        TempData["ErrorMessage"] = "Số tiền chuyển thấp hơn định mức dịch vụ.";
                        return RedirectToAction("ChuyenTienLienNganHang", "Pay");
                    }
                    if (wallet != null && wallet.Balance >= suBankTransfer.FeeTransfer + suBankTransfer.Money)
                    {
                        SendOTP(wallet.UserName);
                        Session["TRANSFER"] = suBankTransfer;
                        return RedirectToAction("ConfirmOTPChuyenTien", "Pay");
                        //wallet.Balance = wallet.Balance - suBankTransfer.FeeTransfer - suBankTransfer.Money;
                        //ViewBag.BALANCE = wallet.Balance;
                        //ViewBag.COMISSION = wallet.Commission;
                        //wallet.UpdatedAt = Utility.CurrentDate();
                        //_unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                        //_unitOfWork.GetRepositoryInstance<SuBankTransferHistory>().Add(suBankTransfer);
                        //_unitOfWork.SaveChanges();
                        //Session["ReceiptID_ChuyentienLienNganHang"] = suBankTransfer.ReceiptID;
                        //return RedirectToAction("ChuyenTienLienNganHangThanhCong", "Pay");


                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                        return RedirectToAction("ChuyenTienLienNganHang", "Pay");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                    return RedirectToAction("ChuyenTienLienNganHang", "Pay");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi không thực hiện được giao dịch, vui lòng quay lại sau.";
                return RedirectToAction("ChuyenTienLienNganHang", "Pay");
            }
        }

        [AllowAnonymous]
        public ActionResult ConfirmOTPChuyenTien()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ConfirmOTPChuyenTien(FormCollection form)
        {
            var code = form["txtCode"] != null ? form["txtCode"].ToString() : string.Empty;
            var phone = User.Identity.Name;
            var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(phone));
            var suBankTransfer = Session["TRANSFER"] as SuBankTransferHistory;
            var user = UserManager.FindByName(phone);
            byte[] secretKey = System.Text.Encoding.UTF8.GetBytes(user.PrivateKey);
            var topt = new Totp(secretKey, step: 600);
            long timeWindowUsed;
            bool isMatch = topt.VerifyTotp(code, out timeWindowUsed, null);
            if (isMatch)
            {
                wallet.Balance = wallet.Balance - suBankTransfer.FeeTransfer / 2 - suBankTransfer.Money;
                ViewBag.BALANCE = wallet.Balance;
                ViewBag.COMISSION = wallet.Commission;
                wallet.UpdatedAt = Utility.CurrentDate();
                _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                _unitOfWork.GetRepositoryInstance<SuBankTransferHistory>().Add(suBankTransfer);
                _unitOfWork.SaveChanges();
                Session["ReceiptID_ChuyentienLienNganHang"] = suBankTransfer.ReceiptID;
                return RedirectToAction("ChuyenTienLienNganHangThanhCong", "Pay");
            }
            else
            {
                TempData["Message"] = "Mã xác nhận không đúng hoặc đã hết hạn!";
                return View();
            }
        }

        public float GetFeeTransfer(double? money)
        {
            //if (money >= 50000 && money < 2000000) return 20000 / 2;
            //if (money >= 2000000 && money < 5000000) return 30000 / 2;
            //if (money >= 5000000 && money < 10000000) return 40000 / 2;
            //if (money >= 10000000 && money < 20000000) return 50000 / 2;
            //if (money >= 20000000 && money < 30000000) return 60000 / 2;
            //if (money >= 30000000 && money < 50000000) return 80000 / 2;
            //if (money >= 50000000 && money < 100000000) return 100000 / 2;
            //if (money >= 100000000 && money < 200000000) return 200000 / 2;
            //if (money >= 200000000 && money < 300000000) return 300000 / 2;
            //return 0;
            if (money >= 50000 && money < 2000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV1")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 2000000 && money < 5000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV2")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 5000000 && money < 10000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV3")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 10000000 && money < 20000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV4")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 20000000 && money < 30000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV5")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 30000000 && money < 50000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV6")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else if (money >= 50000000 && money < 100000000)
            {
                var discountGTEL = _unitOfWork.GetRepositoryInstance<suSetting>().GetFirstOrDefaultByParameter(x => x.SKey.Equals("SETTING_BANK_LV7")).SValue.ToIntOrZero();
                return discountGTEL;
            }
            else
            {
                return 0;
            }
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
    }
    public class CardModelBinding
    {
        public string ReceiptNumber { get; set; }
        public List<string> ServiceCode { get; set; }
        public string PhoneNumber { get; set; }
        public float Price { get; set; }
        public int Amount { get; set; }
    }
    public class CardModelPushPrice
    {
        public string ReceiptNumber { get; set; }
        public string ServiceCode { get; set; }
        public string PhoneNumber { get; set; }
        public float Price { get; set; }
        public int Amount { get; set; }
    }
    public class ServiceCode
    {
        public string Code { get; set; }
        public string Value { get; set; }
    }
    public class ListProvider
    {
        public string ProviderName { get; set; }
        public string ProviderIcon { get; set; }
        public double Discount { get; set; }
    }
    public class SharedBudgetInfo
    {
        public string PhoneNumber { get; set; }
        public float Price { get; set; }
        public string Content { get; set; }
    }
    public class PayModel
    {
        public string Phone { get; set; }
        public string Code { get; set; }
        public string Serial { get; set; }
        public string ExpriedDate { get; set; }
        public string Price { get; set; }
        public string ServiceCode { get; set; }
        public string CSKH { get; set; }
    }
    public class PaymentServiceModel
    {
        public string ProviderName { get; set; }
        public string Name { get; set; }
        public string NumberContact { get; set; }
        public string Phone { get; set; }
        public string Money { get; set; }
    }

}