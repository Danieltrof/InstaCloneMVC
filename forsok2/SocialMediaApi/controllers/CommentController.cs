using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApi.DAL;
using SocialMediaApi.Models;
using SocialMediaApi.ViewModels;
using SocialMediaApi.Repositories;

namespace SocialMediaApi.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<CommentController> _logger;

        public CommentController(
            ICommentRepository commentRepository,
            IPostRepository postRepository,
            IAuthRepository authRepository,
            ILogger<CommentController> logger)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _authRepository = authRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var comments = await _commentRepository.GetAllComments();
            return View(comments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CommentCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authRepository.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var post = await _postRepository.GetPostById(model.PostId);
            if (post == null)
                return NotFound("Post not found");

            var comment = new Comment
            {
                Content = model.Content,
                Created = DateTime.UtcNow,
                Post = post,
                User = user
            };

            var createdComment = await _commentRepository.CreateComment(comment);
            if (createdComment == null)
            {
                _logger.LogError("Failed to create comment");
                return StatusCode(500, "Failed to create comment");
            }

            return RedirectToAction("Details", "Post", new { id = model.PostId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
                return NotFound();

            var user = await _authRepository.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (comment.User.Id != user.Id)
                return Forbid();

            var success = await _commentRepository.DeleteComment(id);
            if (!success)
            {
                _logger.LogError($"Failed to delete comment {id}");
                return StatusCode(500, "Failed to delete comment");
            }

            return RedirectToAction("Details", "Post", new { id = comment.Post.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string content)
        {
            if (string.IsNullOrEmpty(content))
                return BadRequest("Comment content cannot be empty");

            var comment = await _commentRepository.GetCommentById(id);
            if (comment == null)
                return NotFound();

            var user = await _authRepository.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (comment.User.Id != user.Id)
                return Forbid();

            comment.Content = content;
            var success = await _commentRepository.UpdateComment(comment);
            if (!success)
            {
                _logger.LogError($"Failed to update comment {id}");
                return StatusCode(500, "Failed to update comment");
            }

            return RedirectToAction("Details", "Post", new { id = comment.Post.Id });
        }
    }
}