using System.ComponentModel.DataAnnotations;

namespace SocialMediaApi.ViewModels
{
    public abstract class BaseViewModel
    {
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }
}