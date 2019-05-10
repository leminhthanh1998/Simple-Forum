using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SimpleForum.Data.Models;
using SimpleForum.Data;
using SimpleForum.Models.ApplicationUser;
using Microsoft.AspNetCore.Hosting;
using SimpleForum.Models.Post;
using SimpleForum.Models.Forum;
using X.PagedList;
using SimpleForum.CustomAttribute;

namespace SimpleForum.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private readonly IPost _post;
        private readonly int pageSize = 10;


        public ProfileController(UserManager<ApplicationUser> userManager, IApplicationUser userService, IConfiguration configuration, IHostingEnvironment hostingEnvironment, IPost post)
        {
            _userManager = userManager;
            _userService = userService;
            _configuration = configuration;
            _environment = hostingEnvironment;
            _post = post;
        }

        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [Route("Members/{pageNumber?}")]
        [OutputCache(Profile = "default")]
        public IActionResult Index(int? pageNumber)
        {
            var profiles = _userService.GetAll()
                .OrderBy(user => user.UserName)
                .Select(u => new ProfileModel
                {
                    Email = u.Email,
                    ProfileImageUrl = u.ProfileImageUrl,
                    UserRating = u.Rating.ToString(),
                    DateJoined = u.MemberSince,
                    IsActive = u.IsActive,
                    Username=u.UserName,
                    UserId=u.Id
                });

            var model = new ProfileListModel
            {
                Profiles = profiles
            };

            var pageN = pageNumber ?? 1;
            var onePageOfPost = profiles.ToPagedList(pageN, pageSize);
            ViewBag.OnePageOfProfile = onePageOfPost;

            return View(model);            
        }

        [Authorize]
        [Route("{controller}/{action}/{id}/{pageNumber?}")]
        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        [OutputCache(Profile = "default")]
        public IActionResult FindAllPost(string id, int? pageNumber)
        {
            var posts = _post.GetPostsByUserId(id);

            var postListing = posts.Select(p => new PostListingModel
            {
                Id = p.ID,
                AuthorName = p.User.UserName,
                AuthorId = p.User.Id,
                AuthorRating = p.User.Rating,
                Title = p.Title,
                Slug=p.Slug,
                DatePosted = p.Created.ToString(),
                RepliesCount = p.Replies.Count(),
                Forum = BuildForumListing(p)
            });

            var model = new FindAllPostModel
            {
                UserName= _userService.GetById(id).UserName,
                Id=id
            };
            var pageN = pageNumber ?? 1;
            var onePageOfPost = postListing.ToPagedList(pageN, pageSize);
            ViewBag.OnePageOfPost = onePageOfPost;

            return View(model);
        }


        [Authorize]
        [Throttle(Name = "ThrottleTest", Seconds = 5)]
        public IActionResult Detail(string id)
        {
            var user = _userService.GetById(id);
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var model = new ProfileModel()
            {
                UserId = user.Id,
                Username = user.UserName,
                UserRating = user.Rating.ToString(),
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                DateJoined = user.MemberSince,
                IsActive = user.IsActive,
                IsAdmin = userRoles.Contains("Admin"),
                IsMod=userRoles.Contains("Mod"),
                PostCount=_post.GetPostCountUser(user.Id),
                IsComfirmEmail=user.EmailConfirmed
            };

            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(200_000)]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file != null && file.Length< 200001)
            {
                var userId = _userManager.GetUserId(User);

                var userName = _userManager.GetUserName(User);

                var content = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

                var ex = Path.GetExtension(content.FileName.ToString().Trim('"')).ToLower();

                if(ex.Contains("jpg") || ex.Contains("png")|| ex.Contains("gif"))
                {
                    var path = Path.Combine(_environment.WebRootPath + "\\images\\users", userName + ex);


                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    await _userService.SetProfileImage(userId, "/images/users/" + userName + ex);

                    return RedirectToAction("Detail", "Profile", new { id = userId });
                }
                else return RedirectToAction("HandleErrorCode", "Error", new { statusCode = 406 });
            }
            else return RedirectToAction("HandleErrorCode", "Error", new { statusCode = 406 });
        }

        public async Task<IActionResult> BanOrUnban(string userId)
        {
            var user = _userService.GetById(userId);
            var role = _userManager.GetRolesAsync(user).Result;
            if(!role.Contains("Admin") && !role.Contains("Mod"))
                await _userService.BanOrUnban(user);
            return RedirectToAction("Detail", "Profile", new {id=userId });
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