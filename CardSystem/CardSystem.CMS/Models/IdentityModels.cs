using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
//using CardSystem.Model.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace CardSystem.CMS.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public virtual SuUsersInfo UsersInfo { get; set; }
        [Required]
        [MaxLength(16)]
        public string PrivateKey { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            //Configuration.LazyLoadingEnabled = false;
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
             modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityUserRole>()
            .HasKey(r => new { r.UserId, r.RoleId })
            .ToTable("AspNetUserRoles");
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        public DbSet<SuUsersInfo> UserInfo { get; set; }
         
    }

    //Importance to add migrate Identity
    public class SuUsersInfo
    {
        [Key]
        public int Id { get; set; }
        public string ApplicationUserID { get; set; }
        public string Picture { get; set; }
        public string BirthDay { get; set; }
        public string Address { get; set; }
        public int ParentId { get; set; }
        public bool IsCustomer { get; set; }
        public string Created_At { get; set; }
        public string Updated_At { get; set; }
        public int Status { get; set; }
        public DateTime LastLogin { get; set; }
        public string CMND { get; set; }
        public string FullName { get; set; }
    }
}