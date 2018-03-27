using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardSystem.CMS.Models
{
    public class SuCommentViewModels
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
        public string Created_At { get; set; }
        public string Created_By { get; set; }
        public long ParentId { get; set; }
    }
}