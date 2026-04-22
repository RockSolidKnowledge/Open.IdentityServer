using System.ComponentModel.DataAnnotations;

namespace Open.IdentityServer.Models.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}