using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Utitlity;
using CardSystem.CMS.Models;
using Microsoft.AspNet.Identity;
using System.Data.SqlClient;

namespace CardSystem.CMS.Controllers
{
    [Authorize]
    [RoutePrefix("cua-hang")]
    public class ShoppingController : BaseController
    {
        public ShoppingController() : base()
        {

        }

        // GET: Shop
        [Route("gio-hang")]
        public ActionResult MyCart()
        {
            var userId = User.Identity.GetUserId();
            var lstCart = _unitOfWork.GetRepositoryInstance<SuCart>().GetListByParameter(x => x.UserId.Equals(userId) && x.CartStatusId.Equals(Constant.CART_ADDED));
            var lstCartViewModel = new List<CartViewModel>();
            foreach (var item in lstCart)
            {
                CartViewModel cart = new CartViewModel();
                cart.category = _unitOfWork.GetRepositoryInstance<SuCategory>().GetFirstOrDefault(item.SuProduct.CategoryId.Value);
                cart.cart = item;
                lstCartViewModel.Add(cart);
            }
            return View(lstCartViewModel);
        }
        [Route("checkout")]
        public ActionResult CheckOut()
        {
            var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.GetUserId() };
            var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
            var lstCartDetails = _unitOfWork.GetRepositoryInstance<UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
            ViewBag.TotalPrice = lstCartDetails.Sum(i => i.TotalPrice);
            ViewBag.CartIds = string.Join(",", lstCartDetails.Select(i => i.CartId).ToList());
            return View(lstCartDetails);
        }
        [Route("success")]
        public ActionResult PaymentSuccess(SuShippingDetail shippingDetails, FormCollection cl)
        {
            SuShippingDetail sd = new SuShippingDetail();
            sd.UserId = User.Identity.GetUserId();
            sd.AddressLine = shippingDetails.AddressLine;
            sd.City = shippingDetails.City;
            sd.District = shippingDetails.District;
            sd.PhoneNumber = shippingDetails.PhoneNumber;
            sd.OrderId = Guid.NewGuid().ToString();
            //sd.PayementType = shippingDetails.PayementType;
            sd.AmountPaid = float.Parse(cl["TotalPrice"].ToString());
            sd.PayementType = int.Parse(Request.Form["PaymentType"].ToString());
            var wallet = _unitOfWork.GetRepositoryInstance<SuWalletInfo>().GetFirstOrDefaultByParameter(x => x.UserName.Equals(User.Identity.Name));
            if(sd.PayementType == 0)
            {
                if (wallet.Balance < float.Parse(cl["TotalPrice"].ToString()))
                {
                    //Session["NOTE"] = "error";
                    TempData["Message"] = "Số dư của bạn không đủ để thanh toán";
                    return RedirectToAction("CheckOut", "shopping");
                }
                else
                {
                    //Session["NOTE"] = "success";
                    wallet.Balance = wallet.Balance - float.Parse(cl["TotalPrice"].ToString());
                    wallet.UpdatedAt = Utility.CurrentDate();
                    _unitOfWork.GetRepositoryInstance<SuWalletInfo>().Update(wallet);
                    _unitOfWork.SaveChanges();

                    _unitOfWork.GetRepositoryInstance<SuShippingDetail>().Add(sd);
                    _unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => i.UserId == sd.UserId && i.CartStatusId.Equals(Constant.CART_ADDED), (j => j.CartStatusId = Constant.CART_PURCHASED));
                    _unitOfWork.SaveChanges();
                    if (!string.IsNullOrEmpty(Request["CartIds"]))
                    {
                        int[] cartIdsToUpdate = Request["CartIds"].Split(',').Select(Int32.Parse).ToArray();
                        _unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => cartIdsToUpdate.Contains(i.CartId), (j => j.ShipDetailId = sd.ShipDetailId));
                        _unitOfWork.SaveChanges();
                    }
                    return View(sd);
                }
            }
            else
            {
                _unitOfWork.GetRepositoryInstance<SuShippingDetail>().Add(sd);
                _unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => i.UserId == sd.UserId && i.CartStatusId.Equals(Constant.CART_ADDED), (j => j.CartStatusId = Constant.CART_PURCHASED));
                _unitOfWork.SaveChanges();
                if (!string.IsNullOrEmpty(Request["CartIds"]))
                {
                    int[] cartIdsToUpdate = Request["CartIds"].Split(',').Select(Int32.Parse).ToArray();
                    _unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => cartIdsToUpdate.Contains(i.CartId), (j => j.ShipDetailId = sd.ShipDetailId));
                    _unitOfWork.SaveChanges();
                }
                return View(sd);
            }

            //_unitOfWork.GetRepositoryInstance<SuShippingDetail>().Add(sd);
            //_unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => i.UserId == sd.UserId && i.CartStatusId.Equals(Constant.CART_ADDED), (j => j.CartStatusId = Constant.CART_PURCHASED));
            //_unitOfWork.SaveChanges();
            //if (!string.IsNullOrEmpty(Request["CartIds"]))
            //{
            //    int[] cartIdsToUpdate = Request["CartIds"].Split(',').Select(Int32.Parse).ToArray();
            //    _unitOfWork.GetRepositoryInstance<SuCart>().UpdateByWhereClause(i => cartIdsToUpdate.Contains(i.CartId), (j => j.ShipDetailId = sd.ShipDetailId));
            //    _unitOfWork.SaveChanges();

            //}
            //return View(sd);
        }

        public JsonResult AddProductToCart(int productId, int quantity)
        {
            SuCart c = new SuCart();
            c.Created_At = Utility.CurrentDate();
            c.CartStatusId = Constant.CART_ADDED;
            c.UserId = User.Identity.GetUserId();
            c.ProductId = productId;
            c.Quantity = quantity;
            c.Update_At = Utility.CurrentDate();
            _unitOfWork.GetRepositoryInstance<SuCart>().Add(c);
            _unitOfWork.SaveChanges();
            var count = int.Parse(ViewBag.TOTALITEMCART.ToString()) ;
            ViewBag.TOTALITEMCART =count + 1;            
            TempData["ProductAddedToCart"] = "Đã thêm sản phẩm vào giỏ hàng thành công";
            return Json(ViewBag.TOTALITEMCART, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveCartItem(int productId)
        {
            var userId = User.Identity.GetUserId();
            SuCart c = _unitOfWork.GetRepositoryInstance<SuCart>().GetFirstOrDefaultByParameter(i => i.ProductId == productId && i.UserId == userId && i.CartStatusId.Equals(Constant.CART_ADDED));
            c.CartStatusId = Constant.CART_REMOVED;
            c.Update_At = Utility.CurrentDate();
            _unitOfWork.GetRepositoryInstance<SuCart>().Update(c);
            _unitOfWork.SaveChanges();
            var count = int.Parse(ViewBag.TOTALITEMCART.ToString());
            ViewBag.TOTALITEMCART = count - 1;
            TempData["RemoveCartItem"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateQuantity(string productId, string quantity)
        {
            var userId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(quantity))
            {
                return Json("false");
            }
            var prId = long.Parse(productId);
            SuCart c = _unitOfWork.GetRepositoryInstance<SuCart>().GetFirstOrDefaultByParameter(i => i.ProductId == prId && i.UserId == userId && i.CartStatusId.Equals(Constant.CART_ADDED));
            c.Update_At = Utility.CurrentDate();
            c.Quantity = int.Parse(quantity);
            _unitOfWork.GetRepositoryInstance<SuCart>().Update(c);
            _unitOfWork.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);

        }
    }
}