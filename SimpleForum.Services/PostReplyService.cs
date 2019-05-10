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
    public class PostReplyService : IPostReply
    {
        private readonly ApplicationDbContext context;

        public PostReplyService(ApplicationDbContext dbContext)
        {
            context = dbContext;
        }

        public async Task Delete(int id)
        {
            var reply = GetById(id);
            context.Remove(reply);
            await context.SaveChangesAsync();
        }

        public async Task Edit(int id, string message)
        {
            var reply = GetById(id);
            reply.Content = message;
            //reply.Created = DateTime.Now;
            context.Update(reply);
            await context.SaveChangesAsync();
        }

        public PostReply GetById(int id)
        {
            return context.Replies
               .Include(r => r.Post)
               .ThenInclude(post => post.Forum)
               .Include(r => r.Post)
               .ThenInclude(post => post.User).First(r => r.ID == id);
        }
    }
}
