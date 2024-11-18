using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApi.DAL;
using SocialMediaApi.Models;
using SocialMediaApi.ViewModels;
using SocialMediaApi.Repositories;
using System.IO;

namespace SocialMediaApi.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<PostController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(
            IPostRepository postRepository,
            IAuthRepository authRepository,
            ILogger<PostController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _postRepository = postRepository;
            _authRepository = authRepository;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllPosts();
            var currentUser = await _authRepository.GetUserAsync(User);
            
            var viewModels = posts.Select(post => new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                ImageUrl = post.ImageUrl,
                Created = post.Created,
                UserName = post.User.Name,
                UserId = post.User.Id,
                LikesCount = post.Likes.Count,
                IsLikedByCurrentUser = post.Likes.Any(l => l.User.Id == currentUser?.Id),
                Comments = post.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserName = c.User.Name,
                    Created = c.Created
                }).ToList(),
                CanEdit = currentUser?.Id == post.User.Id
            }).ToList();

            return View(viewModels);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePostViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authRepository.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var imageUrl = await SaveImage(model.Image);
            if (string.IsNullOrEmpty(imageUrl))
            {
                ModelState.AddModelError("", "Failed to upload image");
                return View(model);
            }

            var post = new Post
            {
                Title = model.Title,
                ImageUrl = imageUrl,
                Created = DateTime.UtcNow,
                User = user
            };

            var success = await _postRepository.CreatePost(post);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to create post");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _authRepository.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var post = await _postRepository.GetPostById(id);
            if (post == null)
                return NotFound();

            if (post.User.Id != user.Id)
                return Forbid();

            var success = await _postRepository.DeletePost(id);
            if (!success)
            {
                TempData["Error"] = "Failed to delete post";
                return RedirectToAction(nameof(Index));
            }

            await DeletePostImage(post.ImageUrl);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Like(int id)
        {
            var success = await _postRepository.LikePost(id);
            return Json(new { success });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Unlike(int id)
        {
            var success = await _postRepository.UnlikePost(id);
            return Json(new { success });
        }

        private async Task<string?> SaveImage(IFormFile? image)
        {
            if (image == null || image.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder); // Create if doesn't exist

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
                return $"/uploads/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving image: {ex.Message}");
                return null;
            }
        }

        private static Task DeletePostImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return Task.CompletedTask;

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            return Task.CompletedTask;
        }
    }
}