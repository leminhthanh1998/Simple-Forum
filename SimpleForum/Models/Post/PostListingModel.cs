using SimpleForum.Models.Forum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Post
{
    public class PostListingModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public int AuthorRating { get; set; }
        public string AuthorId { get; set; }
        public string DatePosted { get; set; }
        public string Slug { get; set; }

        //public int ForumID { get; set; }
        //public string ForumImageUrl { get; set; }
        //public string ForumName { get; set; }
        public ForumListingModel Forum { get; set; }
        public int RepliesCount { get; set; }
    }
}
