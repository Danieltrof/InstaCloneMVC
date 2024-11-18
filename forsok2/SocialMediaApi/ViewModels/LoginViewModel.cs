using System.ComponentModel.DataAnnotations;

namespace SocialMediaApi.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least 6 characters long.", MinimumLength = 6)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}