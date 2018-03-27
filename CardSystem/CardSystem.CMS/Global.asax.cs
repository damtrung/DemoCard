using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Utitlity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CardSystem.CMS
{
    public class MvcApplication : System.Web.HttpApplication

    {
        //private int totalItemCart; // the global private variable

        //internal int TotalItemCart // the global controlled variable
        //{
        //    get
        //    {
        //        GenericUnitOfWork _unitOfWork = new GenericUnitOfWork();
        //        if (User.Identity.GetUserId() != null)
        //        {
        //            var sqlUser = new SqlParameter("userId", System.Data.SqlDbType.NVarChar) { Value = User.Identity.GetUserId() };
        //            var sqlStatus = new SqlParameter("cartStatus", System.Data.SqlDbType.NVarChar) { Value = Constant.CART_ADDED };
        //            var lstCartDetails = _unitOfWork.GetRepositoryInstance<UserShoppingCartDetails_Result>().GetResultBySqlProcedure("UserShoppingCartDetails @userId,@cartStatus", sqlUser, sqlStatus).ToList();
        //            totalItemCart = lstCartDetails.Count;
        //        }
        //        else { totalItemCart = 0; }

        //        return totalItemCart;
        //    }
        //}
        protected void Application_Start()
        {
            //Application["TotalItemCart"] = TotalItemCart;// the global variable's bed

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.txt");
            using (StreamWriter sw = new StreamWriter(logFilePath, true, System.Text.Encoding.UTF8))
            {
                sw.Write(HttpContext.Current.Error);
            }
        }
    }
}
