using CardSystem.Model.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CardSystem.CMS.Models
{
    public class UserListEditViewModel
    {
        public Model.Entity.SuUsersInfoes usersInfo { get; set; }
        //public string Picture { get; set; }
        public string FullName { get; set; }
        public string BirthDay { get; set; }
        public string Address { get; set; }
        public string CMND { get; set; }
        public bool IsCustomer { get; set; }
        public int Status { get; set; }
        //public ApplicationUser appUser { get; set; }
        public string UsersName { get; set; }
    }
}