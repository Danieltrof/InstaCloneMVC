using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApi.ViewModels
{
    public class CreatePostViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The title must be between 1 and 100 characters.", MinimumLength = 1)]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Please upload an image.")]
        public required IFormFile Image { get; set; }
    }
}
