using System;

namespace SocialMediaApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required Post Post { get; set; }
        public DateTime Created { get; set; }
        public required ApplicationUser User { get; set; }
    }
}