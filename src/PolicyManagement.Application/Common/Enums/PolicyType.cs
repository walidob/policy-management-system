using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Application.Common.Enums;

public enum PolicyType
{
    [Display(Name = "Health Insurance")]
    HealthInsurance = 1,
    
    [Display(Name = "Life Insurance")]
    LifeInsurance = 2,
    
    [Display(Name = "Property Insurance")]
    PropertyInsurance = 3,
}