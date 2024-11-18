namespace SocialMediaApi.ViewModels
{
    public abstract class BaseViewModel
    {
        public string? Title { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? CurrentUsername { get; set; }
    } 
}