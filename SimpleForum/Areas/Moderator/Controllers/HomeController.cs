using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.Areas.Admin.Models.Review;
using SimpleForum.Data;

namespace SimpleForum.Areas.Moderator.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles ="Mod")]
    public class HomeController : Controller
    {
        private readonly IPost _post;

        public HomeController(IPost post)
        {
            _post = post;
        }

        public IActionResult Index()
        {
            var postNeedToReview = _post.GetAllNeedReviewPost();

            var model = postNeedToReview.Select(p => new ReviewPostListingModel
            {
                Id = p.ID,
                Title = p.Title,
                UserId = p.User.Id,
                UserName = p.User.UserName,
                UserImgUrl = p.User.ProfileImageUrl,
                Created = p.Created,
                Slug=p.Slug
            });


            return View(model);
        }

        public async Task<IActionResult> Accept(string id)
        {
            var post = _post.GetBySlug(id);
            await _post.AcceptPost(id);
            return RedirectToAction("Index");
        }
    }
}