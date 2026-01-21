using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Constants;

public enum NotificationMethod
{
    [Display(Name = "sms")]
    SMS = 0,

    [Display(Name = "push")]
    Push = 1,

    [Display(Name = "email")]
    Email = 2,
}