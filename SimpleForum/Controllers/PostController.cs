using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.CustomAttribute;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using SimpleForum.Helper;
using SimpleForum.Models.Post;
using SimpleForum.Models.Reply;
using X.PagedList;

namespace SimpleForum.Controllers
{
    public class PostController : Controller
    {
        private IPost postService;
        private IForum forumService;
        private IPostFormatter postFormatter;
        private IFriendlyUrl friendlyUrl;
        private UserManager<ApplicationUser> _userManager;
        private int pageSize = 10;

        public PostController(IPost post, IForum forum, UserManager<ApplicationUser> userManager, IPostFormatter formatter, IFriendlyUrl friendlyUrl)
        {
            postService = post;
            forumService = forum;
            postFormatter = formatter;
            this.friendlyUrl = friendlyUrl;
            _userManager = userManager;
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [Route("Post/{id}/{pageNumber?}")]
        public IActionResult Index(string id, int? pageNumber = 1)
        {

            var post = postService.GetBySlug(id);
            var replies = BuildPostReplies(post.Replies);

            var model = new PostIndexModel
            {
                Id=post.ID,
                Title=post.Title,
                AuthorName=post.User.UserName,
                AuthorID=post.User.Id,
                AuthorImageUrl=post.User.ProfileImageUrl,
                IsActive=post.User.IsActive,
                Created=post.Created,
                PostContent= postFormatter.Prettify(post.Content),
                Replies=replies,
                ForumId=post.Forum.ID,
                ForumName=post.Forum.Title,
                IsAuthorAdmin= IsAuthorAdmin(post.User),
                IsMod=IsMod(post.User),
                CloseOrOpen=post.CloseOrOpen,
                IsReview=post.IsReview,
                Slug=post.Slug
            };
            var pageN = 0;
            if (pageNumber == null || pageNumber == 0)
                pageN = 1;
            else pageN =(int)pageNumber;
            var onePageOfReply = replies.ToPagedList(pageN, pageSize);
            ViewBag.OnePageOfReply = onePageOfReply;
            return View(model);
        }

       
        [Authorize(Roles ="Admin,Mod")]
        public async Task<IActionResult> CloseOrOpen(string id)
        {
            var post = postService.GetBySlug(id);
            await postService.CloseOrOpen(id);
            return RedirectToAction("Index", "Post", new { id = post.Slug });
        }


        [Authorize(Roles = "Admin,Mod")]
        public async Task<IActionResult> AcceptPost(string id)
        {
            var post = postService.GetBySlug(id);
            await postService.AcceptPost(id);
            return RedirectToAction("Index", "Post", new { id = post.Slug });
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        public async Task<IActionResult> Create(int id)
        {
            var forum = forumService.GetByID(id);
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new NewPostModel
            {
                ForumName=forum.Title,
                ForumId=forum.ID,
                ForumImageUrl=forum.ImageUrl,
                AuthorName=User.Identity.Name,
                IsActive= user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost(NewPostModel newPost)
        {
            if(ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);

                var post = BuildPost(newPost, user);

                postService.Add(post).Wait(); //

                var newP = postService.GetByID(post.ID);

                return RedirectToAction("Index", "Post", new { id = newP.Slug });
            }
            else return RedirectToAction("Create", new { id =  newPost.ForumId});
        }

        private bool IsAuthorAdmin(ApplicationUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result;
            return role.Contains("Admin");
        }

        private bool IsMod(ApplicationUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result;
            return role.Contains("Mod");
        }
        private Post BuildPost(NewPostModel newPost, ApplicationUser user)
        {
            var forum = forumService.GetByID(newPost.ForumId);

            return new Post
            {
                Title = newPost.Title,
                Content = newPost.Content.Trim(),
                Created = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                //Created = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")),
                User = user,
                Forum = forum,
                IsReview = CheckReview(user),
                Slug = friendlyUrl.GetFriendlyTitle(newPost.Title)
            };
        }

        private bool CheckReview(ApplicationUser user)
        {
            if (IsAuthorAdmin(user))
                return false;
            if (IsMod(user))
                return false;
            return true;
        }

        private IEnumerable<PostReplyModel> BuildPostReplies(IEnumerable<PostReply> replies)
        {
            return replies.Select(reply => new PostReplyModel
            {
                Id=reply.ID,
                AuthorName=reply.User.UserName,
                AuthorId=reply.User.Id,
                AuthorImageUrl=reply.User.ProfileImageUrl,
                IsActive=reply.User.IsActive,
                Date=reply.Created,
                ReplyContent=postFormatter.Prettify(reply.Content.Trim()),
                IsAdmin=IsAuthorAdmin(reply.User),
                IsMod=IsMod(reply.User)
            });
        }

        [Authorize(Roles ="Admin,Mod")]
        public IActionResult Move(string id)
        {
            var forums = forumService.GetAll();
            var post = postService.GetBySlug(id);
            var model = new MovePostIndexModel
            {
                PostId = id,
                OldForumId= post.Forum.ID,
                Forums = forums
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MovePost(string postID, int newForumId)
        {
            await postService.MovePost(postID, newForumId);

            return RedirectToAction("Index", "Post", new { id = postID });
        }


        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            var post = postService.GetBySlug(id);
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new EditPostModel
            {
                Title = post.Title,
                Content = post.Content,
                PostID=id,
                IsActive=user.IsActive
            };

            if (userId == post.User.Id || User.IsInRole("Admin") || User.IsInRole("Mod"))
                return View(model);
            else
                return RedirectToAction("Index", "Post", new { id = post.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPostContent(EditPostModel edit)
        {
           if(ModelState.IsValid)
            {
                var newSlug = friendlyUrl.GetFriendlyTitle(edit.Title);
                var tempSlug = edit.PostID;
                if (newSlug == edit.PostID) newSlug = "";
                else tempSlug = newSlug;

                await postService.EditPostContent(edit.PostID, edit.Content.Trim(), edit.Title, newSlug);

                return RedirectToAction("Index", "Post", new { id = tempSlug });
            }
           else return RedirectToAction("Edit", new { id = edit.PostID });
        }

        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = _userManager.GetUserId(User);
            var post = postService.GetBySlug(id);
            var user = await _userManager.FindByIdAsync(userId);

            var model = new DeletePostModel
            {
                PostId = post.Slug,
                ForumId=post.Forum.ID,
                PostAuthor = post.User.UserName,
                PostContent = post.Content,
                IsActive=user.IsActive
            };

            if(userId==post.User.Id || User.IsInRole("Admin") || User.IsInRole("Mod"))
            return View(model);
            else 
                return RedirectToAction("Index", "Post", new { id = post.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string PostId)
        {
            var post = postService.GetBySlug(PostId);
            await postService.Delete(PostId);

            return RedirectToAction("Topic", "Forum", new { id = post.Forum.ID });
        }
    }
}