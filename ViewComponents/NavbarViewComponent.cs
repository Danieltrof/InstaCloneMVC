using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourProject.Models;
using YourProject.ViewModels;

namespace YourProject.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public NavbarViewComponent(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var isAuthenticated = _signInManager.IsSignedIn(HttpContext.User);
            var username = isAuthenticated ? (await _userManager.GetUserAsync(HttpContext.User))?.UserName : null;

            var model = new LayoutViewModel
            {
                IsAuthenticated = isAuthenticated,
                CurrentUsername = username
            };

            return View(model);
        }
    }
} 