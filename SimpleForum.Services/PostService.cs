using Microsoft.EntityFrameworkCore;
using SimpleForum.Data;
using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleForum.Services
{
    public class PostService : IPost
    {
        private ApplicationDbContext context;

        public PostService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task Add(Post post)
        {
            var slug = post.Slug;
            var i = 0;
            while (context.Posts.Any(p => p.Slug == slug))
            {
                slug = $"{post.Slug}-{++i}";
            }
            post.Slug = slug;
            context.Add(post);
            await context.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            var post = GetBySlug(id);
            var replies = post.Replies;
            foreach (var item in replies)
            {
                context.Remove(item);
            }
            context.Remove(post);
            await context.SaveChangesAsync();
        }

        public async Task CloseOrOpen(string id)
        {
            var post = GetBySlug(id);
            post.CloseOrOpen = !post.CloseOrOpen;
            context.Update(post);
            await context.SaveChangesAsync();
        }

        public async Task EditPostContent(string id, string newContent, string newTitle, string newSlug)
        {
            var post = GetBySlug(id);
            if(newSlug!="")
            {
                var slug = newSlug;
                var i = 0;
                while (context.Posts.Any(p => p.Slug == newSlug))
                {
                    slug = $"{newSlug}-{++i}";
                }
                post.Slug = slug;
            }
            else post.Slug = id;

            post.Content = newContent;
            post.Title = newTitle;

            context.Posts.Update(post);
            await context.SaveChangesAsync();
        }

        public async Task AcceptPost(string slug)
        {
            var post = GetBySlug(slug);
            post.IsReview = false;
            context.Update(post);
            await context.SaveChangesAsync();
        }

        public async Task MovePost(string postId, int forumId)
        {
            var post = GetBySlug(postId);
            post.ForumID = forumId;
            context.Update(post);
            await context.SaveChangesAsync();
        }

        public IEnumerable<Post> GetAll()
        {
            return context.Posts
                 .Include(p => p.User)
                .Include(p => p.Replies).ThenInclude(reply => reply.User)
                .Include(p => p.Forum);
        }

        public Post GetBySlug(string slug)
        {
            return GetAll().First(p => p.Slug == slug);
        }

        public Post GetByID(int id)
        {
            return context.Posts.Where(post => post.ID == id)
                .Include(p => p.User)
                .Include(p => p.Replies).ThenInclude(reply=>reply.User)
                .Include(p => p.Forum)
                .First();
        }

        public IEnumerable<Post> GetAllNeedReviewPost()
        {
            return GetAll().Where(p => p.IsReview == true);
        }

        public IEnumerable<Post> GetFilteredPost(Forum forum, string searchQuery)
        {
            return string.IsNullOrEmpty(searchQuery) ? GetPostByForum(forum.ID) :
                forum.Posts
                .OrderByDescending(p => p.Created)
                .Where(post => post.Title.ToLower().Contains(searchQuery.ToLower())
                || post.Content.ToLower().Contains(searchQuery.ToLower()));
                
        }

        public IEnumerable<Post> GetFilteredPost(string searchQuery)
        {
            return GetAll()
                .OrderByDescending(p => p.Created)
                .Where(post => (post.Title.ToLower().Contains(searchQuery.ToLower())
                || post.Content.ToLower().Contains(searchQuery.ToLower())) && post.IsReview == false);
                
        }

        public IEnumerable<Post> GetLatestPost(int number)
        {
            return GetAll().OrderByDescending(p => p.Created).Where(p=>p.IsReview==false).Take(number);
        }

        public IEnumerable<Post> GetLatestPost(int pageIndex, int pageSize)
        {
            return GetAll().OrderByDescending(p => p.Created).Where(p => p.IsReview == false).Skip(pageIndex*pageSize).Take(pageSize);
        }

        public IEnumerable<Post> GetPostByForum(int id)
        {
            return context.Forums
                .Where(f => f.ID == id).First()
                .Posts.Where(p=>p.IsReview==false).OrderByDescending(p => p.Created);
        }

        public IEnumerable<Post> GetPostsByUserId(string id)
        {
            return GetAll().Where(post => post.User.Id == id);
        }

        public int GetPostCountUser(string id)
        {
            return GetPostsByUserId(id).Count();
        }

        public async Task AddReply(PostReply reply)
        {
            context.Replies.Add(reply);
            await context.SaveChangesAsync();
        }

        public Post GetLatestPostByForumID(int id)
        {
            return GetPostByForum(id).FirstOrDefault();
        }
    }
}
