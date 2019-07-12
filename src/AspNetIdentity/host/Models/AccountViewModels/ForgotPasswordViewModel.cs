using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
