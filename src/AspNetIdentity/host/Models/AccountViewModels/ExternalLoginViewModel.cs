using System.ComponentModel.DataAnnotations;

namespace Open.IdentityServer.Models.AccountViewModels;

public class ExternalLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}