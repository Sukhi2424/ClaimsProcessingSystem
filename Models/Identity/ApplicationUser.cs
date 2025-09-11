using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClaimsProcessingSystem.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Profile Picture")]
        public string? ProfilePicturePath { get; set; }
        [StringLength(100)]
        [Display(Name = "Full Name")]
        [Required]
        public string? FullName { get; set; }

        [StringLength(50)]
        [Display(Name = "Employee Number")]
        [Required]
        public string? EmployeeNo { get; set; }

        public DateTime? LastLoginTime { get; set; }
    }
}