using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaApi.ViewModels
{
    public class CreatePostViewModel : BaseViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an image")]
        [Display(Name = "Upload Image")]
        public IFormFile? Image { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();
        public bool CanEdit { get; set; }
    }
}
