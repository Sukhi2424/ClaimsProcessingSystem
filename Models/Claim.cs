using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace ClaimsProcessingSystem.Models
{
    // This defines the possible statuses a claim can have.
    // Using an enum like this is much safer than using simple strings.
    public enum ClaimStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Claim
    {
        [Key] // This makes Id the primary key
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Requested Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal RequestedAmount { get; set; }

        [Display(Name = "Approved Amount")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ApprovedAmount { get; set; } // Nullable decimal

        [Required]
        public DateTime DateSubmitted { get; set; }
        [Display(Name = "Supporting Document")]
        public string? SupportingDocumentPath { get; set; }
        [Required]
        public ClaimStatus Status { get; set; }

        // Foreign Key to link to the IdentityUser table
        public string? SubmittingUserId { get; set; }

        // Navigation property to access the full User object
        [ForeignKey("SubmittingUserId")]
        public virtual ApplicationUser? SubmittingUser { get; set; }
    }
}