using CardSystem.Model.Entity;
using CardSystem.Repository;
using CardSystem.Utitlity;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Controllers
{
    public class SearchController : BaseController
    {
        public SearchController() : base()
        {

        }
        // GET: Search
        public ActionResult Index(FormCollection form)
        {
            //var nameSearch = new SqlParameter("Name", System.Data.SqlDbType.NVarChar) { Value = form[""] != null ? form[""].ToString() : string.Empty};
            var nameSearch = new SqlParameter("Name", System.Data.SqlDbType.NVarChar) { Value = Session["TXTSEARCH"] != null ? Session["TXTSEARCH"] : string.Empty };
            var status = new SqlParameter("Status", System.Data.SqlDbType.Int) { Value = 1 };
            var createBy = new SqlParameter("CreateBy", System.Data.SqlDbType.NVarChar) { Value = "" };
            var updateAt = new SqlParameter("UpdateAt", System.Data.SqlDbType.NVarChar) { Value = "" };
            var sortBy = new SqlParameter("SortBy", System.Data.SqlDbType.Int) { Value = 4 };
            var sortDirect = new SqlParameter("SortDirect", System.Data.SqlDbType.Int) { Value = 2 };
            var pageIndex = new SqlParameter("PageIndex", System.Data.SqlDbType.Int) { Value = 1 };
            var pageSize = new SqlParameter("PageSize", System.Data.SqlDbType.Int) { Value = 10 };
            var lstproductDetails = _unitOfWork.GetRepositoryInstance<SuProduct>().GetResultBySqlProcedure("Ad_Product_SearchFull @Name,@Status,@CreateBy, @UpdateAt, @SortBy, @SortDirect, @PageIndex, @PageSize", nameSearch,status,createBy,updateAt,sortBy,sortDirect,pageIndex,pageSize).ToList();
            return View(lstproductDetails);
        }
    }
}