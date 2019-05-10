using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SimpleForum.Services
{
    public class ForumService : IForum
    {
        private readonly ApplicationDbContext context;
        private readonly IHostingEnvironment _environment;
        private readonly IPost _post;

        public ForumService(ApplicationDbContext context, IHostingEnvironment hostingEnvironment, IPost post)
        {
            this.context = context;
            _environment = hostingEnvironment;
            _post = post;
        }

        public async Task Create(string Title, string Description, IFormFile file)
        {
            var forum = new Forum
            {
                Title = Title,
                Description = Description
            };

            await SetForumImage(forum, file);

            context.Add(forum);
            await context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var forum = GetByID(id);

            var postInThisForum = GetAllPostInForumById(id);

            var postInThisForumClone = postInThisForum.ToList();

            foreach (var item in postInThisForumClone)
            {
                await _post.Delete(item.Slug);
            }

            context.Remove(forum);
            await context.SaveChangesAsync();
        }

        public IEnumerable<Forum> GetAll()
        {
            return context.Forums
                .Include(f=>f.Posts);
        }

        public IEnumerable<ApplicationUser> GetAllActiveUser()
        {
            throw new NotImplementedException();
        }

        public Forum GetByID(int id)
        {
            return  context.Forums
                .Where(f => f.ID == id)
                .Include(f => f.Posts)
                .ThenInclude(f => f.User)
                .Include(f => f.Posts)
                .ThenInclude(f => f.Replies)
                .ThenInclude(f=>f.User)
                .FirstOrDefault();
        }

        public IEnumerable<Post> GetAllPostInForumById(int idForum)
        {
            return GetByID(idForum).Posts;
        }

      

        public async Task UpdateForum(int id, string Title, string Description, IFormFile file)
        {
            var forum = GetByID(id);
            forum.Title = Title;
            forum.Description = Description;
            await SetForumImage(forum, file);
            context.Update(forum);
            await context.SaveChangesAsync();
        }

        private async Task SetForumImage(Forum forum, IFormFile file)
        {
            if(file != null)
            {
                var content = ContentDispositionHeaderValue.Parse(file.ContentDisposition);

                var fileName = content.FileName.ToString().Trim('"').ToLower();

                if (fileName.Contains(".png") || fileName.Contains(".jpg") || fileName.Contains(".jpeg"))
                {
                    var path = Path.Combine(_environment.WebRootPath + "\\images\\forum", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    forum.ImageUrl = "/images/forum/" + fileName;
                }
            }
        }
    }
}
