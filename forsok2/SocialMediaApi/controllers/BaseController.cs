using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialMediaApi.Models;
using SocialMediaApi.ViewModels;

namespace SocialMediaApi.Controllers{
    public abstract class BaseController : Controller
    {
        protected void SetLayoutData(BaseViewModel viewModel)
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            viewModel.IsAuthenticated = isAuthenticated;
            viewModel.CurrentUsername = User.Identity?.Name;
        }
    }
}