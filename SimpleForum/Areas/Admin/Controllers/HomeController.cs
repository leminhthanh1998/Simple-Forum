using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.Areas.Admin.Models.Home;
using SimpleForum.Data;
using SimpleForum.Helper;

namespace SimpleForum.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class HomeController : Controller
    {
        private readonly IForum _forum;
        private readonly IPost _post;
        private readonly MetricsReader _metricsReader;

        public HomeController(IForum forum, IPost post)
        {
            _forum = forum;
            _post = post;
            _metricsReader = new MetricsReader();
        }

        public IActionResult Index()
        {
            var forum = _forum.GetAll();
            var numberOfPostReview = _post.GetAllNeedReviewPost().Count();

            var forumIndex = forum.Select(f => new HomeListingModel
            {
                Title=f.Title,
                Description=f.Description,
                ImgUrl=f.ImageUrl,
                Id=f.ID
            });

            var model = new HomeIndexModel
            {
                HomeList = forumIndex,
                NumberOfPostNeedReview=numberOfPostReview
            };
            ViewBag.NumberOfPostNeedReview = (int?)numberOfPostReview ?? 0;
            return View(model);
        }

        #region ViewMetrics
        [HttpGet("admin/getpv")]
        [Authorize(Roles = "Admin")]
        public async Task<int> GetPageViewsForLast24Hours()
        {
            var response = await _metricsReader.GetP1DIntMetrics("pageViews/count", "sum");
            if (null != response)
            {
                return (int)response.value["pageViews/count"].sum;
            }
            return -1;
        }

        [HttpGet("admin/getpvd")]
        [Authorize(Roles = "Admin")]
        public async Task<int> GetPageViewsDurationForLast24Hours()
        {
            var response = await _metricsReader.GetP1DIntMetrics("pageViews/duration", "avg");
            if (null != response)
            {
                return (int)response.value["pageViews/duration"].avg;
            }
            return -1;
        }

        [HttpGet("admin/getrd")]
        [Authorize(Roles = "Admin")]
        public async Task<double> GetRequestDurationForLast24Hours()
        {
            var response = await _metricsReader.GetP1DIntMetrics("requests/duration", "avg");
            if (null != response)
            {
                return (double)response.value["requests/duration"].avg;
            }
            return -1;
        }

        [HttpGet("admin/getse")]
        [Authorize(Roles = "Admin")]
        public async Task<int> GetServerExceptionForLast24Hours()
        {
            var response = await _metricsReader.GetP1DIntMetrics("exceptions/server", "sum");
            if (null != response)
            {
                return (int)response.value["exceptions/server"].sum;
            }
            return -1;
        }

        [HttpGet("admin/getbe")]
        [Authorize(Roles = "Admin")]
        public async Task<int> GetBrowserExceptionForLast24Hours()
        {
            var response = await _metricsReader.GetP1DIntMetrics("exceptions/browser", "sum");
            if (null != response)
            {
                return (int)response.value["exceptions/browser"].sum;
            }
            return -1;
        }
        #endregion

        public IActionResult Delete(int id)
        {
            var forum = _forum.GetByID(id);

            var model = new DeleteForumModel
            {
                Id = forum.ID
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int Id)
        {
            await _forum.Delete(Id);
           
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Edit(int id)
        {
            var forum = _forum.GetByID(id);

            var model = new EditForumModel
            {
                Title = forum.Title,
                Description = forum.Description,
                Id = forum.ID
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditForum(IFormFile file, string Title, string Description, int Id)
        {
            await _forum.UpdateForum(Id, Title, Description, file);

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Create()
        {
            var model = new CreateForumModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewForum(IFormFile file, string Title, string Description)
        {
            await _forum.Create(Title, Description, file);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult StatusForum()
        {
            var numberOfPostReview = _post.GetAllNeedReviewPost().Count();
            ViewBag.NumberOfPostNeedReview = (int?)numberOfPostReview ?? 0;
            return View();
        }
        public IActionResult OnlineMembers()
        {
            var numberOfPostReview = _post.GetAllNeedReviewPost().Count();
            ViewBag.NumberOfPostNeedReview = (int?)numberOfPostReview ?? 0;
            return View();
        }

    }
}