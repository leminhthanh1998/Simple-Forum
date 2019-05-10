using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.CustomAttribute;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using SimpleForum.Helper;
using SimpleForum.Models.Forum;
using SimpleForum.Models.Post;
using X.PagedList;

namespace SimpleForum.Controllers
{
    public class ForumController : Controller
    {
        private readonly IForum forumService;
        private readonly IPost postService;
        private readonly int pageSize = 10;

        public ForumController(IForum forum, IPost post)
        {
            forumService = forum;
            postService = post;
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [OutputCache(Profile = "default")]
        public IActionResult Index()
        {
            var forums = forumService.GetAll()
                .Select(f => new ForumListingModel
                {
                    Id = f.ID,
                    Description = f.Description,
                    Name = f.Title,
                    ImageUrl=f.ImageUrl,
                    LatestPost=postService.GetLatestPostByForumID(f.ID)
                });



            var model = new ForumIndexModel
            {
                ForumList = forums
            };

            return View(model);
        }

        [Route("Forum/{id}/{pageNumber?}/{searchQuery?}")]
        [OutputCache(Profile = "default")]
        public IActionResult Topic(int id, int? pageNumber = 1, string searchQuery="")
        {
            var forum = forumService.GetByID(id);

            var post = postService.GetFilteredPost(forum, searchQuery).ToList();
            

            var postListing = post.Select(p => new PostListingModel
            {
                Id = p.ID,
                AuthorName=p.User.UserName,
                AuthorId = p.User.Id,
                AuthorRating = p.User.Rating,
                Title = p.Title,
                DatePosted = p.Created.ToString(),
                RepliesCount=p.Replies.Count(),
                Slug=p.Slug,
                Forum= BuildForumListing(p)
            });

            var model = new ForumTopicModel
            {
                Forum = BuildForumListing(forum)
            };

            var pageN = pageNumber ?? 1;
            var onePageOfPost = postListing.ToPagedList(pageN, pageSize);
            ViewBag.OnePageOfPost = onePageOfPost;
            return View(model);
        }

        [HttpPost]
        public IActionResult Search(int id, string searchQuery)
        {
            return RedirectToAction("Topic", new { id,pageNumber=1, searchQuery });
        }

        private ForumListingModel BuildForumListing(Forum forum)
        {
            return new ForumListingModel
            {
                Id = forum.ID,
                Name = forum.Title,
                Description = forum.Description,
                ImageUrl = forum.ImageUrl
            };
        }

        private ForumListingModel BuildForumListing(Post p)
        {
            var forum = p.Forum;
            return BuildForumListing(forum);
        }
    }
}