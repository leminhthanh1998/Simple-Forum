using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using SimpleForum.CustomAttribute;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using SimpleForum.Helper;
using SimpleForum.Helper.Recapcha;
using SimpleForum.Hubs;
using SimpleForum.Models.Reply;

namespace SimpleForum.Controllers
{
    public class ReplyController : Controller
    {
        private readonly IForum _forumService;
        private readonly IPost _postService;
        private readonly IPostReply _postReply;
        private readonly IApplicationUser _userService;
        private readonly IPostFormatter postFormatter;
        private readonly IHubContext<NotiHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecaptchaSettings _recapchaSettings;
        private readonly int pageSize=10;
        private readonly LinkGenerator _linkGenerator;

        public ReplyController(IForum forumService, IPost postService, IApplicationUser userService, IPostReply reply, UserManager<ApplicationUser> userManager, IPostFormatter formatter, IHubContext<NotiHub> hubContext, RecaptchaSettings recapchaSettings, LinkGenerator linkGenerator)
        {
            _forumService = forumService;
            _postService = postService;
            _userService = userService;
            _postReply = reply;
            _userManager = userManager;
            postFormatter = formatter;
            _hubContext = hubContext;
            _recapchaSettings = recapchaSettings;
            _linkGenerator = linkGenerator;
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        public async Task<IActionResult> Create(string id)
        {
            var post = _postService.GetBySlug(id);
            var forum = _forumService.GetByID(post.Forum.ID);

            var name = User.Identity.Name;

            var user = await _userManager.FindByNameAsync(name);

            var model = new PostReplyModel
            {
                PostContent = postFormatter.Prettify(post.Content),
                PostTitle = post.Title,
                PostId = post.ID,
                CloseOrOpen=post.CloseOrOpen,
                ForumName = forum.Title,
                ForumId = forum.ID,
                ForumImageUrl = forum.ImageUrl,
                Slug=post.Slug,
                AuthorName = User.Identity.Name,
                AuthorImageUrl = user.ProfileImageUrl,
                AuthorId = user.Id,
                AuthorRating = user.Rating,
                IsAdmin = IsAuthorAdmin(user),
                IsMod=IsAuthorMod(user),
                IsActive=user.IsActive,
                IsReview=post.IsReview,
                Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")),
                //Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")),
                NumberOfReply =post.Replies.Count()
            };

            return View(model);
        }

        private bool IsAuthorAdmin(ApplicationUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result;
            return role.Contains("Admin");
        }
        private bool IsAuthorMod(ApplicationUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result;
            return role.Contains("Mod");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(PostReplyModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                if( !IsAuthorAdmin(user) && !IsAuthorMod(user) && _postService.GetPostCountUser(user.Id) < 6)
                {
                    if (!await RecaptchaHelper.IsReCaptchaPassedAsync(Request.Form["g-recaptcha-response"], _recapchaSettings.SecretKey))
                    {
                        ModelState.AddModelError(string.Empty, "Recaptcha nói rằng bạn không phải là người!");
                        return RedirectToAction("Create", new { id = model.Slug });
                    }

                }
                var reply = BuildReply(model, user);
                await _postService.AddReply(reply);                
                var pageNumber = Math.Ceiling((double)(model.NumberOfReply+1) / pageSize);
                if (pageNumber == 0) pageNumber = 1;

                var count = (model.NumberOfReply+1) % pageSize;
                if (count == 0) count = 10;

                var htmlData = CreateHtmlReply(reply, count);

                await _hubContext.Clients.All.SendAsync("ReceiveNotify", model.Slug, (model.NumberOfReply + 1) % pageSize, pageNumber, htmlData);

                var link = _linkGenerator.GetPathByAction("Index", "Post", new { id = model.Slug, pageNumber = pageNumber }, new PathString(), new FragmentString($"#{count}"), new LinkOptions());
                return Redirect(link);
                //return RedirectToAction("Index", "Post", new { id = model.Slug, pageNumber = pageNumber }, fragment:$"{count}");
            }
            else return RedirectToAction("Create", new { id = model.Slug });
        }

        public string CreateHtmlReply(PostReply model, int count)
        {
            var linkProfile = _linkGenerator.GetPathByAction("Detail", "Profile", new { id = model.User.Id });
            string data = string.Empty;
            
            if(IsAuthorAdmin(model.User) && model.User.IsActive)
            {
                 data = $"<div class=\"row replyContent\" id=\"{count}\">\r\n " +
                    $"<div class=\"col-md-3 replyAuthorContainer\">\r\n " +
                    $"<div class=\"postAuthorImage\" style=\"background-image: url({model.User.ProfileImageUrl})\"></div><br/>\r\n" +
                    $"<a href=\"{linkProfile}\">{model.User.UserName}</a><br />" +
                    $"<span style=\"color: red\">Admin</span><br />" +
                    $"<span>{model.Created}</span>" + $"</div>\r\n" +
                    $"<div class=\"col-md-9 replyContentContainer\">\r\n" +
                    $"<div class=\"postContent\">\r\n" +
                    $"{model.Content}" +
                    $"</div>\r\n" + $"</div>\r\n" +
                    $"</div>";
                return data;
            }
            if(IsAuthorMod(model.User) && model.User.IsActive)
            {
                data = $"<div class=\"row replyContent\" id=\"{count}\">\r\n " +
                   $"<div class=\"col-md-3 replyAuthorContainer\">\r\n " +
                   $"<div class=\"postAuthorImage\" style=\"background-image: url({model.User.ProfileImageUrl})\"></div><br/>\r\n" +
                   $"<a href=\"{linkProfile}\">{model.User.UserName}</a><br />" +
                   $"<span style=\"color: hotpink\">Mod</span><br />" +
                   $"<span>{model.Created}</span>" + $"</div>\r\n" +
                   $"<div class=\"col-md-9 replyContentContainer\">\r\n" +
                   $"<div class=\"postContent\">\r\n" +
                   $"{model.Content}" +
                   $"</div>\r\n" + $"</div>\r\n" +
                   $"</div>";
                return data;
            }
            if (model.User.IsActive)
            {
                data = $"<div class=\"row replyContent\" id=\"{count}\">\r\n " +
                    $"<div class=\"col-md-3 replyAuthorContainer\">\r\n " +
                    $"<div class=\"postAuthorImage\" style=\"background-image: url({model.User.ProfileImageUrl})\"></div><br/>\r\n" +
                    $"<a href=\"{linkProfile}\">{model.User.UserName}</a><br />" +
                    $"<span>{model.Created}</span>" + $"</div>\r\n" +
                    $"<div class=\"col-md-9 replyContentContainer\">\r\n" +
                    $"<div class=\"postContent\">\r\n" +
                    $"{model.Content}" +
                    $"</div>\r\n" + $"</div>\r\n" +
                    $"</div>";
                return data;
            }

            return data;
        }

        private PostReply BuildReply(PostReplyModel reply, ApplicationUser user)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            //var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            var post = _postService.GetByID(reply.PostId);

            return new PostReply
            {
                Post = post,
                Content = reply.ReplyContent,
                Created = now,
                User = user
            };
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var reply = _postReply.GetById(id);

            var model = new DeleteReplyModel
            {
                PostID=reply.Post.ID,
                ReplyId=id,
                IsActive = user.IsActive
            };

            if (userId == reply.User?.Id || User.IsInRole("Admin") || User.IsInRole("Mod"))
                return View(model);
            else
                return RedirectToAction("Index", "Post", new { id = reply.Post.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int ReplyId)
        {
            var reply = _postReply.GetById(ReplyId);
            await _postReply.Delete(ReplyId);

            return RedirectToAction("Index", "Post", new { id = reply.Post.Slug });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var reply = _postReply.GetById(id);

            var model = new EditReplyModel
            {
                Content=reply.Content,
                ID=reply.ID,
                IsActive=user.IsActive
            };

            if (userId == reply.User?.Id || User.IsInRole("Admin") || User.IsInRole("Mod"))
                return View(model);
            else
                return RedirectToAction("Index", "Post", new { id = reply.Post.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReplyContent(EditReplyModel edit)
        {
            if(ModelState.IsValid)
            {
                await _postReply.Edit(edit.ID, edit.Content);
                var reply = _postReply.GetById(edit.ID);

                return RedirectToAction("Index", "Post", new { id = reply.Post.Slug });
            }
            else return RedirectToAction("Edit", new { id = edit.ID });
        }
    }
}