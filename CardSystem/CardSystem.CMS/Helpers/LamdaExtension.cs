using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardSystem.CMS.Helpers
{
    public static class LamdaExtension
    {
        public static DateTime ToDate(this string strDate)
        {
            return DateTime.ParseExact(strDate, "dd/MM/YYYY", null);
        }
        public static DateTime ToDateTime(this string strDate)
        {
            return DateTime.ParseExact(strDate, "dd/MM/YYYY", null);
        }
    }
}