using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Enums;

public enum ClaimStatus
{
    [Display(Name = "Submitted")]
    Submitted = 1,
    
    [Display(Name = "Approved")]
    Approved = 2,
    
    [Display(Name = "Rejected")]
    Rejected = 3
} 