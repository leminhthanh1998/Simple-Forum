using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.CustomAttribute;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using SimpleForum.Models;
using SimpleForum.Models.Forum;
using SimpleForum.Models.Home;
using SimpleForum.Models.Post;

namespace SimpleForum.Controllers
{
    public class HomeController : Controller
    {
        private IPost postService;

        public HomeController(IPost post)
        {
            postService = post;
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [OutputCache(Profile = "default")]
        public IActionResult Index()
        {
            var model =  BuildHomeIndexModel();
            return View(model);
        }

        private HomeIndexModel BuildHomeIndexModel()
        {
            var latestPost = postService.GetLatestPost(10);
            
            var post = latestPost.Select(p => new PostListingModel
            {
                Title=p.Title,
                Id=p.ID,
                AuthorName=p.User.UserName,
                AuthorId=p.User.Id,
                AuthorRating=p.User.Rating,
                DatePosted=p.Created.ToString(),
                RepliesCount=p.Replies.Count(),
                Slug=p.Slug,
                Forum= GetForumListingForPost(p)
            });

            return new HomeIndexModel
            {
                LatestPosts=post,
                SearchQuery=""
            };
        }

        private ForumListingModel GetForumListingForPost(Post post)
        {
            var forum = post.Forum;
            return new ForumListingModel
            {
                Name = forum.Title,
                Id = forum.ID,
                ImageUrl=forum.ImageUrl
            };
        }

        [Route("{controller}/{action}")]
        public string GetPost(int pageIndex, int pageSize)
        {
            var post = postService.GetLatestPost(pageIndex, pageSize);
            var htmlData = string.Empty;
            foreach (var item in post)
            {
                var text = $"<tr>\r\n  <td>\r\n  <div class=\"forumLogo\" style=\"background-image: url({item.Forum.ImageUrl});\"></div>\r\n                            <div class=\"postTitle\">\r\n <a href=\"https://simpleforum-bts.azurewebsites.net/Post/{item.Slug}\">{item.Title}</a>\r\n  </div>\r\n  <div class=\"postSubTitle\">\r\n  <span>{item.Replies.Count()} trả lời </span>                                <span>\r\n                                    <a href=\"https://simpleforum-bts.azurewebsites.net/Profile/Detail/{item.User.Id}\">{item.User.UserName}</a>\r\n                                </span>\r\n                            </div>\r\n                        </td>\r\n                    </tr>";
                htmlData += text;
            }

            return htmlData;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
