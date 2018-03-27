using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CardSystem.Model.Entity;
using CardSystem.Repository;

namespace CardSystem.Helpers
{
    public class PageSEOHelper
    {
        public static string UpdateMetaDetails(string pageUrl)
        {
            //--- StringBuilder object to store MetaTags information.
            StringBuilder sbMetaTags = new StringBuilder();

            //--Step1 Get data from database.
            using (GenericUnitOfWork db = new GenericUnitOfWork())
            {
                var obj = db.GetRepositoryInstance<PageMetaDetail>().GetFirstOrDefaultByParameter(a => a.PageUrl == pageUrl);
                if (obj == null)
                {
                    return sbMetaTags.ToString();
                }
                else
                {
                    //---- Step2 In this step we will add <title> tag to our StringBuilder Object.
                    sbMetaTags.Append("<title>" + obj.Title + "</title>");
                    sbMetaTags.Append(Environment.NewLine);
                }
                //---- Step3 In this step we will add "Meta Description" to our StringBuilder Object.
                sbMetaTags.Append("<meta name=\"description\" content =\"" + obj.MetaDescription + "\"/>");
                sbMetaTags.Append(Environment.NewLine);
                //---- Step4 In this step we will add "Meta Keywords" to our StringBuilder Object.
                sbMetaTags.Append("<meta name=\"keywords\" content =\"" + obj.MetaKeywords + "\"/>");
            }
            return sbMetaTags.ToString();
        }
        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }
        public static string GenerateMetaDetails(string strTitle, string strDescription, string strKey)
        {
            //--- StringBuilder object to store MetaTags information.
            StringBuilder sbMetaTags = new StringBuilder();

            //--Step1 Get data from database.
            //---- Step2 In this step we will add <title> tag to our StringBuilder Object.
            sbMetaTags.Append("<title>" + strTitle + "</title>");
            sbMetaTags.Append(Environment.NewLine);

            //---- Step3 In this step we will add "Meta Description" to our StringBuilder Object.
            sbMetaTags.Append("<meta name=\"description\" content = \"" + strKey + "\">");
            sbMetaTags.Append(Environment.NewLine);
            //---- Step4 In this step we will add "Meta Keywords" to our StringBuilder Object.
            sbMetaTags.Append("<meta name=\"keywords\" content = \"" + strKey + "\">");
            return sbMetaTags.ToString();
        }

    }
    public static class SlugHelper
    {
        public static string GenerateSlug(int Id, string Name)
        {
            //Name = FriendlyUrlHelper.RemoveSign4VietnameseString(Name);
            string phrase = string.Format("{0}-{1}", Id, Name);

            string str = FriendlyUrlHelper.RemoveSign4VietnameseString(phrase).ToLower();
            //string str = phrase;
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }
        public static string GeneratSlugEx(int Id, string Name)
        {
            return string.Format("{0}-{1}", Id, FriendlyUrlHelper.GetFriendlyTitle(Name, true));
        }
        private static string RemoveAccent(string text)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
    /// <summary>
    /// Helps convert <see cref="string"/> title text to URL friendly <see cref="string"/>'s that can safely be
    /// displayed in a URL.
    /// </summary>
    public static class FriendlyUrlHelper
    {
        /// <summary>
        /// Converts the specified title so that it is more human and search engine readable e.g.
        /// http://example.com/product/123/this-is-the-seo-and-human-friendly-product-title. Note that the ID of the
        /// product is still included in the URL, to avoid having to deal with two titles with the same name. Search
        /// Engine Optimization (SEO) friendly URL's gives your site a boost in search rankings by including keywords
        /// in your URL's. They are also easier to read by users and can give them an indication of what they are
        /// clicking on when they look at a URL. Refer to the code example below to see how this helper can be used.
        /// Go to definition on this method to see a code example. To learn more about friendly URL's see
        /// https://moz.com/blog/15-seo-best-practices-for-structuring-urls.
        /// To learn more about how this was implemented see
        /// http://stackoverflow.com/questions/25259/how-does-stack-overflow-generate-its-seo-friendly-urls/25486#25486
        /// </summary>
        /// <param name="title">The title of the URL.</param>
        /// <param name="remapToAscii">if set to <c>true</c>, remaps special UTF8 characters like 'è' to their ASCII
        /// equivalent 'e'. All modern browsers except Internet Explorer display the 'è' correctly. Older browsers and
        /// Internet Explorer percent encode these international characters so they are displayed as'%C3%A8'. What you
        /// set this to depends on whether your target users are English speakers or not.</param>
        /// <param name="maxlength">The maximum allowed length of the title.</param>
        /// <returns>The SEO and human friendly title.</returns>
        /// <code>
        /// [HttpGet("product/{id}/{title}", Name = "GetDetails")]
        /// public IActionResult Product(int id, string title)
        /// {
        ///     // Get the product as indicated by the ID from a database or some repository.
        ///     var product = ProductRepository.Find(id);
        ///
        ///     // If a product with the specified ID was not found, return a 404 Not Found response.
        ///     if (product == null)
        ///     {
        ///         return this.HttpNotFound();
        ///     }
        ///
        ///     // Get the actual friendly version of the title.
        ///     var friendlyTitle = FriendlyUrlHelper.GetFriendlyTitle(product.Title);
        ///
        ///     // Compare the title with the friendly title.
        ///     if (!string.Equals(friendlyTitle, title, StringComparison.Ordinal))
        ///     {
        ///         // If the title is null, empty or does not match the friendly title, return a 301 Permanent
        ///         // Redirect to the correct friendly URL.
        ///         return this.RedirectToRoutePermanent("GetProduct", new { id = id, title = friendlyTitle });
        ///     }
        ///
        ///     // The URL the client has browsed to is correct, show them the view containing the product.
        ///     return this.View(product);
        /// }
        /// </code>
        public static string GetFriendlyTitle(string title, bool remapToAscii = false, int maxlength = 80)
        {
            if (title == null)
            {
                return string.Empty;
            }

            int length = title.Length;
            bool prevdash = false;
            StringBuilder stringBuilder = new StringBuilder(length);
            char c;

            for (int i = 0; i < length; ++i)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    stringBuilder.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lower-case
                    stringBuilder.Append((char)(c | 32));
                    prevdash = false;
                }
                else if ((c == ' ') || (c == ',') || (c == '.') || (c == '/') ||
                    (c == '\\') || (c == '-') || (c == '_') || (c == '='))
                {
                    if (!prevdash && (stringBuilder.Length > 0))
                    {
                        stringBuilder.Append('-');
                        prevdash = true;
                    }
                }
                else if (c >= 128)
                {
                    int previousLength = stringBuilder.Length;

                    if (remapToAscii)
                    {
                        stringBuilder.Append(RemapInternationalCharToAscii(c));
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }

                    if (previousLength != stringBuilder.Length)
                    {
                        prevdash = false;
                    }
                }

                if (i == maxlength)
                {
                    break;
                }
            }

            if (prevdash)
            {
                return stringBuilder.ToString().Substring(0, stringBuilder.Length - 1);
            }
            else
            {
                return stringBuilder.ToString();
            }
        }

        /// <summary>
        /// Remaps the international character to their equivalent ASCII characters. See
        /// http://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </summary>
        /// <param name="character">The character to remap to its ASCII equivalent.</param>
        /// <returns>The remapped character</returns>
        private static string RemapInternationalCharToAscii(char character)
        {
            string s = character.ToString().ToLowerInvariant();
            if ("àåáâäãåąā".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (character == 'ř')
            {
                return "r";
            }
            else if (character == 'ł')
            {
                return "l";
            }
            else if (character == 'đ')
            {
                return "d";
            }
            else if (character == 'ß')
            {
                return "ss";
            }
            else if (character == 'Þ')
            {
                return "th";
            }
            else if (character == 'ĥ')
            {
                return "h";
            }
            else if (character == 'ĵ')
            {
                return "j";
            }
            else
            {
                return string.Empty;
            }
        }
        private static readonly string[] VietnameseSigns = new string[]
        {

            "aAeEoOuUiIdDyY",

            "áàạảãâấầậẩẫăắằặẳẵ",

            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",

            "éèẹẻẽêếềệểễ",

            "ÉÈẸẺẼÊẾỀỆỂỄ",

            "óòọỏõôốồộổỗơớờợởỡ",

            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",

            "úùụủũưứừựửữ",

            "ÚÙỤỦŨƯỨỪỰỬỮ",

            "íìịỉĩ",

            "ÍÌỊỈĨ",

            "đ",

            "Đ",

            "ýỳỵỷỹ",

            "ÝỲỴỶỸ"
        };
        public static string RemoveSign4VietnameseString(string str)
        {
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }

    }
}