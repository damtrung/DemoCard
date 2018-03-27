using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardSystem.CMS.Models
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Parent { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public double? Balance { get; set; }
        public double? Commission { get; set; }
        public string Created_At { get; set; }
    }
}