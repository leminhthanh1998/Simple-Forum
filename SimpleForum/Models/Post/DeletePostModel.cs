using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Post
{
    public class DeletePostModel
    {
        public string PostId { get; set; }
        public int ForumId { get; set; }
        public string PostAuthor { get; set; }
        public string PostContent { get; set; }
        public bool IsActive { get; set; }
    }
}
