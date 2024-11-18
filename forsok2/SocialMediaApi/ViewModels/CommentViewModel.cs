using SocialMediaApi.ViewModels;

public class CommentViewModel
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public UserViewModel User { get; set; } = new();
}