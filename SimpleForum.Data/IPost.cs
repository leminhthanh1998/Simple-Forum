using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleForum.Data
{
    public interface IPost
    {
        Post GetByID(int id);
        Post GetBySlug(string slug);
        Post GetLatestPostByForumID(int id);
        IEnumerable<Post> GetAll();
        IEnumerable<Post> GetFilteredPost(Forum forum, string searchQuery);
        IEnumerable<Post> GetFilteredPost(string searchQuery);
        Task AddReply(PostReply postReply);
        Task Add(Post post);
        Task Delete(string id);
        Task CloseOrOpen(string id);
        Task AcceptPost(string id);
        Task MovePost(string postId, int forumId);
        Task EditPostContent(string id, string newContent, string newTitle, string newSlug);
        IEnumerable<Post> GetPostByForum(int id);
        IEnumerable<Post> GetLatestPost(int number);
        IEnumerable<Post> GetLatestPost(int pageIndex, int pageSize);
        IEnumerable<Post> GetAllNeedReviewPost();
        IEnumerable<Post> GetPostsByUserId(string id);
        int GetPostCountUser(string id);

    }
}
