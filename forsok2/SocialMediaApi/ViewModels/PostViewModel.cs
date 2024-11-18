using SocialMediaApi.ViewModels;

namespace SocialMediaApi.ViewModels
{
    public class PostViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
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