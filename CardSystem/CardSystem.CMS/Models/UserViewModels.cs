using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CardSystem.CMS.Common;
namespace CardSystem.CMS.Models
{
    public class UserViewModels
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings =false,ErrorMessage ="Tên tài khoản là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên tài khoản tối thiểu 3 ký tự và không được quá 50 ký tự.", MinimumLength = 3)]
        [Display(Name = "Tên tài khoản")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "Mật khẩu phải tối thiểu 6 ký tự bao gồm cả chữ cái và số.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Nhắc lại mật khẩu không đúng.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhắc mật khẩu")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Tên đầy đủ không quá 100 ký tự.", MinimumLength = 6)]
        [Display(Name = "Tên đầy đủ")]
        public string FullName { get; set; }
        [Display(Name = "Ảnh avatar")]
        public string Picture { get; set; }
        public byte[] ImageData { get; set; }
        [Display(Name = "Ngày sinh")]
        public string BirthDay { get; set; }
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        [Display(Name = "Người giới thiệu")]
        public int ParentId { get; set; }
        [Display(Name = "Trạng thái")]
        public int Status { get; set; }
        [Display(Name = "Là khách hàng")]
        public bool IsCustomer { get; set; }
        [PhoneAnotation]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
        [RegularExpression("([0-9]{10})|([0-9]{11})|([0-9]{12})", ErrorMessage = "Số CMT hoặc số căn cước không hợp lệ.")]
        [Display(Name = "Số CMTND")]
        public string CMTND { get; set; }
        [Display(Name = "Id tài khoản")]
        public string ApplicationID { get; set; }
        [Display(Name = "Ngày tạo")]
        public string Created_At { get; set; }
        [Display(Name = "Ngày cập nhật")]
        public string Updated_At { get; set; }
        [Display(Name = "Đăng nhập lần cuối")]
        public DateTime LastLogin { get; set; }
    }
}