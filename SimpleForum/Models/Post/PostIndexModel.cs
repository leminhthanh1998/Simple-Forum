using SimpleForum.Models.Reply;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Post
{
    public class PostIndexModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorID { get; set; }
        public bool IsActive { get; set; }
        public string AuthorImageUrl { get; set; }
        public DateTime Created { get; set; }
        public string PostContent { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; }
        public bool IsAuthorAdmin { get; set; }
        public bool IsMod { get; set; }
        public bool CloseOrOpen { get; set; }
        public bool IsReview { get; set; }
        public string Slug { get; set; }

        public IEnumerable<PostReplyModel> Replies { get; set; }
    }
}
