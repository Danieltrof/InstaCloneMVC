namespace SocialMediaApi.ViewModels
{
    public class PostViewModel : BaseViewModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Title { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();
    }

    public class CommentViewModel
    {
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}