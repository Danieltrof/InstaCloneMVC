using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApi.Models;
using SocialMediaApi.ViewModels;
using SocialMediaApi.Repositories;

namespace SocialMediaApi.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthRepository authRepository,
            ILogger<AccountController> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid");
                return View(model);
            }

            var result = await _authRepository.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                isPersistent: false, 
                lockoutOnFailure: false);

            _logger.LogInformation("Login result: {Succeeded}", result.Succeeded);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in successfully.");
                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("Register attempt with data: {@Model}", model);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                Name = model.FullName
            };

            var result = await _authRepository.CreateUserAsync(user, model.Password);
            _logger.LogInformation("User creation result: {Succeeded}", result.Succeeded);
            
            if (result.Succeeded)
            {
                await _authRepository.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Post");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authRepository.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Post");
        }
    }
}