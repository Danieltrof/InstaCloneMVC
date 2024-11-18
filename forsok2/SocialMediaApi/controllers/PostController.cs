using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApi.DAL;
using SocialMediaApi.Models;
using SocialMediaApi.ViewModels;

namespace SocialMediaApi.Controllers
{
    public class PostController : BaseController
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PostController> _logger;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public PostController(
            IPostRepository postRepository, 
            UserManager<ApplicationUser> userManager,
            ILogger<PostController> logger)
        {
            _postRepository = postRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllPosts();
            var viewModels = posts.Select(post => new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                ImageUrl = post.ImageUrl,
                Created = post.Created,
                UserName = post.User.Name,
                LikesCount = post.Likes.Count,
                Comments = post.Comments.Select(c => new CommentViewModel
                {
                    Content = c.Content,
                    UserName = c.User.Name,
                    Created = c.Created
                }).ToList()
            }).ToList();

            SetLayoutData(viewModels.FirstOrDefault() ?? new PostViewModel());
            return View(viewModels);
        }

        [Authorize]
        public IActionResult Create()
        {
            var viewModel = new CreatePostViewModel();
            SetLayoutData(viewModel);
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid || model.Image == null)
            {
                SetLayoutData(model);
                return View(model);
            }

            if (!ValidateImage(model.Image))
            {
                ModelState.AddModelError("Image", "Please upload a valid image file (jpg, jpeg, png, gif) under 5MB.");
                SetLayoutData(model);
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var fileName = await SaveImageAsync(model.Image);
                var post = new Post
                {
                    Title = model.Title,
                    ImageUrl = "/uploads/" + fileName,
                    User = user
                };

                await _postRepository.CreatePost(post);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", user.Id);
                ModelState.AddModelError("", "Error uploading image. Please try again.");
                SetLayoutData(model);
                return View(model);
            }
        }
        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
                
            var post = await _postRepository.GetPostById(id);
            if (post == null)
                return NotFound();

            if (post.User.Id != user.Id)
            {
                _logger.LogWarning("User {UserId} attempted to delete post {PostId} belonging to another user", user.Id, id);
                return Forbid();
            }

            try
            {
                await DeletePostImage(post.ImageUrl);
                await _postRepository.DeletePost(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId} for user {UserId}", id, user.Id);
                TempData["Error"] = "Failed to delete post. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task DeletePostImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
            if (File.Exists(imagePath))
                File.Delete(imagePath);
        }

        private bool ValidateImage(IFormFile file)
        {
            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var filePath = Path.Combine(uploadDir, fileName);
            
            Directory.CreateDirectory(uploadDir);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}