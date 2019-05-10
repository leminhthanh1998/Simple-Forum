using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.CustomAttribute;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using SimpleForum.Models.Forum;
using SimpleForum.Models.Post;
using SimpleForum.Models.Search;
using X.PagedList;

namespace SimpleForum.Controllers
{
    public class SearchController : Controller
    {
        private readonly IPost postService;
        private readonly int pageSize = 10;

        public SearchController(IPost post)
        {
            postService = post;
        }

        [HttpPost]
        public IActionResult Search(string searchQuery)
        {
            return RedirectToAction("Results", new { searchQuery });
        }

        [Route("{controller}/{action}/{searchQuery}/{pageNumber?}")]
        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [OutputCache(Profile = "default")]
        public IActionResult Results(string searchQuery, int? pageNumber)
        {
            var post = postService.GetFilteredPost(searchQuery);

            var areNoResults = (!string.IsNullOrEmpty(searchQuery)) && !post.Any();

            var postListing = post.Select(p => new PostListingModel
            {
                Id=p.ID,
                AuthorName=p.User.UserName,
                AuthorId=p.User.Id,
                AuthorRating=p.User.Rating,
                Title=p.Title,
                Slug=p.Slug,
                DatePosted=p.Created.ToString(),
                RepliesCount=p.Replies.Count(),
                Forum= BuildForumListing(p)
            });

            var model = new SearchResultModel
            {
                SearchQuery=searchQuery,
                EmptySearchResults=areNoResults
            };
            var pageN = pageNumber ?? 1;
            var onePageOfPost = postListing.ToPagedList(pageN, pageSize);
            ViewBag.OnePageOfPost = onePageOfPost;

            return View(model);
        }

        private ForumListingModel BuildForumListing(Post p)
        {
            var forum = p.Forum;

            return new ForumListingModel
            {
                Id = forum.ID,
                ImageUrl = forum.ImageUrl,
                Description = forum.Description,
                Name = forum.Title
            };
        }
    }
}